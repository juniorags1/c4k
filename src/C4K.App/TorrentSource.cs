using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.WebUtilities;

namespace C4K;

public sealed record TorrentCandidate(string Hash, int? FileIndex, int Seeds, string Magnet);
public static class TorrentSource
{
    public static string MagnetHash(string magnet)
    {
        if (magnet.Length > 12000 || !magnet.StartsWith("magnet:?", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Magnet inválido.");
        var xt = QueryHelpers.ParseQuery(magnet[7..]).TryGetValue("xt", out var value) ? value.ToString() : "";
        if (!Regex.IsMatch(xt, @"^urn:btih:[a-fA-F0-9]{40}$", RegexOptions.IgnoreCase)) throw new ArgumentException("Use magnet com hash BTIH hexadecimal.");
        return xt[9..].ToLowerInvariant();
    }
    public static TorrentCandidate? Select(JsonElement root)
    {
        if (!root.TryGetProperty("streams", out var streams) || streams.ValueKind != JsonValueKind.Array) return null;
        var candidates = new List<TorrentCandidate>();
        foreach (var stream in streams.EnumerateArray())
        {
            string Field(string name) => stream.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString()! : "";
            var label = Field("name") + "\n" + Field("title");
            if (!Regex.IsMatch(label, @"(?<!\w)(4k|2160p)(?!\w)", RegexOptions.IgnoreCase)) continue;
            var hash = Field("infoHash");
            if (!Regex.IsMatch(hash, @"^[a-fA-F0-9]{40}$")) continue;
            var match = Regex.Match(label, @"(?:👤\s*|\bseeds?\s*[:=]?\s*)([0-9]+)", RegexOptions.IgnoreCase);
            int seeds;
            if (stream.TryGetProperty("seeders", out var raw) && raw.ValueKind == JsonValueKind.Number && raw.TryGetInt32(out var number)) seeds = number;
            else if (!match.Success || !int.TryParse(match.Groups[1].Value, out seeds)) continue;
            if (seeds < 0) continue;
            int? index = stream.TryGetProperty("fileIdx", out var idx) && idx.TryGetInt32(out var n) && n >= 0 ? n : null;
            var magnet = "magnet:?xt=urn:btih:" + hash.ToLowerInvariant();
            if (stream.TryGetProperty("sources", out var sources) && sources.ValueKind == JsonValueKind.Array)
                foreach (var s in sources.EnumerateArray())
                    if (s.ValueKind == JsonValueKind.String && s.GetString() is { } source && source.StartsWith("tracker:", StringComparison.Ordinal))
                        magnet += "&tr=" + Uri.EscapeDataString(source[8..]);
            candidates.Add(new(hash.ToLowerInvariant(), index, seeds, magnet));
        }
        return candidates.OrderByDescending(c => c.Seeds).ThenBy(c => c.Hash, StringComparer.Ordinal).FirstOrDefault();
    }
    public static async Task<TorrentCandidate> Resolve(HttpClient http, string manifest, DownloadRequest request, CancellationToken ct)
    {
        var url = manifest[..^"manifest.json".Length] + "stream/" + request.Type + "/" + Uri.EscapeDataString(request.VideoId) + ".json";
        using var response = await http.GetAsync(url, ct); response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        return Select(json.RootElement) ?? throw new InvalidOperationException("Nenhuma fonte 4K com hash e quantidade de seeds informada. Tente novamente depois ou informe magnet manual.");
    }
}
