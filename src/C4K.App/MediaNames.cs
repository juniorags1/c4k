using System.Text.Json;
using System.Text.RegularExpressions;

namespace C4K;

public static class MediaNames
{
    public static string? Poster(string metaId) => Regex.IsMatch(metaId, @"^tt\d{5,12}$", RegexOptions.CultureInvariant)
        ? $"https://images.metahub.space/poster/medium/{metaId}/img" : null;
    public static string Display(DownloadRequest request)
    {
        var title = request.Title ?? request.VideoId.Split(':')[0];
        var parts = request.VideoId.Split(':');
        return request.Type == "series" && parts.Length == 3 ? $"{title} — T{int.Parse(parts[1]):00}E{int.Parse(parts[2]):00}" : title;
    }
    public static string? FromMeta(JsonElement root, string id)
    {
        if (!root.TryGetProperty("meta", out var meta) || meta.ValueKind != JsonValueKind.Object ||
            !meta.TryGetProperty("id", out var metaId) || metaId.ValueKind != JsonValueKind.String || metaId.GetString() != id ||
            !meta.TryGetProperty("name", out var name) || name.ValueKind != JsonValueKind.String) return null;
        var title = name.GetString()!.Trim();
        if (title.Length is 0 or > 180 || title.Any(char.IsControl)) return null;
        var year = meta.TryGetProperty("year", out var y) && y.ValueKind == JsonValueKind.String ? y.GetString() : null;
        return year is not null && Regex.IsMatch(year, @"^\d{4}$") ? $"{title} ({year})" : title;
    }
    public static async Task<string?> Resolve(HttpClient http, DownloadRequest request, CancellationToken ct)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct); timeout.CancelAfter(TimeSpan.FromSeconds(5));
        try
        {
            var id = request.VideoId.Split(':')[0];
            using var response = await http.GetAsync($"https://v3-cinemeta.strem.io/meta/{request.Type}/{id}.json", timeout.Token);
            response.EnsureSuccessStatusCode();
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(timeout.Token));
            return FromMeta(json.RootElement, id);
        }
        catch (Exception e) when ((e is HttpRequestException or JsonException or OperationCanceledException) && !ct.IsCancellationRequested) { return null; }
    }
}
