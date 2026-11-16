using System.Net;
using System.Text.Json;

namespace C4K;

public sealed class QbitClient : IDisposable
{
    private readonly HttpClient http;
    public QbitClient(string url, HttpMessageHandler? handler = null)
    {
        http = new HttpClient(handler ?? new HttpClientHandler { CookieContainer = new CookieContainer(), AllowAutoRedirect = false })
        { BaseAddress = new Uri(url.TrimEnd('/') + "/api/v2/"), Timeout = TimeSpan.FromSeconds(20) };
        http.DefaultRequestHeaders.Referrer = new Uri(url.TrimEnd('/') + "/");
    }
    public async Task<string> Connect(string username, string password, CancellationToken ct)
    {
        using var probe = await http.GetAsync("app/version", ct);
        if (probe.IsSuccessStatusCode) return await probe.Content.ReadAsStringAsync(ct);
        if (password.Length == 0) throw new InvalidOperationException("qBittorrent exige autenticação. Informe usuário e senha no painel.");
        using var login = await http.PostAsync("auth/login", new FormUrlEncodedContent(new Dictionary<string, string> { ["username"] = username, ["password"] = password }), ct);
        if (login.StatusCode == HttpStatusCode.Forbidden)
            throw new InvalidOperationException("qBittorrent bloqueou o login (HTTP 403). Verifique bloqueio temporário por tentativas e proteções da Interface Web.");
        if (!login.IsSuccessStatusCode)
            throw new InvalidOperationException("Login do qBittorrent recusado. Verifique usuário e senha.");
        // qBittorrent 5.3 retorna 204 vazio; versões anteriores podem retornar 200/Ok.
        // A consulta autenticada comprova a sessão, inclusive quando o login antigo retorna 200/Fails.
        using var version = await http.GetAsync("app/version", ct);
        if (version.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            throw new InvalidOperationException("A sessão do qBittorrent não foi autenticada. Verifique usuário e senha.");
        version.EnsureSuccessStatusCode(); return await version.Content.ReadAsStringAsync(ct);
    }
    public async Task<JsonElement> Get(string path, CancellationToken ct)
    {
        using var r = await http.GetAsync(path, ct); r.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await r.Content.ReadAsStringAsync(ct)); return json.RootElement.Clone();
    }
    public async Task Post(string path, Dictionary<string, string> values, CancellationToken ct)
    {
        using var r = await http.PostAsync(path, new FormUrlEncodedContent(values), ct); r.EnsureSuccessStatusCode();
        if (path == "torrents/add" && r.StatusCode != HttpStatusCode.NoContent &&
            (await r.Content.ReadAsStringAsync(ct)).Trim() != "Ok.") throw new InvalidOperationException("qBittorrent recusou o magnet.");
    }
    public void Dispose() => http.Dispose();
}
