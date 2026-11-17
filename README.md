# C4K

C4K is a Windows server and Stremio addon that downloads movies and episodes to a PC and serves completed files to a TV over the home network. Once a title is ready, playback uses the local file rather than relying on torrent seeders during the session.

The current implementation uses ASP.NET Core, SQLite, and the native qBittorrent Web API. Playback has been successfully validated on a TCL C6K running Android TV 14.

## Contents

- [Requirements](#requirements)
- [Initial setup](#initial-setup)
- [Everyday use](#everyday-use)
- [Download automation](#download-automation)
- [Library and storage](#library-and-storage)
- [Automatic cleanup](#automatic-cleanup)
- [Configuration reference](#configuration-reference)
- [Backup and recovery](#backup-and-recovery)
- [Updating the server](#updating-the-server)
- [Troubleshooting](#troubleshooting)
- [Development and repository layout](#development-and-repository-layout)

## Requirements

| Component | Requirement |
| --- | --- |
| Server PC | Windows 11 x64, local storage, and a reliable connection to the home network |
| Downloader | qBittorrent 5.x with its Web UI enabled on the same PC |
| Client | Stremio on a device that can reach the PC over the local network |
| Building from source | .NET SDK specified in [global.json](global.json), currently 10.0.400 |
| Running a published build | No .NET SDK, Node.js, Docker, or external database required |

Keep the PC powered on and awake during downloads and playback. Actual playback performance depends on the file bitrate, PC storage, network, and client codec support. C4K does not transcode video.

Use only media and sources you are authorized to access. The repository does not contain media files, private sources, or credentials.

## Initial setup

### 1. Build the Windows application

From the repository root:

```powershell
dotnet publish src/C4K.App -c Release -r win-x64 --self-contained true -o artifacts/win-x64
```

Keep the entire output directory together. This is a self-contained application folder; a Windows installer is not implemented yet.

### 2. Start qBittorrent and C4K

Enable qBittorrent's Web UI and make it available locally at `http://127.0.0.1:8082`, or choose another local port and configure C4K accordingly.

Start C4K in the background:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Start-C4K.ps1 -Background
```

Open the [dashboard](http://127.0.0.1:48241/admin/), enter the qBittorrent Web UI address and credentials, save, and test the connection. The password field can be left empty when saving later changes to retain the saved password.

### 3. Configure storage and automation

The dashboard lets you change the incomplete and completed download directories. Defaults are:

| Purpose | Default directory |
| --- | --- |
| Incomplete downloads | `C:\media\new` |
| Completed files served by C4K | `C:\media\done` |

The default provider manifest is `https://torrentio.strem.fun/manifest.json`. Configure your authorized provider, then enable automatic downloads when ready. Magnets are sent directly to qBittorrent through its API; no watched folder is needed.

### 4. Connect Stremio

1. Copy the TV installation address from the dashboard and install that manifest in Stremio.
2. Use the LAN address for the TV. An address starting with `127.0.0.1` only works on the PC itself.
3. Open the C4K catalog or an associated title in Stremio to play a completed file.

The dashboard provides separate installation links for the PC and TV. Treat these URLs as private access keys. If the PC's LAN address is incorrect, save the correct address in the dashboard's network settings. A DHCP reservation helps keep the address stable.

The dashboard is currently in Portuguese; this guide describes the purpose of each control in English.

## Everyday use

Run these commands from the repository root.

| Task | Command or location |
| --- | --- |
| Start after reboot | `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Start-C4K.ps1 -Background` |
| Stop the background server | `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Stop-C4K.ps1` |
| Open automation, queue, and events | [Dashboard](http://127.0.0.1:48241/admin/) |
| Search, register, or delete local videos | [Library](http://127.0.0.1:48241/list) |

C4K does not currently install itself as a Windows service or configure automatic startup. After rebooting, start qBittorrent and C4K again. Closing the browser does not stop the background server.

The launcher records its process ID in `artifacts/server.pid`. The stop script only manages that recorded process. If you start C4K directly in a terminal, stop it with `Ctrl+C` instead.

## Download automation

With automatic downloads enabled, C4K processes events received from Stremio:

| Event or action | Behavior |
| --- | --- |
| Add a movie to the Stremio library | Queue that movie |
| Add a new series to the Stremio library | Prepare the first two released regular episodes, usually S01E01 and S01E02 |
| Mark a specific episode as watched | Prepare its next two released regular episodes, including a season transition |
| Start playback | Record the event; playback alone does not trigger episode preloading |
| Mark an item as unwatched | Cancel its pending cleanup; existing downloads remain unchanged |

Episode order and release dates come from Cinemeta. Future or missing release dates are checked again after six hours; metadata lookup failures are retried after ten minutes. At the end of the known episode list, C4K waits for updated metadata rather than inventing a future season.

Repeated events do not duplicate download jobs or move known series progress backward. When a watched event contains multiple episodes, preloading uses the latest regular episode in that batch. A series-level watched event without episode IDs cannot identify where you stopped. Season-zero specials do not anchor preloading.

C4K only reacts to events the Stremio client actually sends. Check the dashboard's received events when diagnosing automation. Existing Stremio library contents and old watched history are not imported automatically.

### Source selection and manual requests

Automatic selection chooses the highest reported seed count among recognizable 4K/2160p sources with a torrent hash. A reported count does not guarantee available peers or a successful download. Sources without a usable seed count are not selected automatically.

Use the dashboard to request an individual movie or episode by IMDb ID, or to supply a magnet manually:

- Movie: `tt1234567`.
- Episode: `tt1234567:1:2` for season 1, episode 2.
- Multi-video torrent: provide the exact zero-based video file index when selection is ambiguous.

C4K does not silently pick the largest video in a package. Titles and years are resolved through Cinemeta when available; display names do not rename physical files.

### Failed downloads

A failed job offers two actions:

- **Retry** attempts the download again.
- **Cancel** removes it from the displayed queue without deleting files or changing the existing qBittorrent task.

Cancellation survives restarts. Automatic events do not reactivate a canceled job; an explicit manual request can queue it again. Because canceling only affects C4K's queue, inspect qBittorrent separately if you also want to stop or remove its torrent.

## Library and storage

The library page supports search, movie/episode filters, size sorting, file availability, manual registration, and confirmed deletion. Total size counts repeated file paths only once.

C4K registers completed videos directly from the configured completed directory, including subdirectories. It does not create a second copy. Supported video extensions are MP4, MKV, WEBM, and M4V. Files outside that directory, network paths, symbolic links, and junctions are rejected.

For manual registration, provide the full path to an already completed file, a title, and optionally its IMDb ID. Select the season and episode for series entries. IMDb-backed items receive posters from MetaHub; poster loading requires internet access or a Stremio cache, while playback uses the local video.

A downloaded file becomes available only after completion is confirmed and the torrent is no longer moving or being checked. For a package, all selected files must be complete. Multiple requested episodes can share the same torrent while retaining their selected file indexes.

### Change download directories

1. Finish or resolve pending C4K download/import tasks before changing the completed directory.
2. Move or copy registered videos and subtitles to the new location, preserving relative paths and subdirectories.
3. Update existing torrent locations in qBittorrent as needed.
4. Save the new directories in the C4K dashboard.

C4K validates registered files before accepting a new completed directory. Saving a directory does not move files, change existing torrent locations, or change qBittorrent's global preferences. New C4K torrent tasks receive the configured incomplete and completed paths.

### Delete a video manually

Deleting through the library removes the physical video and its catalog entry, not just the entry. Unshared registered subtitles are also removed. C4K removes its own torrent task with `deleteFiles=false` before deleting the specific file; it does not delete an entire directory or unrelated package contents.

A working qBittorrent connection is required to check ownership. Files in use, externally managed torrents, and shared items can block deletion.

## Automatic cleanup

**A new individual movie or episode `watched` event schedules physical deletion after 72 hours.** This runs even when automatic downloads are disabled.

- The deadline starts when C4K receives the first watched event. Duplicate events do not postpone it.
- Marking the item as `unwatched` cancels its pending cleanup. A later watched event starts a new deadline.
- Schedules survive restarts. If the PC is off at the deadline, cleanup runs when C4K is available again.
- The dashboard shows pending deadlines and errors. Failed cleanup is retried after one hour.
- Files in use and files belonging to external torrents are preserved. Shared torrent packages wait until the other managed items are ready and their deadlines have elapsed.
- Old watched history is not processed retroactively. A generic series-level mark without episode IDs does not schedule deletion of the entire series.

Disabling downloads does not disable cleanup. To keep a scheduled item, mark it as unwatched before its deadline and check that C4K received the event.

## Configuration reference

### Network endpoints

| Service | Default address | Access |
| --- | --- | --- |
| Dashboard | `http://127.0.0.1:48241/admin/` | Server PC only |
| Library | `http://127.0.0.1:48241/list` | Server PC only |
| Addon and media | TCP port `48240` | Home network |
| qBittorrent Web UI | `http://127.0.0.1:8082` | Local API connection |

The TV manifest has the form `http://PC-LAN-IP:48240/PRIVATE-CODE/manifest.json`. Copy the actual URL from the dashboard rather than constructing it yourself.

Allow C4K through Windows Firewall only on the private home network. C4K does not configure the firewall or router automatically. Do not forward these ports to the internet. LAN traffic uses HTTP and is not encrypted; no certificate or tunnel is required for this setup.

### Environment overrides

Set these before starting the server. A running process does not pick up later environment changes.

| Variable | Default or fallback | Purpose |
| --- | --- | --- |
| `C4K_DATA_DIR` | `%LOCALAPPDATA%\C4K` | Database, credentials, and saved settings |
| `C4K_MEDIA_ROOT` | Saved library directory, then `C:\media\done` | Completed-file root; locks dashboard changes while set |
| `C4K_ADDON_PORT` | `48240` | Addon and media listener |
| `C4K_ADMIN_PORT` | `48241` | Loopback-only dashboard listener |
| `C4K_PUBLIC_URL` | Saved network address, then LAN detection | Advertised HTTP address; overrides dashboard network settings |

The addon and admin ports must be different and between 1024 and 65535. A public URL override must use the configured addon port.

Prefer saving the LAN address through the dashboard. Use an environment override when needed, for example with multiple interfaces or a VPN:

```powershell
$env:C4K_PUBLIC_URL = 'http://192.168.1.50:48240' # Replace with this PC's LAN address.
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Start-C4K.ps1 -Background
```

## Backup and recovery

Application state lives in `%LOCALAPPDATA%\C4K` by default, independently of the executable folder.

| File | Contents |
| --- | --- |
| `c4k.db` | Library, jobs, received events, episode plans, and cleanup schedules |
| `secrets.json` | Private addon and media access tokens |
| `automation.json` | Provider, qBittorrent settings, protected password, and incomplete directory |
| `library.json` | Saved completed directory, when configured through the dashboard |
| `network.json` | Saved LAN address, when configured through the dashboard |

To back up, stop C4K and copy the entire data directory, including any SQLite companion files present. Back up completed and incomplete media directories separately. For an ongoing qBittorrent download, also preserve qBittorrent's own configuration and resume data; the C4K database is not a replacement for them.

Restore with C4K stopped, retain the expected media paths, then restart and check the dashboard. qBittorrent credentials use Windows DPAPI tied to the Windows user: moving data to another account or PC may require entering the password again. Keeping `secrets.json` preserves the existing addon links.

Never commit the data directory or backup files. If installation links are exposed, generate new links in the dashboard and reinstall the addon on the clients. This revokes the old links while retaining the library files.

## Updating the server

Back up application data first. If the server was started by the launcher:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Stop-C4K.ps1
dotnet publish src/C4K.App -c Release -r win-x64 --self-contained true -o artifacts/win-x64
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Start-C4K.ps1 -Background
```

Check that publishing succeeded before starting again. If using another launch method, stop that process before replacing its executable files. Updating the application folder does not require deleting the data directory or generating new addon links.

## Troubleshooting

| Symptom | What to check |
| --- | --- |
| Dashboard does not open | Start C4K; verify that the configured admin port is free and use the browser on the server PC. |
| TV cannot connect | Confirm the PC is awake, both devices share the LAN, the manifest uses the current PC LAN address, and the firewall allows the addon port on the private network. |
| qBittorrent connection fails | Confirm qBittorrent is running, its Web UI is enabled, and the saved local URL, port, and credentials are correct. Test the saved connection in the dashboard. |
| No download follows a Stremio action | Look for the event in the dashboard, check automation is enabled, and ensure the event identifies the movie or episode. Old library/history entries are not automatically imported. |
| An upcoming episode is not queued | Check release metadata. Future or missing dates wait for another check; metadata errors have a separate retry delay. |
| Download fails | Read its error, inspect qBittorrent, and use Retry after correcting the issue. Cancel only hides/cancels the C4K job. |
| Completed file is missing from Stremio | Check the C4K library, torrent completion, selected files, and whether qBittorrent is moving or checking the content. Refresh Stremio's catalog. |
| Directory change is rejected | Resolve pending tasks, place registered files and subtitles under the new root, and check whether `C4K_MEDIA_ROOT` is set. |
| Cleanup or manual deletion fails | Check qBittorrent connectivity, external/shared torrent ownership, file availability, and whether the video is in use. Review the cleanup error in the dashboard. |
| A canceled torrent keeps downloading | Cancellation only changes the C4K queue. Manage the existing torrent directly in qBittorrent. |

### Stremio installation reports "Failed to fetch"

Use the dashboard's browser installation option and allow local network access for `web.stremio.com` if prompted. If access was denied earlier, review that site's permissions in Edge or Chrome. Do not disable browser security globally.

A successful request in PowerShell does not prove that a browser or WebView has permission to access the LAN. Use the PC-specific link on the PC and the LAN link on the TV. If using account synchronization, sign in to the intended Stremio account on both devices.

### Inspect received events

The dashboard displays the latest 100 events; the database retains up to 5,000. Received events help distinguish a client that did not send a notification from a download or metadata failure. Player changes or another Stremio client may affect which events are sent.

## Development and repository layout

To run from source in a terminal:

```powershell
dotnet run --project src/C4K.App
```

Stop that process with `Ctrl+C`. Do not run it alongside the published server on the same ports.

To validate a change:

```powershell
dotnet restore C4K.slnx --locked-mode
dotnet build C4K.slnx -c Release --no-restore
dotnet run --project tests/C4K.LibraryTests -c Release --no-build
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Smoke-Test.ps1
```

After publishing, validate the distribution with:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Smoke-Test.ps1 -Published
```

The complete suite runs on Windows because credential protection uses DPAPI. Tests use isolated fixtures and simulated APIs; HTTP checks are not a substitute for real playback testing. GitHub Actions builds, runs the checks, and validates a self-contained Windows publication on pushes and pull requests.

| Path | Purpose |
| --- | --- |
| `src/C4K.App/` | Server, Stremio addon, dashboard, library, and downloader integration |
| `tests/C4K.LibraryTests/` | Persistence, automation, and file-operation checks |
| `scripts/` | Start/stop launchers and HTTP smoke checks |
| `.github/workflows/ci.yml` | Windows CI workflow |
| `.planning/` | Historical decisions, research, and implementation records; older documents may describe superseded behavior |

See [CONTRIBUTING.md](CONTRIBUTING.md) for contribution instructions and [SECURITY.md](SECURITY.md) for access and data protection. No distribution license has been selected yet.
