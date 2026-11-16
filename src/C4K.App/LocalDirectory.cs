namespace C4K;

public static class LocalDirectory
{
    public static string Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !Path.IsPathFullyQualified(value) ||
            value.StartsWith(@"\\", StringComparison.Ordinal) || value[2..].Contains(':'))
            throw new ArgumentException("Informe uma pasta local absoluta.");
        var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(value));
        if (root.Length <= Path.TrimEndingDirectorySeparator(Path.GetPathRoot(root)!).Length)
            throw new ArgumentException("Escolha uma pasta, não a raiz do disco.");
        for (var current = root; current is not null; current = Path.GetDirectoryName(current))
            if ((Directory.Exists(current) || File.Exists(current)) && (File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                throw new ArgumentException("Não use links simbólicos ou junctions.");
        return root;
    }
}
