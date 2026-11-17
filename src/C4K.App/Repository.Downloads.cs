using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace C4K;

public sealed record DownloadRequest(string Type, string VideoId, string? Title = null, string? Magnet = null, int? FileIndex = null);
public sealed record DownloadJob(string Id, DownloadRequest Request, string State = "Queued", string? Hash = null,
    int? FileIndex = null, int? Seeds = null, double Progress = 0, string? Error = null, bool Selected = false);

public sealed partial class Repository
{
    private void InitializeDownloads()
    {
        using var db = Open(); using var cmd = db.CreateCommand();
        cmd.CommandText = "CREATE TABLE IF NOT EXISTS downloads(id TEXT PRIMARY KEY, body TEXT NOT NULL)"; cmd.ExecuteNonQuery();
    }
    public List<DownloadJob> Downloads()
    {
        using var db = Open(); using var cmd = db.CreateCommand(); cmd.CommandText = "SELECT body FROM downloads ORDER BY rowid DESC";
        using var r = cmd.ExecuteReader(); var result = new List<DownloadJob>();
        while (r.Read()) result.Add(JsonSerializer.Deserialize<DownloadJob>(r.GetString(0))!); return result;
    }
    public DownloadJob QueueDownload(DownloadRequest request)
    {
        request = NormalizeDownload(request);
        using var db = Open(); using var tx = db.BeginTransaction(); InsertDownload(db, tx, request); tx.Commit();
        var existing = Downloads().Single(j => j.Id == request.Type + ":" + request.VideoId);
        if (existing.State is "Failed" or "Removed" or "Cancelled")
        {
            existing = new DownloadJob(existing.Id, request);
            SaveDownload(existing);
        }
        return existing;
    }
    public static DownloadRequest NormalizeDownload(DownloadRequest request)
    {
        if (request.Type is not ("movie" or "series") || !System.Text.RegularExpressions.Regex.IsMatch(request.VideoId,
            request.Type == "movie" ? @"^tt\d{5,12}$" : @"^tt\d{5,12}(:\d{1,4}:\d{1,4})?$") || request.Title?.Length > 200 || request.FileIndex < 0)
            throw new ArgumentException("Informe IMDb e episódio válidos.");
        if (request.Type == "series" && !request.VideoId.Contains(':')) request = request with { VideoId = request.VideoId + ":1:1" };
        if (request.Magnet is not null) TorrentSource.MagnetHash(request.Magnet);
        return request;
    }
    private static void InsertDownload(SqliteConnection db, SqliteTransaction tx, DownloadRequest request)
    {
        using var cmd = db.CreateCommand(); cmd.Transaction = tx;
        var job = new DownloadJob(request.Type + ":" + request.VideoId, request);
        cmd.CommandText = "INSERT OR IGNORE INTO downloads(id,body) VALUES($id,$body)";
        cmd.Parameters.AddWithValue("$id", job.Id); cmd.Parameters.AddWithValue("$body", JsonSerializer.Serialize(job)); cmd.ExecuteNonQuery();
    }
    // Called under DownloadWorker.Gate by the administrative endpoint.
    public bool CancelFailedDownload(string id)
    {
        var job = Downloads().FirstOrDefault(j => j.Id == id);
        if (job is null || job.State is not ("Failed" or "Cancelled")) return false;
        SaveDownload(job with { State = "Cancelled", Error = null });
        return true;
    }
    public void SaveDownload(DownloadJob job)
    {
        using var db = Open(); using var cmd = db.CreateCommand(); cmd.CommandText = "UPDATE downloads SET body=$body WHERE id=$id";
        cmd.Parameters.AddWithValue("$id", job.Id); cmd.Parameters.AddWithValue("$body", JsonSerializer.Serialize(job)); cmd.ExecuteNonQuery();
    }
}
