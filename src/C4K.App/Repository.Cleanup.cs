using Microsoft.Data.Sqlite;

namespace C4K;

public sealed record CleanupEntry(string VideoId, string Type, DateTimeOffset WatchedAt, DateTimeOffset DeleteAfter,
    DateTimeOffset NextAttempt, string? Error, bool Completed);

public sealed partial class Repository
{
    private void InitializeCleanup()
    {
        using var db = Open(); using var cmd = db.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS watched_cleanup(
                video_id TEXT PRIMARY KEY, type TEXT NOT NULL, watched_at TEXT NOT NULL,
                delete_after TEXT NOT NULL, next_attempt TEXT NOT NULL, error TEXT, completed INTEGER NOT NULL DEFAULT 0);
            """;
        cmd.ExecuteNonQuery();
    }

    private static void ObserveCleanup(SqliteConnection db, SqliteTransaction tx, string type, string id, string action, string? videos)
    {
        var ids = type == "movie" ? new[] { id } : (videos ?? (id.Contains(':') ? id : "")).Split(',');
        var now = DateTimeOffset.UtcNow;
        foreach (var video in ids.Distinct())
        {
            if (!MediaFiles.ValidId(video) || (type == "movie" ? video.Contains(':') :
                video.Split(':').Length != 3 || video.Split(':')[0] != id.Split(':')[0])) continue;
            using var cmd = db.CreateCommand(); cmd.Transaction = tx;
            cmd.CommandText = action == "unwatched" ? "DELETE FROM watched_cleanup WHERE video_id=$id AND type=$type" : """
                INSERT INTO watched_cleanup(video_id,type,watched_at,delete_after,next_attempt)
                VALUES($id,$type,$at,$due,$due) ON CONFLICT(video_id) DO NOTHING;
                """;
            cmd.Parameters.AddWithValue("$id", video); cmd.Parameters.AddWithValue("$type", type);
            if (action == "watched")
            {
                cmd.Parameters.AddWithValue("$at", now.ToString("O"));
                cmd.Parameters.AddWithValue("$due", now.AddDays(3).ToString("O"));
            }
            cmd.ExecuteNonQuery();
        }
    }

    public List<CleanupEntry> CleanupEntries()
    {
        using var db = Open(); using var cmd = db.CreateCommand();
        cmd.CommandText = "SELECT video_id,type,watched_at,delete_after,next_attempt,error,completed FROM watched_cleanup";
        using var r = cmd.ExecuteReader(); var entries = new List<CleanupEntry>();
        while (r.Read()) entries.Add(new(r.GetString(0), r.GetString(1), DateTimeOffset.Parse(r.GetString(2)),
            DateTimeOffset.Parse(r.GetString(3)), DateTimeOffset.Parse(r.GetString(4)), r.IsDBNull(5) ? null : r.GetString(5), r.GetInt32(6) != 0));
        return entries;
    }

    public void CleanupResult(string videoId, DateTimeOffset now, string? error)
    {
        using var db = Open(); using var cmd = db.CreateCommand();
        cmd.CommandText = "UPDATE watched_cleanup SET completed=$done,error=$error,next_attempt=$next WHERE video_id=$id";
        cmd.Parameters.AddWithValue("$id", videoId); cmd.Parameters.AddWithValue("$done", error is null ? 1 : 0);
        cmd.Parameters.AddWithValue("$error", (object?)error ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$next", now.AddHours(1).ToString("O")); cmd.ExecuteNonQuery();
    }
}
