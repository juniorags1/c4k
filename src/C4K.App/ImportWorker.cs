using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace C4K;

public sealed class ImportWorker(Repository repository, MediaFiles files) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        repository.RecoverImports();
        while (!stoppingToken.IsCancellationRequested)
        {
            var job = repository.Imports(pendingOnly: true).LastOrDefault();
            if (job is null) { await Task.Delay(500, stoppingToken); continue; }
            await Import(job, stoppingToken);
        }
    }

    private async Task Import(ImportJob job, CancellationToken token)
    {
        string? destination = null, temporary = null, subtitleDestination = null, subtitleTemporary = null;
        var committed = false;
        try
        {
            repository.ImportState(job.Id, "Importing");
            var request = JsonSerializer.Deserialize<ImportRequest>(job.RequestJson) ?? throw new InvalidDataException();
            var source = LibraryRequests.Source(request.SourcePath) ?? throw new InvalidDataException();
            var subtitle = string.IsNullOrWhiteSpace(request.SubtitlePath) ? null :
                LibraryRequests.Source(request.SubtitlePath, subtitle: true) ?? throw new InvalidDataException();
            var name = job.Id + Path.GetExtension(source).ToLowerInvariant();
            destination = Path.Combine(files.Root, name); temporary = destination + ".partial";
            File.Delete(temporary);
            // Jobs Ready não são processados novamente. Arquivos com estes nomes pertencem apenas a este job.
            File.Delete(destination);
            var sourceInfo = new FileInfo(source);
            if (sourceInfo.Length != job.TotalBytes) throw new InvalidDataException();
            await using var input = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 1024, true);
            var linked = !request.ForceCopy && OperatingSystem.IsWindows() && CreateHardLink(temporary, source, IntPtr.Zero);
            var method = linked ? "Hardlink" : "Copy";
            if (!linked)
            {
                var drive = new DriveInfo(Path.GetPathRoot(files.Root)!);
                if (drive.AvailableFreeSpace < job.TotalBytes + 64L * 1024 * 1024) throw new IOException();
                await using var output = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1024 * 1024, true);
                var buffer = new byte[1024 * 1024]; long copied = 0;
                var lastUpdate = DateTime.UtcNow;
                int count;
                while ((count = await input.ReadAsync(buffer, token)) > 0)
                {
                    await output.WriteAsync(buffer.AsMemory(0, count), token); copied += count;
                    if (DateTime.UtcNow - lastUpdate > TimeSpan.FromMilliseconds(500))
                    { repository.ImportState(job.Id, "Importing", copied, method); lastUpdate = DateTime.UtcNow; }
                }
                await output.FlushAsync(token);
            }
            token.ThrowIfCancellationRequested();
            if (new FileInfo(temporary).Length != job.TotalBytes || File.GetLastWriteTimeUtc(source) != sourceInfo.LastWriteTimeUtc)
                throw new InvalidDataException();
            string? subtitleName = null;
            if (subtitle is not null)
            {
                await using var subtitleInput = new FileStream(subtitle, FileMode.Open, FileAccess.Read, FileShare.Read);
                if (subtitleInput.Length > 5 * 1024 * 1024) throw new InvalidDataException();
                using var reader = new StreamReader(subtitleInput, new UTF8Encoding(false, true), detectEncodingFromByteOrderMarks: false);
                var text = await reader.ReadToEndAsync(token);
                if (string.IsNullOrWhiteSpace(text)) throw new InvalidDataException();
                subtitleName = job.Id + Path.GetExtension(subtitle).ToLowerInvariant();
                subtitleDestination = Path.Combine(files.Root, subtitleName); subtitleTemporary = subtitleDestination + ".partial";
                await File.WriteAllTextAsync(subtitleTemporary, text, new UTF8Encoding(false), token);
                File.Move(subtitleTemporary, subtitleDestination, overwrite: true);
            }
            File.Move(temporary, destination);
            repository.CompleteImport(job, request, name, subtitleName, method); committed = true;
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { repository.ImportState(job.Id, "Queued"); }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException)
        {
            repository.ImportState(job.Id, "Failed", error: error is InvalidDataException or DecoderFallbackException
                ? "Arquivo alterado, vazio ou legenda inválida. Use um arquivo completo e legenda UTF-8."
                : "Não foi possível importar. Confira acesso ao arquivo e espaço livre e tente novamente.");
        }
        finally
        {
            foreach (var path in new[] { temporary, subtitleTemporary, committed ? null : destination, committed ? null : subtitleDestination })
                if (path is not null) try { File.Delete(path); } catch (IOException) { } catch (UnauthorizedAccessException) { }
        }
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CreateHardLink(string link, string target, IntPtr reserved);
}
