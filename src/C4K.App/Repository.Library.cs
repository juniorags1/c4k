using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace C4K;

public sealed record ImportJob(string Id, string VideoId, string Title, string State, long TotalBytes, long CopiedBytes,
    string? Method, string? Error, [property: JsonIgnore] string RequestJson);

public sealed partial class Repository
{
    private void InitializeLibrary()
    {
        using var db = Open(); using var cmd = db.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS subtitles(media_id TEXT PRIMARY KEY, file_name TEXT NOT NULL, language TEXT NOT NULL);
            CREATE TABLE IF NOT EXISTS imports(
                id TEXT PRIMARY KEY, fingerprint TEXT NOT NULL UNIQUE, video_id TEXT NOT NULL, title TEXT NOT NULL,
                state TEXT NOT NULL, total_bytes INTEGER NOT NULL, copied_bytes INTEGER NOT NULL DEFAULT 0,
                method TEXT, error TEXT, request_json TEXT NOT NULL, created_at TEXT NOT NULL);
            """;
        cmd.ExecuteNonQuery();
    }

    public ImportJob QueueImport(ImportRequest input)
    {
        var json = JsonSerializer.Serialize(input);
        var info = new FileInfo(input.SourcePath);
        var subtitle = string.IsNullOrWhiteSpace(input.SubtitlePath) ? null : new FileInfo(input.SubtitlePath);
        var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
            $"{json}|{info.Length}|{info.LastWriteTimeUtc.Ticks}|{subtitle?.Length}|{subtitle?.LastWriteTimeUtc.Ticks}")));
        using var db = Open(); using var tx = db.BeginTransaction(); using var cmd = db.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = "SELECT id FROM imports WHERE fingerprint=$fingerprint";
        cmd.Parameters.AddWithValue("$fingerprint", fingerprint);
        var existing = cmd.ExecuteScalar() as string;
        var id = existing ?? Guid.NewGuid().ToString("N");
        if (existing is null)
        {
            cmd.CommandText = """
                INSERT INTO imports(id,fingerprint,video_id,title,state,total_bytes,request_json,created_at)
                VALUES($id,$fingerprint,$video,$title,'Queued',$size,$json,$at);
                """;
            cmd.Parameters.AddWithValue("$id", id); cmd.Parameters.AddWithValue("$video", input.Media.VideoId);
            cmd.Parameters.AddWithValue("$title", input.Media.Title); cmd.Parameters.AddWithValue("$size", info.Length);
            cmd.Parameters.AddWithValue("$json", json); cmd.Parameters.AddWithValue("$at", DateTimeOffset.UtcNow.ToString("O"));
            cmd.ExecuteNonQuery();
        }
        if (existing is not null)
        {
            cmd.CommandText = "UPDATE imports SET state='Queued',error=NULL WHERE id=$existing AND state='Removed'";
            cmd.Parameters.AddWithValue("$existing", existing); cmd.ExecuteNonQuery();
        }
        tx.Commit();
        return Imports(id).Single();
    }

    public List<ImportJob> Imports(string? id = null, bool pendingOnly = false)
    {
        using var db = Open(); using var cmd = db.CreateCommand();
        cmd.CommandText = "SELECT id,video_id,title,state,total_bytes,copied_bytes,method,error,request_json FROM imports " +
            (id is not null ? "WHERE id=$id " : pendingOnly ? "WHERE state='Queued' " : "") + "ORDER BY created_at DESC LIMIT 500";
        if (id is not null) cmd.Parameters.AddWithValue("$id", id);
        using var r = cmd.ExecuteReader(); var result = new List<ImportJob>();
        while (r.Read()) result.Add(new(r.GetString(0), r.GetString(1), r.GetString(2), r.GetString(3), r.GetInt64(4), r.GetInt64(5),
            r.IsDBNull(6) ? null : r.GetString(6), r.IsDBNull(7) ? null : r.GetString(7), r.GetString(8)));
        return result;
    }

    public void RecoverImports()
    {
        using var db = Open(); using var cmd = db.CreateCommand();
        cmd.CommandText = "UPDATE imports SET state='Queued',copied_bytes=0 WHERE state='Importing'"; cmd.ExecuteNonQuery();
    }

    public bool CanChangeRoot()
    {
        using var db = Open(); using var cmd = db.CreateCommand();
        cmd.CommandText = "SELECT (SELECT COUNT(*) FROM media)+(SELECT COUNT(*) FROM imports WHERE state IN ('Queued','Importing'))";
        return Convert.ToInt64(cmd.ExecuteScalar()) == 0;
    }

    public void ImportState(string id, string state, long copied = 0, string? method = null, string? error = null)
    {
        using var db = Open(); using var cmd = db.CreateCommand();
        cmd.CommandText = "UPDATE imports SET state=$state,copied_bytes=$copied,method=$method,error=$error WHERE id=$id";
        cmd.Parameters.AddWithValue("$id", id); cmd.Parameters.AddWithValue("$state", state); cmd.Parameters.AddWithValue("$copied", copied);
        cmd.Parameters.AddWithValue("$method", (object?)method ?? DBNull.Value); cmd.Parameters.AddWithValue("$error", (object?)error ?? DBNull.Value);
        cmd.ExecuteNonQuery();
    }

    public bool RetryImport(string id)
    {
        using var db = Open(); using var cmd = db.CreateCommand();
        cmd.CommandText = "UPDATE imports SET state='Queued',error=NULL,copied_bytes=0 WHERE id=$id AND state='Failed'";
        cmd.Parameters.AddWithValue("$id", id); return cmd.ExecuteNonQuery() == 1;
    }

    public LocalSubtitle? Subtitle(string id)
    {
        using var db = Open(); using var cmd = db.CreateCommand();
        cmd.CommandText = "SELECT file_name,language FROM subtitles WHERE media_id=$id"; cmd.Parameters.AddWithValue("$id", id);
        using var r = cmd.ExecuteReader(); return r.Read() ? new(r.GetString(0), r.GetString(1)) : null;
    }

    public void CompleteImport(ImportJob job, ImportRequest request, string fileName, string? subtitleName, string method)
    {
        using var db = Open(); using var tx = db.BeginTransaction(); using var cmd = db.CreateCommand(); cmd.Transaction = tx;
        cmd.CommandText = """
            INSERT INTO media(id,type,meta_id,video_id,title,file_name,season,episode)
            VALUES($id,$type,$meta,$video,$title,$file,$season,$episode)
            ON CONFLICT(video_id) DO UPDATE SET type=$type,meta_id=$meta,title=$title,file_name=$file,season=$season,episode=$episode;
            DELETE FROM subtitles WHERE media_id=(SELECT id FROM media WHERE video_id=$video);
            INSERT INTO subtitles(media_id,file_name,language)
            SELECT id,$subtitle,$lang FROM media WHERE video_id=$video AND $subtitle IS NOT NULL;
            UPDATE imports SET state='Ready',copied_bytes=total_bytes,method=$method,error=NULL WHERE id=$id;
            """;
        var item = request.Media;
        cmd.Parameters.AddWithValue("$id", job.Id); cmd.Parameters.AddWithValue("$type", item.Type);
        cmd.Parameters.AddWithValue("$meta", item.MetaId); cmd.Parameters.AddWithValue("$video", item.VideoId);
        cmd.Parameters.AddWithValue("$title", item.Title); cmd.Parameters.AddWithValue("$file", fileName);
        cmd.Parameters.AddWithValue("$season", (object?)item.Season ?? DBNull.Value); cmd.Parameters.AddWithValue("$episode", (object?)item.Episode ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$subtitle", (object?)subtitleName ?? DBNull.Value); cmd.Parameters.AddWithValue("$lang", request.SubtitleLang ?? "por");
        cmd.Parameters.AddWithValue("$method", method); cmd.ExecuteNonQuery(); tx.Commit();
    }

    public bool RemoveMedia(string id)
    {
        using var db = Open(); using var tx = db.BeginTransaction(); using var cmd = db.CreateCommand(); cmd.Transaction = tx;
        cmd.CommandText = """
            UPDATE imports SET state='Removed' WHERE video_id=(SELECT video_id FROM media WHERE id=$id) AND state='Ready';
            DELETE FROM subtitles WHERE media_id=$id; DELETE FROM media WHERE id=$id;
            """;
        cmd.Parameters.AddWithValue("$id", id); var removed = cmd.ExecuteNonQuery() > 0; tx.Commit(); return removed;
    }
}
