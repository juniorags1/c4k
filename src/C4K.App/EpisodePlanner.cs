using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace C4K;

public sealed record EpisodePlan(string[] VideoIds, bool WaitingForRelease);
public static class EpisodePlanner
{
    public static (int Season, int Episode)? Position(string? id, string series)
    {
        if (id is null || !id.StartsWith(series + ":", StringComparison.Ordinal)) return null;
        var parts = id.Split(':');
        return parts.Length == 3 && int.TryParse(parts[1], out var s) && int.TryParse(parts[2], out var e) && s > 0 && e > 0 ? (s, e) : null;
    }
    public static bool ValidSeries(string id) => Regex.IsMatch(id, @"^tt\d{5,12}$", RegexOptions.CultureInvariant);
    public static EpisodePlan Next(JsonElement root, string series, string anchor, DateTimeOffset now)
    {
        if (!root.TryGetProperty("meta", out var meta) || !meta.TryGetProperty("id", out var identity) || identity.GetString() != series ||
            !meta.TryGetProperty("videos", out var videos) || videos.ValueKind != JsonValueKind.Array)
            throw new InvalidOperationException("Metadados da série sem lista de episódios válida.");
        var entries = new List<(string Id, int Season, int Episode, bool Released)>();
        foreach (var video in videos.EnumerateArray())
        {
            if (!video.TryGetProperty("id", out var raw) || raw.ValueKind != JsonValueKind.String) continue;
            var id = raw.GetString()!; var position = Position(id, series); if (position is null) continue;
            var released = video.TryGetProperty("released", out var date) && date.ValueKind == JsonValueKind.String &&
                DateTimeOffset.TryParse(date.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var when) && when <= now;
            entries.Add((id, position.Value.Season, position.Value.Episode, released));
        }
        var ordered = entries.DistinctBy(v => v.Id).OrderBy(v => v.Season).ThenBy(v => v.Episode).ToList();
        var index = ordered.FindIndex(v => v.Id == anchor);
        if (index < 0) throw new InvalidOperationException("Episódio assistido não consta na sequência conhecida; confira o ID. Especiais não iniciam antecipação automática.");
        var next = ordered.Skip(index + 1).Take(2).ToArray();
        return new(next.TakeWhile(v => v.Released).Select(v => v.Id).ToArray(), next.Any(v => !v.Released));
    }
}
