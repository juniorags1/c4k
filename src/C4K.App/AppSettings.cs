using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text.Json;

namespace C4K;

public sealed record AppSettings(string AddonToken, string MediaToken)
{
    public static string NewToken() => Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();

    public void Save(string directory)
    {
        var path = Path.Combine(directory, "secrets.json");
        File.WriteAllText(path + ".tmp", JsonSerializer.Serialize(this));
        File.Move(path + ".tmp", path, overwrite: true);
    }

    public static AppSettings Load(string directory)
    {
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "secrets.json");
        if (File.Exists(path))
        {
            var saved = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path));
            if (saved is null || saved.AddonToken.Length != 64 || saved.MediaToken.Length != 64)
                throw new InvalidOperationException("Configuração de pareamento inválida. Restaure secrets.json do backup.");
            return saved;
        }
        var settings = new AppSettings(NewToken(), NewToken());
        File.WriteAllText(path + ".tmp", JsonSerializer.Serialize(settings));
        File.Move(path + ".tmp", path);
        return settings;
    }

    public static string PublicUrl(int port, string? saved = null, int? httpsPort = null)
    {
        var configured = Environment.GetEnvironmentVariable("C4K_PUBLIC_URL") ?? saved;
        if (!string.IsNullOrWhiteSpace(configured))
        {
            if (!Uri.TryCreate(configured, UriKind.Absolute, out var uri) ||
                !(uri.Scheme == "http" && uri.Port == port || uri.Scheme == "https" && uri.Port == httpsPort) ||
                uri.AbsolutePath != "/" || uri.Query != "" || uri.Fragment != "" || uri.UserInfo != "")
                throw new InvalidOperationException("C4K_PUBLIC_URL deve usar uma porta HTTP/HTTPS configurada, sem caminho ou credenciais.");
            return uri.GetLeftPart(UriPartial.Authority);
        }
        var address = NetworkInterface.GetAllNetworkInterfaces()
            .Where(n => n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            .Where(n => n.GetIPProperties().GatewayAddresses.Any())
            .SelectMany(n => n.GetIPProperties().UnicastAddresses)
            .Select(n => n.Address)
            .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork && IsPrivate(ip));
        return $"http://{address?.ToString() ?? "127.0.0.1"}:{port}";
    }

    private static bool IsPrivate(IPAddress ip)
    {
        var b = ip.GetAddressBytes();
        return b[0] == 10 || b[0] == 192 && b[1] == 168 || b[0] == 172 && b[1] is >= 16 and <= 31;
    }
}
