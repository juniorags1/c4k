using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;
using C4K;
using Microsoft.AspNetCore.WebUtilities;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders(); // URLs de pareamento não podem aparecer nos logs HTTP.
var dataDir = Environment.GetEnvironmentVariable("C4K_DATA_DIR") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "C4K");
var settings = AppSettings.Load(dataDir);
var libraryPath = Path.Combine(dataDir, "library.json");
var savedRoot = File.Exists(libraryPath) ? JsonSerializer.Deserialize<LibrarySettings>(File.ReadAllText(libraryPath))?.Root : null;
var mediaRoot = Environment.GetEnvironmentVariable("C4K_MEDIA_ROOT") ?? savedRoot ?? @"C:\media\done";
Directory.CreateDirectory(mediaRoot);
var files = new MediaFiles(mediaRoot);
var repository = new Repository(dataDir);
var addonPort = int.Parse(Environment.GetEnvironmentVariable("C4K_ADDON_PORT") ?? "48240", CultureInfo.InvariantCulture);
var adminPort = int.Parse(Environment.GetEnvironmentVariable("C4K_ADMIN_PORT") ?? "48241", CultureInfo.InvariantCulture);
if (addonPort == adminPort || addonPort is < 1024 or > 65535 || adminPort is < 1024 or > 65535)
    throw new InvalidOperationException("Portas devem ser distintas e estar entre 1024 e 65535.");
var networkPath = Path.Combine(dataDir, "network.json");
var savedAddress = File.Exists(networkPath) ? JsonSerializer.Deserialize<NetworkSettings>(File.ReadAllText(networkPath))?.PublicUrl : null;
var publicUrl = AppSettings.PublicUrl(addonPort, savedAddress);
string ManifestUrl() => $"{publicUrl}/{settings.AddonToken[..10]}/manifest.json";
string DesktopManifestUrl() => $"http://127.0.0.1:{addonPort}/{settings.AddonToken[..10]}/manifest.json";
var pairingGate = new object();
var libraryGate = new object();
builder.Services.AddSingleton(repository);
builder.Services.AddSingleton(files);
var automation = new AutomationSettings(dataDir);
var downloader = new DownloadWorker(repository, files, automation);
builder.Services.AddSingleton(automation);
builder.Services.AddSingleton(downloader);
builder.Services.AddHostedService(_ => downloader);
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(addonPort);
    options.Listen(IPAddress.Loopback, adminPort);
    options.Limits.MaxRequestBodySize = 16 * 1024;
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddPolicy("events", _ => RateLimitPartition.GetFixedWindowLimiter("events", _ => new FixedWindowRateLimiterOptions
    { PermitLimit = 120, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});
var app = builder.Build();
app.Use(async (context, next) =>
{
    // Reescrita interna, sem redirect: recursos continuam sob a base curta no cliente.
    if (context.Request.Path.StartsWithSegments($"/{settings.AddonToken[..10]}", out var remainder))
        context.Request.Path = $"/a/{settings.AddonToken}{remainder}";
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers.CacheControl = "no-store";
    var path = context.Request.Path;
    var admin = path.StartsWithSegments("/admin") || path == "/";
    if (admin)
    {
        var host = context.Request.Host.Host;
        if (context.Connection.LocalPort != adminPort || context.Connection.RemoteIpAddress is not { } ip ||
            !IPAddress.IsLoopback(ip) || host is not ("localhost" or "127.0.0.1"))
        { context.Response.StatusCode = 404; return; }
        if (path.StartsWithSegments("/admin/api"))
        {
            if (context.Request.Headers["X-C4K-Admin"] != "1") { context.Response.StatusCode = 403; return; }
            var origin = context.Request.Headers.Origin.ToString();
            if (origin.Length > 0 && origin != $"http://{context.Request.Host}") { context.Response.StatusCode = 403; return; }
        }
    }
    else if (context.Connection.LocalPort != addonPort) { context.Response.StatusCode = 404; return; }
    if (path.StartsWithSegments("/a") || path.StartsWithSegments("/m") || path.StartsWithSegments("/s"))
    {
        context.Response.Headers.AccessControlAllowOrigin = "*";
        context.Response.Headers.AccessControlAllowMethods = "GET, HEAD, OPTIONS";
        context.Response.Headers.AccessControlAllowHeaders = "Content-Type, Range, If-Range";
        context.Response.Headers.AccessControlExposeHeaders = "Content-Length, Content-Range, Accept-Ranges";
        if(context.Request.Method == "OPTIONS") { context.Response.StatusCode = 204; return; }
    }
    try { await next(context); }
    catch (Exception e) when (e is ArgumentException or FormatException)
    { if (!context.Response.HasStarted) { context.Response.StatusCode = 400; await context.Response.WriteAsJsonAsync(new { error = "Dados inválidos." }); } }
    catch (Exception error)
    {
        Console.Error.WriteLine($"Falha de requisição: {error.GetType().Name}");
        if (!context.Response.HasStarted) { context.Response.StatusCode = 500; await context.Response.WriteAsJsonAsync(new { error = "Falha local. Verifique disco e configuração." }); }
    }
});
app.UseRouting(); // Resolver a rota após expandir o alias curto.
app.UseRateLimiter();
app.MapGet("/", () => Results.Redirect("/admin/"));
app.MapGet("/admin/", () => Results.Content(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "LibraryPanel.html")), "text/html; charset=utf-8"));
app.MapGet("/admin/api/status", () => new
{
    manifestUrl = ManifestUrl(), desktopManifestUrl = DesktopManifestUrl(),
    installUrl = $"stremio:///addons?addon={Uri.EscapeDataString(DesktopManifestUrl())}",
    browserInstallUrl = $"https://web.stremio.com/#/addons?addon={Uri.EscapeDataString(ManifestUrl())}",
    publicUrl, mediaRoot = files.Root, media = repository.Media(), events = repository.Events(),
    imports = repository.Imports(),
    automation = automation.Public(), downloads = repository.Downloads(), connectionStatus = downloader.ConnectionStatus,
    warning = "Adicionar uma série sem episódio explícito prepara S1E1. Remover pelo painel apaga o vídeo físico."
});
app.MapPost("/admin/api/automation", async (AutomationInput input, CancellationToken ct) =>
{
    await downloader.Gate.WaitAsync(ct);
    try { automation.Save(input); return Results.Ok(new { ok = true }); }
    finally { downloader.Gate.Release(); }
});
app.MapPost("/admin/api/automation/test", async (CancellationToken ct) =>
{
    try { return Results.Ok(new { message = await downloader.Test(ct) }); }
    catch (Exception e) { return Results.BadRequest(new { error = DownloadWorker.Friendly(e) }); }
});
app.MapPost("/admin/api/downloads", async (DownloadRequest input, CancellationToken ct) =>
{
    await downloader.Gate.WaitAsync(ct);
    try { return Results.Accepted(value: repository.QueueDownload(input)); }
    finally { downloader.Gate.Release(); }
});
app.MapPost("/admin/api/downloads/retry", async (DownloadRetry input, CancellationToken ct) =>
{
    await downloader.Gate.WaitAsync(ct);
    try
    {
        var job = repository.Downloads().FirstOrDefault(j => j.Id == input.Id);
        if (job is null || job.State is not ("Failed" or "Removed")) return Results.Conflict();
        var request = input.Magnet is null ? job.Request : job.Request with { Magnet = input.Magnet, FileIndex = input.FileIndex };
        Repository.NormalizeDownload(request);
        repository.SaveDownload(job with { State = "Queued", Error = null, Selected = false,
            Hash = input.Magnet is null ? job.Hash : null, FileIndex = input.Magnet is null ? job.FileIndex : input.FileIndex, Request = request });
        return Results.Ok(new { ok = true });
    }
    finally { downloader.Gate.Release(); }
});
app.MapPost("/admin/api/imports", (ImportRequest input) =>
{
    lock (libraryGate)
    {
    if (!LibraryRequests.ValidMetadata(input.Media)) return Results.BadRequest(new { error = "Identificação do filme/episódio inválida." });
    var source = LibraryRequests.Source(input.SourcePath);
    var subtitle = string.IsNullOrWhiteSpace(input.SubtitlePath) ? null : LibraryRequests.Source(input.SubtitlePath, subtitle: true);
    if (source is null || !string.IsNullOrWhiteSpace(input.SubtitlePath) && subtitle is null ||
        input.SubtitleLang is not null && !Regex.IsMatch(input.SubtitleLang, "^[a-z]{3}$", RegexOptions.CultureInvariant))
        return Results.BadRequest(new { error = "Selecione arquivo completo local MP4/MKV/WEBM/M4V e, opcionalmente, SRT/VTT de até 5 MB. Não use links ou pastas de rede." });
    var relative = Path.GetRelativePath(files.Root, source);
    if (files.Resolve(relative) is null || input.ForceCopy || subtitle is not null)
        return Results.BadRequest(new { error = "Cadastre o vídeo completo diretamente em done. Cópias e importações de outras pastas foram desativadas." });
    var m = input.Media;
    repository.Register(new(Guid.NewGuid().ToString("N"), m.Type, m.MetaId, m.VideoId, m.Title, relative, m.Season, m.Episode));
    return Results.Ok(new { state = "Ready" });
    }
});
app.MapGet("/admin/api/imports/{id}", (string id) => repository.Imports(id).FirstOrDefault() is { } job ? Results.Ok(job) : Results.NotFound());
app.MapPost("/admin/api/imports/{id}/retry", (string id) =>
{
    lock (libraryGate)
        return Results.Conflict(new { error = "Importações com cópia foram desativadas. Cadastre o arquivo diretamente em done." });
});
app.MapDelete("/admin/api/media/{id}", async (string id, CancellationToken ct) =>
{
    try { return await downloader.Delete(id, ct) ? Results.Ok(new { ok = true }) : Results.NotFound(); }
    catch (Exception e) { return Results.Conflict(new { error = DownloadWorker.Friendly(e) }); }
});
app.MapPost("/admin/api/pairing/rotate", () =>
{
    lock (pairingGate)
    {
        var replacement = new AppSettings(AppSettings.NewToken(), AppSettings.NewToken());
        replacement.Save(dataDir); settings = replacement;
    }
    return Results.Ok(new { manifestUrl = ManifestUrl() });
});
app.MapPost("/admin/api/network", (NetworkSettings input) =>
{
    if (Environment.GetEnvironmentVariable("C4K_PUBLIC_URL") is not null)
        return Results.Conflict(new { error = "Endereço definido no ambiente. Remova C4K_PUBLIC_URL antes de alterar pelo painel." });
    if (!Uri.TryCreate(input.PublicUrl, UriKind.Absolute, out var address) || address.Scheme != "http" ||
        address.Port != addonPort || address.AbsolutePath != "/" || address.Query != "" || address.Fragment != "" || address.UserInfo != "" ||
        !IPAddress.TryParse(address.Host, out var ip) || !NetworkInterface.GetAllNetworkInterfaces().SelectMany(n => n.GetIPProperties().UnicastAddresses).Any(a => a.Address.Equals(ip)))
        return Results.BadRequest(new { error = "Use http://IP-deste-PC:porta, sem caminho, na porta atual do addon." });
    lock (pairingGate)
    {
        var replacement = new NetworkSettings(address.GetLeftPart(UriPartial.Authority));
        File.WriteAllText(networkPath + ".tmp", JsonSerializer.Serialize(replacement));
        File.Move(networkPath + ".tmp", networkPath, overwrite: true); publicUrl = replacement.PublicUrl;
    }
    return Results.Ok(new { manifestUrl = ManifestUrl() });
});
app.MapPost("/admin/api/library-root", (LibrarySettings input) =>
{
    lock (libraryGate)
    {
        if (Environment.GetEnvironmentVariable("C4K_MEDIA_ROOT") is not null)
            return Results.Conflict(new { error = "Pasta definida no ambiente. Remova C4K_MEDIA_ROOT antes de alterar pelo painel." });
        if (!repository.CanChangeRoot()) return Results.Conflict(new { error = "A biblioteca precisa estar vazia e sem importações em andamento. Esta ação não move arquivos existentes." });
        if (string.IsNullOrWhiteSpace(input.Root) || !Path.IsPathFullyQualified(input.Root) || input.Root.StartsWith(@"\\", StringComparison.Ordinal) || input.Root[2..].Contains(':'))
            return Results.BadRequest(new { error = "Informe uma pasta local absoluta, sem links ou caminho de rede." });
        var root = Path.GetFullPath(input.Root);
        if (Path.TrimEndingDirectorySeparator(root).Length <= Path.TrimEndingDirectorySeparator(Path.GetPathRoot(root)!).Length)
            return Results.BadRequest(new { error = "Escolha uma pasta, não a raiz do disco." });
        for (var current = root; current is not null; current = Path.GetDirectoryName(current))
            if ((Directory.Exists(current) || File.Exists(current)) && (File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                return Results.BadRequest(new { error = "Não use links simbólicos ou junctions." });
        Directory.CreateDirectory(root);
        File.WriteAllText(libraryPath + ".tmp", JsonSerializer.Serialize(new LibrarySettings(root)));
        File.Move(libraryPath + ".tmp", libraryPath, overwrite: true); files.SetRoot(root);
        return Results.Ok(new { mediaRoot = files.Root });
    }
});
app.MapPost("/admin/api/media", (RegisterMedia input) =>
{
    lock (libraryGate)
    {
    if (input.Type is not ("movie" or "series") || !MediaFiles.ValidId(input.MetaId) || !MediaFiles.ValidId(input.VideoId) ||
        input.Title is null || input.Title.Length is < 1 or > 200 || input.FileName is null || input.FileName.Length > 1000)
        return Results.BadRequest(new { error = "Informe tipo, IDs, título e arquivo válidos." });
    if (input.Type == "movie" && input.MetaId != input.VideoId || input.Type == "series" &&
        (input.Season is null or < 0 or > 9999 || input.Episode is null or < 1 or > 9999 || input.VideoId != $"{input.MetaId}:{input.Season}:{input.Episode}"))
        return Results.BadRequest(new { error = "Filme usa o mesmo ID; série exige ID:temporada:episódio." });
    if (files.Resolve(input.FileName) is null) return Results.BadRequest(new { error = "Arquivo completo deve existir na pasta de mídia, sem links, e ser MP4, MKV, WEBM ou M4V." });
    repository.Register(new(Guid.NewGuid().ToString("N"), input.Type, input.MetaId, input.VideoId, input.Title, input.FileName, input.Season, input.Episode));
    return Results.Ok(new { ok = true });
    }
});
app.MapGet("/health/live", () => Results.Ok(new { status = "ok", stage = "qbittorrent-automation", version = "0.3.0" }));

var addon = app.MapGroup("/a/{token}").AddEndpointFilter(async (context, next) =>
{
    return TokenEquals(context.HttpContext.Request.RouteValues["token"]?.ToString(), settings.AddonToken)
        ? await next(context) : Results.NotFound();
});
addon.MapGet("/manifest.json", () => Results.Json(new
{
    id = "local.c4k.probe", version = "0.3.0", name = "C4K — biblioteca local",
    description = "Arquivos completos no PC e automação de downloads pelo qBittorrent.",
    types = new[] { "movie", "series" },
    resources = new object[] { "catalog", new { name = "meta", types = new[] { "movie", "series" }, idPrefixes = new[] { "tt", "c4k:" } },
        new { name = "stream", types = new[] { "movie", "series" }, idPrefixes = new[] { "tt", "c4k:" } },
        new { name = "player", types = new[] { "movie", "series" }, idPrefixes = new[] { "tt", "c4k:" } },
        new { name = "library", types = new[] { "movie", "series" }, idPrefixes = new[] { "tt", "c4k:" } } },
    catalogs = new[] { new { type = "movie", id = "c4k-ready", name = "C4K — filmes no PC" }, new { type = "series", id = "c4k-ready", name = "C4K — séries no PC" } }
}));
addon.MapGet("/catalog/{type}/{id}.json", (string type, string id) => Results.Json(new
{
    // Manter o catálogo já salvo nos clientes que fizeram o teste anterior.
    metas = id is not ("c4k-ready" or "casa4k-ready") ? [] : repository.Media().Where(m => m.Type == type && files.Resolve(m.FileName) is not null)
        .DistinctBy(m => m.MetaId).Select(m => new { id = m.MetaId, type = m.Type, name = m.Title,
            poster = MediaNames.Poster(m.MetaId), posterShape = "poster" }).ToArray()
}));
addon.MapGet("/meta/{type}/{id}.json", (string type, string id) =>
{
    var items = repository.Media().Where(m => m.Type == type && m.MetaId == id && files.Resolve(m.FileName) is not null).ToList();
    return items.Count == 0 ? Results.NotFound() : Results.Json(new { meta = new { id, type, name = items[0].Title,
        poster = MediaNames.Poster(id), posterShape = "poster",
        videos = items.Select(m => new { id = m.VideoId, title = m.Title, season = m.Season, episode = m.Episode }).ToArray() } });
});
addon.MapGet("/stream/{type}/{id}.json", (string type, string id) =>
{
    var item = repository.Media().FirstOrDefault(m => m.Type == type && m.VideoId == id && files.Resolve(m.FileName) is not null);
    return item is null ? Results.Json(new { streams = Array.Empty<object>() }) : Results.Json(new
    {
        streams = new[] { new { name = "C4K — disponível no PC", url = $"{publicUrl}/m/{settings.MediaToken}/{item.Id}",
            subtitles = repository.Subtitle(item.Id) is { } subtitle && files.ResolveSubtitle(subtitle.FileName) is not null
                ? new[] { new { id = item.Id + "-local", url = $"{publicUrl}/s/{settings.MediaToken}/{item.Id}{Path.GetExtension(subtitle.FileName)}", lang = subtitle.Language } }
                : [],
            behaviorHints = new { notWebReady = true, bingeGroup = "c4k-local", filename = Path.GetFileName(item.FileName), videoSize = new FileInfo(files.Resolve(item.FileName)!).Length } } }
    });
});
addon.MapGet("/{resource}/{type}/{id}/{extra}.json", (string resource, string type, string id, string extra) =>
{
    if (resource is not ("player" or "library") || type is not ("movie" or "series") || !MediaFiles.ValidId(id) || extra.Length > 4096)
        return Results.BadRequest();
    var values = QueryHelpers.ParseQuery(extra);
    string Value(string key) => values.TryGetValue(key, out var value) ? value.ToString() : "";
    var action = Value("action");
    long? time = null, duration = null;
    if (resource == "player")
    {
        if (action is not ("start" or "pause" or "stop") ||
            !long.TryParse(Value("currentTime"), NumberStyles.None, CultureInfo.InvariantCulture, out var t) ||
            !long.TryParse(Value("duration"), NumberStyles.None, CultureInfo.InvariantCulture, out var d)) return Results.BadRequest();
        time = t; duration = d;
    }
    else if (action is not ("libraryAdd" or "libraryRemove" or "watched" or "unwatched")) return Results.BadRequest();
    var videoIds = Value("videoId");
    if (videoIds.Length > 0 && (videoIds.Split(',').Length > 100 || videoIds.Split(',').Any(v => !MediaFiles.ValidId(v)))) return Results.BadRequest();
    repository.Record(resource, type, id, action, time, duration, videoIds.Length == 0 ? null : videoIds, automation.Current.Enabled);
    return Results.Json(new { });
}).RequireRateLimiting("events");
app.MapMethods("/m/{token}/{id}", ["GET", "HEAD"], (string token, string id) =>
{
    if (!TokenEquals(token, settings.MediaToken)) return Results.NotFound();
    var item = repository.Media().FirstOrDefault(m => m.Id == id);
    var path = item is null ? null : files.Resolve(item.FileName);
    return path is null ? Results.NotFound() : Results.File(path, MediaFiles.ContentType(path), enableRangeProcessing: true);
});
app.MapMethods("/s/{token}/{id}.{extension}", ["GET", "HEAD"], (string token, string id, string extension) =>
{
    if (!TokenEquals(token, settings.MediaToken)) return Results.NotFound();
    var subtitle = repository.Subtitle(id);
    var path = subtitle is null || Path.GetExtension(subtitle.FileName) != "." + extension ? null : files.ResolveSubtitle(subtitle.FileName);
    return path is null ? Results.NotFound() : Results.File(path, extension == "vtt" ? "text/vtt; charset=utf-8" : "application/x-subrip; charset=utf-8");
});
Console.WriteLine($"C4K: painel local http://127.0.0.1:{adminPort}/admin/");
Console.WriteLine("Configure o qBittorrent no painel. O catálogo serve os arquivos completos diretamente de done.");
await app.RunAsync();

static bool TokenEquals(string? actual, string expected) => actual is not null &&
    CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(actual), Encoding.UTF8.GetBytes(expected));

public sealed record RegisterMedia(string Type, string MetaId, string VideoId, string Title, string FileName, int? Season, int? Episode);
public sealed record NetworkSettings(string PublicUrl);
public sealed record LibrarySettings(string Root);
public sealed record DownloadRetry(string Id, string? Magnet = null, int? FileIndex = null);
