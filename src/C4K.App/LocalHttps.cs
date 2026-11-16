using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace C4K;

public sealed record LocalHttpsOptions(string CertificateThumbprint, string RootThumbprint, string Address, int Port = 48242);
public sealed class LocalHttps : IDisposable
{
    public LocalHttpsOptions Options { get; }
    public X509Certificate2 Certificate { get; }
    public X509Certificate2 Root { get; }
    public string BaseUrl => $"https://{Options.Address}:{Options.Port}";
    public string HttpBase(int port) => $"http://{Options.Address}:{port}";
    private LocalHttps(LocalHttpsOptions options, X509Certificate2 certificate, X509Certificate2 root)
    { Options = options; Certificate = certificate; Root = root; }
    public static LocalHttps? Load(string directory, int httpPort, int adminPort)
    {
        var path = Path.Combine(directory, "tls.json"); if (!File.Exists(path)) return null;
        var options = JsonSerializer.Deserialize<LocalHttpsOptions>(File.ReadAllText(path)) ?? throw new InvalidOperationException("Configuração HTTPS inválida.");
        if (options.Port is < 1024 or > 65535 || options.Port == httpPort || options.Port == adminPort ||
            !IPAddress.TryParse(options.Address, out var ip) || ip.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork ||
            !IsPrivate(ip) || !ValidThumbprint(options.CertificateThumbprint) || !ValidThumbprint(options.RootThumbprint))
            throw new InvalidOperationException("Endereço, portas ou certificados HTTPS inválidos.");
        var certificate = Find(StoreName.My, options.CertificateThumbprint);
        var root = Find(StoreName.Root, options.RootThumbprint);
        try
        {
            if (!certificate.HasPrivateKey || !certificate.MatchesHostname(options.Address, allowWildcards: false, allowCommonName: false) ||
                certificate.NotAfter.ToUniversalTime() <= DateTime.UtcNow || certificate.NotBefore.ToUniversalTime() > DateTime.UtcNow ||
                !certificate.Extensions.OfType<X509EnhancedKeyUsageExtension>().Any(e => e.EnhancedKeyUsages.Cast<Oid>().Any(o => o.Value == "1.3.6.1.5.5.7.3.1")))
                throw new InvalidOperationException("Certificado HTTPS sem chave, expirado ou incompatível com o IP.");
            using var chain = new X509Chain(); chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            chain.ChainPolicy.DisableCertificateDownloads = true;
            if (!chain.Build(certificate) || !chain.ChainElements[^1].Certificate.Thumbprint.Equals(root.Thumbprint, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("A cadeia HTTPS não confia na CA local configurada.");
            return new(options, certificate, root);
        }
        catch { certificate.Dispose(); root.Dispose(); throw; }
    }
    private static bool IsPrivate(IPAddress ip) { var b = ip.GetAddressBytes(); return b[0] == 127 || b[0] == 10 ||
        b[0] == 192 && b[1] == 168 || b[0] == 172 && b[1] is >= 16 and <= 31; }
    private static bool ValidThumbprint(string value) => Regex.IsMatch(value, "^[a-fA-F0-9]{40}$", RegexOptions.CultureInvariant);
    private static X509Certificate2 Find(StoreName name, string thumbprint)
    {
        using var store = new X509Store(name, StoreLocation.CurrentUser); store.Open(OpenFlags.ReadOnly);
        return store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, validOnly: false).OfType<X509Certificate2>().FirstOrDefault()
            ?? throw new InvalidOperationException("Certificado HTTPS não encontrado para este usuário do Windows.");
    }
    public void Dispose() { Certificate.Dispose(); Root.Dispose(); }
}
