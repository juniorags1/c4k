namespace C4K;

public sealed record ImportRequest(RegisterMedia Media, string SourcePath, string? SubtitlePath, string? SubtitleLang, bool ForceCopy = false);
public sealed record LocalSubtitle(string FileName, string Language);

public static class LibraryRequests
{
    public static bool ValidMetadata(RegisterMedia? item) => item is not null && item.Type is "movie" or "series" &&
        MediaFiles.ValidId(item.MetaId) && MediaFiles.ValidId(item.VideoId) && item.Title is { Length: > 0 and <= 200 } &&
        (item.Type == "movie" ? item.MetaId == item.VideoId :
            item.Season is >= 0 and <= 9999 && item.Episode is >= 1 and <= 9999 &&
            item.VideoId == $"{item.MetaId}:{item.Season}:{item.Episode}");

    public static string? Source(string? input, bool subtitle = false)
    {
        if (string.IsNullOrWhiteSpace(input) || input.Length > 2000 || !Path.IsPathFullyQualified(input) ||
            input.StartsWith(@"\\", StringComparison.Ordinal) || input[2..].Contains(':')) return null;
        var full = Path.GetFullPath(input);
        var extension = Path.GetExtension(full).ToLowerInvariant();
        if (subtitle ? extension is not (".srt" or ".vtt") : extension is not (".mp4" or ".mkv" or ".webm" or ".m4v")) return null;
        if (!File.Exists(full)) return null;
        var info = new FileInfo(full);
        if (info.Length == 0 || subtitle && info.Length > 5 * 1024 * 1024) return null;
        for (var current = full; current is not null; current = Path.GetDirectoryName(current))
            if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0) return null;
        return full;
    }
}
