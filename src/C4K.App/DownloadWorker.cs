using System.Text.Json;

namespace C4K;

public sealed class DownloadWorker(Repository repository, MediaFiles files, AutomationSettings settings, Func<QbitClient>? clientFactory = null) : BackgroundService
{
    private QbitClient Client() => clientFactory?.Invoke() ?? new QbitClient(settings.Current.QbitUrl);
    private string TorrentDisplay(DownloadJob job) => repository.Downloads().Any(j => j.Id != job.Id &&
        j.Hash == job.Hash && j.State != "Removed") && job.Request.Type == "series"
        ? (job.Request.Title ?? job.Request.VideoId.Split(':')[0]) + " — episódios" : MediaNames.Display(job.Request);
    public SemaphoreSlim Gate { get; } = new(1, 1);
    public string ConnectionStatus { get; private set; } = "Configure e teste a conexão com o qBittorrent.";
    private readonly HttpClient sourceHttp = new() { Timeout = TimeSpan.FromSeconds(40), MaxResponseContentBufferSize = 8 * 1024 * 1024 };
    private AutomationOptions? rejectedSettings;
    public async Task<string> Test(CancellationToken ct)
    {
        using var qbit = Client();
        var version = await qbit.Connect(settings.Current.Username, settings.Password, ct);
        if (!version.StartsWith("v5.", StringComparison.Ordinal)) throw new InvalidOperationException("Integração requer qBittorrent 5.x.");
        ConnectionStatus = "Conectado: qBittorrent " + version; return ConnectionStatus;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Gate.WaitAsync(stoppingToken);
                try
                {
                    if (settings.Current.Enabled && !ReferenceEquals(rejectedSettings, settings.Current))
                    {
                        foreach (var plan in repository.WatchedPlans().Where(p => (p.Anchor is not null || p.FromBeginning) && p.NextAttempt <= DateTimeOffset.UtcNow))
                        {
                            try
                            {
                                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken); timeout.CancelAfter(TimeSpan.FromSeconds(8));
                                using var response = await sourceHttp.GetAsync($"https://v3-cinemeta.strem.io/meta/series/{plan.SeriesId}.json", timeout.Token);
                                response.EnsureSuccessStatusCode();
                                using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(timeout.Token));
                                repository.ApplyWatched(plan, plan.FromBeginning ? EpisodePlanner.First(json.RootElement, plan.SeriesId, DateTimeOffset.UtcNow)
                                    : EpisodePlanner.Next(json.RootElement, plan.SeriesId, plan.Anchor!, DateTimeOffset.UtcNow));
                            }
                            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { throw; }
                            catch (Exception e) { repository.ApplyWatched(plan, null, Friendly(e)); }
                        }
                        using var qbit = Client();
                        var version = await qbit.Connect(settings.Current.Username, settings.Password, stoppingToken);
                        if (!version.StartsWith("v5.", StringComparison.Ordinal)) throw new InvalidOperationException("Integração requer qBittorrent 5.x.");
                        ConnectionStatus = "Conectado: qBittorrent " + version;
                        foreach (var job in repository.Downloads().Where(j => j.State is "Queued" or "Resolving" or "Downloading"))
                        {
                            try { await Process(job, qbit, stoppingToken); }
                            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { throw; }
                            catch (Exception e) { var latest = repository.Downloads().Single(j => j.Id == job.Id);
                                repository.SaveDownload(latest with { State = "Failed", Error = Friendly(e) }); }
                        }
                    }
                }
                finally { Gate.Release(); }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception e) { ConnectionStatus = Friendly(e);
                if (e is InvalidOperationException) rejectedSettings = settings.Current; }
            try { await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); } catch (OperationCanceledException) { break; }
        }
    }
    public static string Friendly(Exception e) => e is InvalidOperationException ? e.Message
        : e is HttpRequestException or TaskCanceledException ? "Falha de conexão com qBittorrent ou provedor. Confira a configuração e tente novamente."
        : "Falha local. Verifique arquivo, permissões e espaço em disco.";

    public async Task Process(DownloadJob original, QbitClient qbit, CancellationToken ct)
    {
        var job = original;
        if (string.IsNullOrWhiteSpace(job.Request.Title))
        {
            var title = await MediaNames.Resolve(sourceHttp, job.Request, ct);
            if (title is not null) { job = job with { Request = job.Request with { Title = title } }; repository.SaveDownload(job); }
        }
        if (job.Hash is null)
        {
            var candidate = job.Request.Magnet is { } manual
                ? new TorrentCandidate(TorrentSource.MagnetHash(manual), job.Request.FileIndex, -1, manual)
                : await TorrentSource.Resolve(sourceHttp, settings.Current.Provider, job.Request, ct);
            job = job with { Hash = candidate.Hash, FileIndex = candidate.FileIndex, Seeds = candidate.Seeds < 0 ? null : candidate.Seeds,
                Request = job.Request with { Magnet = candidate.Magnet }, State = "Resolving", Error = null };
            repository.SaveDownload(job); // Hash durável antes da chamada remota; recuperação consulta antes de reenviar.
        }
        var torrents = await qbit.Get("torrents/info?hashes=" + job.Hash, ct);
        if (torrents.GetArrayLength() == 0)
        {
            if (job.State == "Downloading") throw new InvalidOperationException("Torrent removido do qBittorrent. Use tentar novamente para adicioná-lo de novo.");
            var magnet = job.Request.Magnet!;
            if (job.Request.Title is not null && !Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(magnet[7..]).ContainsKey("dn"))
                magnet += "&dn=" + Uri.EscapeDataString(MediaNames.Display(job.Request));
            await qbit.Post("torrents/add", new() { ["urls"] = magnet, ["savepath"] = files.Root,
                ["downloadPath"] = settings.Current.NewDirectory, ["useDownloadPath"] = "true", ["autoTMM"] = "false",
                ["tags"] = "C4K", ["rename"] = MediaNames.Display(job.Request), ["stopped"] = "true", ["stopCondition"] = "MetadataReceived" }, ct);
            return;
        }
        var torrent = torrents[0];
        if (!torrent.GetProperty("tags").GetString()!.Split(',').Any(t => t.Trim() == "C4K"))
            throw new InvalidOperationException("Este torrent já existe fora do C4K. Não alteramos tarefas externas.");
        if (job.Request.Title is not null && torrent.TryGetProperty("name", out var torrentName) && torrentName.GetString() != TorrentDisplay(job))
            await qbit.Post("torrents/rename", new() { ["hash"] = job.Hash!, ["name"] = TorrentDisplay(job) }, ct);
        if (!string.Equals(Path.TrimEndingDirectorySeparator(Path.GetFullPath(torrent.GetProperty("save_path").GetString()!)), Path.TrimEndingDirectorySeparator(files.Root), StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Torrent fora da pasta done configurada.");
        var contents = await qbit.Get("torrents/files?hash=" + job.Hash, ct);
        if (contents.GetArrayLength() == 0)
        {
            repository.SaveDownload(job with { State = "Resolving", Error = null });
            // stopCondition interrompe automaticamente após obter metadados, antes dos dados do vídeo.
            await qbit.Post("torrents/start", new() { ["hashes"] = job.Hash! }, ct); return;
        }
        var videos = contents.EnumerateArray().Where(f => Path.GetExtension(f.GetProperty("name").GetString()!).ToLowerInvariant() is ".mp4" or ".mkv" or ".webm" or ".m4v").ToList();
        var selected = job.FileIndex is { } idx ? videos.Where(f => f.GetProperty("index").GetInt32() == idx).ToList() : videos;
        if (selected.Count != 1) throw new InvalidOperationException("Pacote ambíguo. Informe o índice exato do vídeo em uma nova solicitação manual.");
        var target = selected[0]; var fileIndex = target.GetProperty("index").GetInt32();
        if (!job.Selected)
        {
            // Dois episódios podem compartilhar o pacote: preservar a união dos índices solicitados.
            var wanted = repository.Downloads().Where(j => j.Hash == job.Hash && j.State != "Removed" && j.FileIndex is not null)
                .Select(j => j.FileIndex!.Value).Append(fileIndex).ToHashSet();
            wanted.IntersectWith(videos.Select(v => v.GetProperty("index").GetInt32()));
            await qbit.Post("torrents/stop", new() { ["hashes"] = job.Hash! }, ct);
            var other = contents.EnumerateArray().Where(f => !wanted.Contains(f.GetProperty("index").GetInt32())).Select(f => f.GetProperty("index").GetInt32().ToString()).ToArray();
            if (other.Length > 0) await qbit.Post("torrents/filePrio", new() { ["hash"] = job.Hash!, ["id"] = string.Join('|', other), ["priority"] = "0" }, ct);
            await qbit.Post("torrents/filePrio", new() { ["hash"] = job.Hash!, ["id"] = string.Join('|', wanted.Order()), ["priority"] = "1" }, ct);
            job = job with { Selected = true, FileIndex = fileIndex, State = "Downloading" }; repository.SaveDownload(job);
            await qbit.Post("torrents/start", new() { ["hashes"] = job.Hash! }, ct); return;
        }
        var progress = target.GetProperty("progress").GetDouble();
        var state = torrent.GetProperty("state").GetString()!;
        if (state is "error" or "missingFiles" or "unknown") throw new InvalidOperationException("qBittorrent informou erro no torrent. Confira disco e arquivo.");
        repository.SaveDownload(job with { Progress = progress, Error = null });
        if (progress < 1 && state == "stoppedDL")
            await qbit.Post("torrents/start", new() { ["hashes"] = job.Hash! }, ct);
        if (progress < 1 || torrent.GetProperty("amount_left").GetInt64() != 0 || state.Contains("checking", StringComparison.OrdinalIgnoreCase) || state == "moving") return;
        var name = target.GetProperty("name").GetString()!;
        var path = files.Resolve(name);
        if (path is null || new FileInfo(path).Length != target.GetProperty("size").GetInt64()) return;
        using (File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read)) { }
        var parts = job.Request.VideoId.Split(':');
        repository.Register(new(Guid.NewGuid().ToString("N"), job.Request.Type, parts[0], job.Request.VideoId,
            job.Request.Title ?? Path.GetFileNameWithoutExtension(path), name, parts.Length == 3 ? int.Parse(parts[1]) : null, parts.Length == 3 ? int.Parse(parts[2]) : null));
        repository.SaveDownload(job with { State = "Ready", Progress = 1, Error = null });
    }

    public async Task<bool> Delete(string id, CancellationToken ct)
    {
        await Gate.WaitAsync(ct);
        try
        {
            var item = repository.Media().FirstOrDefault(m => m.Id == id); if (item is null) return false;
            var path = files.Resolve(item.FileName) ?? throw new InvalidOperationException("Arquivo ausente ou fora da pasta válida. Catálogo mantido.");
            if (repository.Media().Any(m => m.Id != id && string.Equals(m.FileName, item.FileName, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Arquivo compartilhado por outro item do catálogo.");
            var job = repository.Downloads().FirstOrDefault(j => j.Request.VideoId == item.VideoId && j.Request.Type == item.Type);
            using var qbit = Client();
            // Mesmo cadastros manuais podem pertencer a torrents: não apagar sob uma tarefa externa ativa.
            await qbit.Connect(settings.Current.Username, settings.Password, ct);
            var torrents = await qbit.Get("torrents/info", ct);
            foreach (var torrent in torrents.EnumerateArray())
            {
                var hash = torrent.GetProperty("hash").GetString()!;
                var entries = await qbit.Get("torrents/files?hash=" + hash, ct);
                var save = torrent.GetProperty("save_path").GetString()!;
                var ownsPath = entries.EnumerateArray().Any(f => string.Equals(Path.GetFullPath(Path.Combine(save, f.GetProperty("name").GetString()!)), path, StringComparison.OrdinalIgnoreCase));
                if (!ownsPath) continue;
                if (job?.Hash != hash || !torrent.GetProperty("tags").GetString()!.Split(',').Any(t => t.Trim() == "C4K"))
                    throw new InvalidOperationException("Arquivo pertence a uma tarefa externa do qBittorrent. Remova essa tarefa mantendo os arquivos antes de apagar pelo C4K.");
                if (repository.Downloads().Any(j => j.Id != job.Id && j.Hash == hash && j.State != "Removed"))
                    throw new InvalidOperationException("Outro item usa o mesmo torrent.");
                await qbit.Post("torrents/delete", new() { ["hashes"] = hash, ["deleteFiles"] = "false" }, ct);
            }
            // Abre sem compartilhar: impede exclusão durante reprodução local ativa.
            using (File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) { }
            File.Delete(path);
            repository.RemoveMedia(id);
            if (job is not null) repository.SaveDownload(job with { State = "Removed" });
            return true;
        }
        finally { Gate.Release(); }
    }
    public override void Dispose() { sourceHttp.Dispose(); Gate.Dispose(); base.Dispose(); }
}
