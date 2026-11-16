using Microsoft.Data.Sqlite;

namespace C4K;

public sealed record MediaItem(string Id, string Type, string MetaId, string VideoId, string Title, string FileName, int? Season, int? Episode);
public sealed record Activity(long Id, string Resource, string Type, string VideoId, string Action, long? CurrentTime, long? Duration, string? ChangedVideos, string ReceivedAt);

public sealed partial class Repository
{
    private readonly string connectionString;
    public Repository(string directory)
    {
        connectionString = new SqliteConnectionStringBuilder { DataSource = Path.Combine(directory, "c4k.db"), DefaultTimeout = 10 }.ToString();
        using var db = Open();
        using var command = db.CreateCommand();
        command.CommandText = """
            PRAGMA journal_mode=WAL;
            CREATE TABLE IF NOT EXISTS media (
                id TEXT PRIMARY KEY, type TEXT NOT NULL, meta_id TEXT NOT NULL, video_id TEXT NOT NULL UNIQUE,
                title TEXT NOT NULL, file_name TEXT NOT NULL, season INTEGER, episode INTEGER);
            CREATE TABLE IF NOT EXISTS activity (
                id INTEGER PRIMARY KEY AUTOINCREMENT, resource TEXT NOT NULL, type TEXT NOT NULL,
                video_id TEXT NOT NULL, action TEXT NOT NULL, current_time INTEGER, duration INTEGER,
                changed_videos TEXT, received_at TEXT NOT NULL);
            """;
        command.ExecuteNonQuery();
        InitializeLibrary();
    }

    private SqliteConnection Open() { var db = new SqliteConnection(connectionString); db.Open(); return db; }

    public List<MediaItem> Media()
    {
        using var db = Open(); using var cmd = db.CreateCommand();
        cmd.CommandText = "SELECT id,type,meta_id,video_id,title,file_name,season,episode FROM media ORDER BY title,season,episode";
        using var reader = cmd.ExecuteReader(); var items = new List<MediaItem>();
        while (reader.Read()) items.Add(new(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.IsDBNull(6) ? null : reader.GetInt32(6), reader.IsDBNull(7) ? null : reader.GetInt32(7)));
        return items;
    }

    public void Register(MediaItem item)
    {
        using var db = Open(); using var cmd = db.CreateCommand();
        cmd.CommandText = """
            INSERT INTO media VALUES ($id,$type,$meta,$video,$title,$file,$season,$episode)
            ON CONFLICT(video_id) DO UPDATE SET type=$type,meta_id=$meta,title=$title,file_name=$file,season=$season,episode=$episode;
            """;
        cmd.Parameters.AddWithValue("$id", item.Id); cmd.Parameters.AddWithValue("$type", item.Type);
        cmd.Parameters.AddWithValue("$meta", item.MetaId); cmd.Parameters.AddWithValue("$video", item.VideoId);
        cmd.Parameters.AddWithValue("$title", item.Title); cmd.Parameters.AddWithValue("$file", item.FileName);
        cmd.Parameters.AddWithValue("$season", (object?)item.Season ?? DBNull.Value); cmd.Parameters.AddWithValue("$episode", (object?)item.Episode ?? DBNull.Value);
        cmd.ExecuteNonQuery();
    }

    public void Record(string resource, string type, string id, string action, long? time, long? duration, string? videos)
    {
        using var db = Open(); using var transaction = db.BeginTransaction(); using var cmd = db.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = """
            INSERT INTO activity(resource,type,video_id,action,current_time,duration,changed_videos,received_at)
            VALUES ($r,$t,$v,$a,$c,$d,$videos,$at);
            DELETE FROM activity WHERE id <= (SELECT COALESCE(MAX(id),0)-5000 FROM activity);
            """;
        cmd.Parameters.AddWithValue("$r", resource); cmd.Parameters.AddWithValue("$t", type); cmd.Parameters.AddWithValue("$v", id);
        cmd.Parameters.AddWithValue("$a", action); cmd.Parameters.AddWithValue("$c", (object?)time ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$d", (object?)duration ?? DBNull.Value); cmd.Parameters.AddWithValue("$videos", (object?)videos ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$at", DateTimeOffset.UtcNow.ToString("O")); cmd.ExecuteNonQuery(); transaction.Commit();
    }

    public List<Activity> Events()
    {
        using var db = Open(); using var cmd = db.CreateCommand();
        cmd.CommandText = """SELECT id,resource,type,video_id,action,"current_time",duration,changed_videos,received_at FROM activity ORDER BY id DESC LIMIT 100""";
        using var r = cmd.ExecuteReader(); var result = new List<Activity>();
        while(r.Read()) result.Add(new(r.GetInt64(0),r.GetString(1),r.GetString(2),r.GetString(3),r.GetString(4),r.IsDBNull(5)?null:r.GetInt64(5),r.IsDBNull(6)?null:r.GetInt64(6),r.IsDBNull(7)?null:r.GetString(7),r.GetString(8)));
        return result;
    }
}
