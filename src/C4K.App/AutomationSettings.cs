using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace C4K;

public sealed record AutomationOptions(bool Enabled = false, string Provider = "https://torrentio.strem.fun/manifest.json",
    string QbitUrl = "http://127.0.0.1:8082", string Username = "admin", string PasswordProtected = "",
    string NewDirectory = @"C:\media\new");
public sealed record AutomationInput(bool Enabled, string Provider, string QbitUrl, string Username, string? Password,
    string? NewDirectory = null);

public sealed class AutomationSettings(string directory)
{
    private readonly string path = Path.Combine(directory, "automation.json");
    public AutomationOptions Current { get; private set; } = Load(directory);
    private static AutomationOptions Load(string directory) => File.Exists(Path.Combine(directory, "automation.json"))
        ? JsonSerializer.Deserialize<AutomationOptions>(File.ReadAllText(Path.Combine(directory, "automation.json")))! : new();
    public object Public() => new { Current.Enabled, Current.Provider, Current.QbitUrl, Current.Username,
        hasPassword = Current.PasswordProtected.Length > 0, Current.NewDirectory };
    public void Save(AutomationInput input)
    {
        if (!Uri.TryCreate(input.QbitUrl, UriKind.Absolute, out var q) || q.Scheme != "http" ||
            q.Host is not ("127.0.0.1" or "localhost") || q.AbsolutePath != "/" || q.UserInfo != "" || q.Query != "" || q.Fragment != "")
            throw new ArgumentException("Use o endereço HTTP local do qBittorrent.");
        if (!Uri.TryCreate(input.Provider, UriKind.Absolute, out var p) || p.Scheme != "https" ||
            !p.AbsolutePath.EndsWith("/manifest.json", StringComparison.Ordinal) || p.UserInfo != "" || p.Query != "" || p.Fragment != "")
            throw new ArgumentException("Manifest inválido.");
        if (input.Username.Length > 200 || input.Password?.Length > 1000) throw new ArgumentException();
        var newDirectory = input.NewDirectory is null ? Current.NewDirectory : LocalDirectory.Validate(input.NewDirectory);
        Directory.CreateDirectory(newDirectory);
        var next = Current with { Enabled = input.Enabled, Provider = input.Provider, QbitUrl = q.GetLeftPart(UriPartial.Authority), NewDirectory = newDirectory,
            Username = input.Username, PasswordProtected = string.IsNullOrEmpty(input.Password) ? Current.PasswordProtected : Protect(input.Password, false) };
        File.WriteAllText(path + ".tmp", JsonSerializer.Serialize(next)); File.Move(path + ".tmp", path, true); Current = next;
    }
    [JsonIgnore] public string Password => Current.PasswordProtected.Length == 0 ? "" : Protect(Current.PasswordProtected, true);

    // DPAPI CurrentUser: o segredo salvo só pode ser aberto pelo usuário Windows que o gravou.
    private static string Protect(string value, bool decrypt)
    {
        var bytes = decrypt ? Convert.FromBase64String(value) : Encoding.UTF8.GetBytes(value);
        var input = new Blob { Size = bytes.Length, Data = Marshal.AllocHGlobal(bytes.Length) };
        Marshal.Copy(bytes, 0, input.Data, bytes.Length);
        try
        {
            Blob output;
            var ok = decrypt ? CryptUnprotectData(ref input, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 1, out output)
                : CryptProtectData(ref input, null, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 1, out output);
            if (!ok) throw new InvalidOperationException("Não foi possível proteger as credenciais no Windows.");
            try { var result = new byte[output.Size]; Marshal.Copy(output.Data, result, 0, result.Length);
                return decrypt ? Encoding.UTF8.GetString(result) : Convert.ToBase64String(result); }
            finally { LocalFree(output.Data); }
        }
        finally { Marshal.FreeHGlobal(input.Data); Array.Clear(bytes); }
    }
    [StructLayout(LayoutKind.Sequential)] private struct Blob { public int Size; public IntPtr Data; }
    [DllImport("crypt32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)] private static extern bool CryptProtectData(ref Blob input, string? description, IntPtr entropy, IntPtr reserved, IntPtr prompt, int flags, out Blob output);
    [DllImport("crypt32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)] private static extern bool CryptUnprotectData(ref Blob input, IntPtr description, IntPtr entropy, IntPtr reserved, IntPtr prompt, int flags, out Blob output);
    [DllImport("kernel32.dll")] private static extern IntPtr LocalFree(IntPtr memory);
}
