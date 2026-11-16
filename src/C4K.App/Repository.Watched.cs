using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace C4K;

public sealed record WatchedPlan(string SeriesId, string? Anchor, string State, string Message, DateTimeOffset NextAttempt, bool FromBeginning = false);
public sealed partial class Repository
{
    private void InitializeWatched()
    {
        using var db = Open(); using var cmd = db.CreateCommand();
        cmd.CommandText = "CREATE TABLE IF NOT EXISTS watched_plans(series_id TEXT PRIMARY KEY,body TEXT NOT NULL)"; cmd.ExecuteNonQuery();
    }
    public List<WatchedPlan> WatchedPlans()
    {
        using var db = Open(); using var cmd = db.CreateCommand(); cmd.CommandText = "SELECT body FROM watched_plans";
        using var r = cmd.ExecuteReader(); var plans = new List<WatchedPlan>();
        while (r.Read()) plans.Add(JsonSerializer.Deserialize<WatchedPlan>(r.GetString(0))!); return plans;
    }
    private static void ObserveSeriesAdded(SqliteConnection db, SqliteTransaction tx, string series)
    {
        if (!EpisodePlanner.ValidSeries(series)) return;
        using var cmd = db.CreateCommand(); cmd.Transaction = tx;
        cmd.CommandText = "SELECT body FROM watched_plans WHERE series_id=$id";
        cmd.Parameters.AddWithValue("$id", series);
        if (cmd.ExecuteScalar() is string previous)
        {
            var old = JsonSerializer.Deserialize<WatchedPlan>(previous)!;
            if (old.Anchor is not null || old.FromBeginning) return;
        }
        var plan = new WatchedPlan(series, null, "Pending", "Buscando os dois primeiros episódios.", DateTimeOffset.MinValue, true);
        cmd.CommandText = "INSERT INTO watched_plans(series_id,body) VALUES($id,$body) ON CONFLICT(series_id) DO UPDATE SET body=$body";
        cmd.Parameters.AddWithValue("$body", JsonSerializer.Serialize(plan)); cmd.ExecuteNonQuery();
    }
    private static void ObserveWatched(SqliteConnection db, SqliteTransaction tx, string series, string? videos)
    {
        if (!EpisodePlanner.ValidSeries(series)) return;
        var anchor = (videos ?? "").Split(',').Select(v => new { Id = v, Position = EpisodePlanner.Position(v, series) })
            .Where(v => v.Position is not null).OrderByDescending(v => v.Position!.Value.Season).ThenByDescending(v => v.Position!.Value.Episode).FirstOrDefault()?.Id;
        using var cmd = db.CreateCommand(); cmd.Transaction = tx;
        cmd.CommandText = "SELECT body FROM watched_plans WHERE series_id=$id"; cmd.Parameters.AddWithValue("$id", series);
        if (cmd.ExecuteScalar() is string body)
        {
            var old = JsonSerializer.Deserialize<WatchedPlan>(body)!;
            if (old.FromBeginning && anchor is null) return;
            if (old.Anchor is not null && (anchor is null || EpisodePlanner.Position(anchor, series)!.Value.CompareTo(EpisodePlanner.Position(old.Anchor, series)!.Value) <= 0)) return;
        }
        var plan = new WatchedPlan(series, anchor, anchor is null ? "NeedsEpisode" : "Pending",
            anchor is null ? "Watched recebido sem episódio regular identificável. Marque um episódio específico." : "Buscando os próximos dois episódios.", DateTimeOffset.MinValue);
        cmd.CommandText = "INSERT INTO watched_plans VALUES($id,$body) ON CONFLICT(series_id) DO UPDATE SET body=$body";
        cmd.Parameters.AddWithValue("$body", JsonSerializer.Serialize(plan)); cmd.ExecuteNonQuery();
    }
    public void ApplyWatched(WatchedPlan observed, EpisodePlan? selection, string? error = null)
    {
        using var db = Open(); using var tx = db.BeginTransaction(); using var cmd = db.CreateCommand(); cmd.Transaction = tx;
        cmd.CommandText = "SELECT body FROM watched_plans WHERE series_id=$id"; cmd.Parameters.AddWithValue("$id", observed.SeriesId);
        if (cmd.ExecuteScalar() is not string body) return;
        var current = JsonSerializer.Deserialize<WatchedPlan>(body)!;
        if (current.Anchor != observed.Anchor || current.FromBeginning != observed.FromBeginning) return;
        if (selection is not null)
            foreach (var id in selection.VideoIds)
            {
                cmd.CommandText = "SELECT COUNT(*) FROM media WHERE type='series' AND video_id=$video";
                cmd.Parameters.Clear(); cmd.Parameters.AddWithValue("$video", id);
                if (Convert.ToInt64(cmd.ExecuteScalar()) == 0) InsertDownload(db, tx, new("series", id));
            }
        var state = error is not null ? "Retry" : selection!.WaitingForRelease ? "WaitingRelease" : "Prepared";
        var message = error ?? (selection!.VideoIds.Length == 0 ? "Sem próximos episódios lançados na lista conhecida."
            : "Episódios assegurados na fila ou já disponíveis: " + string.Join(", ", selection.VideoIds));
        var next = observed with { State = state, Message = message, NextAttempt = DateTimeOffset.UtcNow.Add(error is not null ? TimeSpan.FromMinutes(10) : TimeSpan.FromHours(6)) };
        cmd.Parameters.Clear(); cmd.Parameters.AddWithValue("$id", observed.SeriesId); cmd.Parameters.AddWithValue("$body", JsonSerializer.Serialize(next));
        cmd.CommandText = "UPDATE watched_plans SET body=$body WHERE series_id=$id"; cmd.ExecuteNonQuery(); tx.Commit();
    }
}
