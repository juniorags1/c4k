using System.Text.RegularExpressions;

namespace C4K;

public sealed class MediaFiles(string root)
{
    public string Root { get; } = Path.GetFullPath(root);
    public string? Resolve(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || Path.IsPathRooted(name) || name.Contains(':')) return null;
        var path = Path.GetFullPath(Path.Combine(Root, name));
        if (!path.StartsWith(Path.TrimEndingDirectorySeparator(Root) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) return null;
        if (Path.GetExtension(path).ToLowerInvariant() is not (".mp4" or ".mkv" or ".webm" or ".m4v")) return null;
        if (!File.Exists(path) || new FileInfo(path).Length == 0) return null;
        for (var entry = path; entry is not null; entry = Path.GetDirectoryName(entry))
        {
            if ((File.GetAttributes(entry) & FileAttributes.ReparsePoint) != 0) return null;
            if (string.Equals(entry, Path.TrimEndingDirectorySeparator(Root), StringComparison.OrdinalIgnoreCase)) break;
        }
        return path;
    }

    public static bool ValidId(string? id) => id is not null && Regex.IsMatch(id, @"^(tt[0-9]{5,12}|c4k:[a-zA-Z0-9_-]{1,80})(:[0-9]{1,4}:[0-9]{1,4})?$", RegexOptions.CultureInvariant);
    public static string ContentType(string file) => Path.GetExtension(file).ToLowerInvariant() switch
    { ".mkv" => "video/x-matroska", ".webm" => "video/webm", _ => "video/mp4" };
}
