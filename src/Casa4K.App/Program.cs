using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;
using Casa4K;
using Microsoft.AspNetCore.WebUtilities;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders(); // URLs de pareamento não podem aparecer nos logs HTTP.
var dataDir = Environment.GetEnvironmentVariable("CASA4K_DATA_DIR") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Casa4K");
var settings = AppSettings.Load(dataDir);
var mediaRoot = Environment.GetEnvironmentVariable("CASA4K_MEDIA_ROOT") ?? Path.Combine(dataDir, "media");
Directory.CreateDirectory(mediaRoot);
var files = new MediaFiles(mediaRoot);
var repository = new Repository(dataDir);
var addonPort = int.Parse(Environment.GetEnvironmentVariable("CASA4K_ADDON_PORT") ?? "48240", CultureInfo.InvariantCulture);
var adminPort = int.Parse(Environment.GetEnvironmentVariable("CASA4K_ADMIN_PORT") ?? "48241", CultureInfo.InvariantCulture);
if (addonPort == adminPort || addonPort is < 1024 or > 65535 || adminPort is < 1024 or > 65535)
    throw new InvalidOperationException("Portas devem ser distintas e estar entre 1024 e 65535.");
var publicUrl = AppSettings.PublicUrl(addonPort);
// Alias curto para digitação no controle da TV. Restrito ao uso doméstico em LAN.
var shortAddonCode = settings.AddonToken[..10];
var manifestUrl = $"{publicUrl}/{shortAddonCode}/manifest.json";
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
    if (context.Request.Path.StartsWithSegments($"/{shortAddonCode}", out var remainder))
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
            if (context.Request.Headers["X-Casa4K-Admin"] != "1") { context.Response.StatusCode = 403; return; }
            var origin = context.Request.Headers.Origin.ToString();
            if (origin.Length > 0 && origin != $"http://{context.Request.Host}") { context.Response.StatusCode = 403; return; }
        }
    }
    else if (context.Connection.LocalPort != addonPort) { context.Response.StatusCode = 404; return; }
    if (path.StartsWithSegments("/a") || path.StartsWithSegments("/m"))
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
app.MapGet("/admin/", () => Results.Content(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Panel.html")), "text/html; charset=utf-8"));
app.MapGet("/admin/api/status", () => new
{
    manifestUrl,
    installUrl = $"stremio:///addons?addon={Uri.EscapeDataString(manifestUrl)}",
    browserInstallUrl = $"https://web.stremio.com/#/addons?addon={Uri.EscapeDataString(manifestUrl)}",
    publicUrl, mediaRoot = files.Root, media = repository.Media(), events = repository.Events(),
    warning = "Prova de compatibilidade: downloads e antecipação ainda não implementados. Eventos reais da TV não foram validados."
});
app.MapPost("/admin/api/media", (RegisterMedia input) =>
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
});
app.MapGet("/health/live", () => Results.Ok(new { status = "ok", stage = "compatibility-probe" }));

var addon = app.MapGroup("/a/{token}").AddEndpointFilter(async (context, next) =>
{
    return TokenEquals(context.HttpContext.Request.RouteValues["token"]?.ToString(), settings.AddonToken)
        ? await next(context) : Results.NotFound();
});
addon.MapGet("/manifest.json", () => Results.Json(new
{
    id = "local.casa4k.probe", version = "0.1.0", name = "Casa4K — teste local",
    description = "Arquivos completos no PC e diagnóstico de eventos. Não realiza downloads nesta versão.",
    types = new[] { "movie", "series" },
    resources = new object[] { "catalog", new { name = "meta", types = new[] { "movie", "series" }, idPrefixes = new[] { "tt", "c4k:" } },
        new { name = "stream", types = new[] { "movie", "series" }, idPrefixes = new[] { "tt", "c4k:" } },
        new { name = "player", types = new[] { "movie", "series" }, idPrefixes = new[] { "tt", "c4k:" } },
        new { name = "library", types = new[] { "movie", "series" }, idPrefixes = new[] { "tt", "c4k:" } } },
    catalogs = new[] { new { type = "movie", id = "casa4k-ready", name = "Casa4K — filmes no PC" }, new { type = "series", id = "casa4k-ready", name = "Casa4K — séries no PC" } }
}));
addon.MapGet("/catalog/{type}/{id}.json", (string type, string id) => Results.Json(new
{
    metas = id != "casa4k-ready" ? [] : repository.Media().Where(m => m.Type == type && files.Resolve(m.FileName) is not null)
        .DistinctBy(m => m.MetaId).Select(m => new { id = m.MetaId, type = m.Type, name = m.Title }).ToArray()
}));
addon.MapGet("/meta/{type}/{id}.json", (string type, string id) =>
{
    var items = repository.Media().Where(m => m.Type == type && m.MetaId == id && files.Resolve(m.FileName) is not null).ToList();
    return items.Count == 0 ? Results.NotFound() : Results.Json(new { meta = new { id, type, name = items[0].Title,
        videos = items.Select(m => new { id = m.VideoId, title = m.Title, season = m.Season, episode = m.Episode }).ToArray() } });
});
addon.MapGet("/stream/{type}/{id}.json", (string type, string id) =>
{
    var item = repository.Media().FirstOrDefault(m => m.Type == type && m.VideoId == id && files.Resolve(m.FileName) is not null);
    return item is null ? Results.Json(new { streams = Array.Empty<object>() }) : Results.Json(new
    {
        streams = new[] { new { name = "Casa4K — disponível no PC", url = $"{publicUrl}/m/{settings.MediaToken}/{item.Id}",
            behaviorHints = new { notWebReady = true, bingeGroup = "casa4k-local", filename = Path.GetFileName(item.FileName), videoSize = new FileInfo(files.Resolve(item.FileName)!).Length } } }
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
    repository.Record(resource, type, id, action, time, duration, videoIds.Length == 0 ? null : videoIds);
    return Results.Json(new { });
}).RequireRateLimiting("events");
app.MapMethods("/m/{token}/{id}", ["GET", "HEAD"], (string token, string id) =>
{
    if (!TokenEquals(token, settings.MediaToken)) return Results.NotFound();
    var item = repository.Media().FirstOrDefault(m => m.Id == id);
    var path = item is null ? null : files.Resolve(item.FileName);
    return path is null ? Results.NotFound() : Results.File(path, MediaFiles.ContentType(path), enableRangeProcessing: true);
});
Console.WriteLine($"Casa4K: painel local http://127.0.0.1:{adminPort}/admin/");
Console.WriteLine("Cadastre um arquivo completo e copie o manifest pelo painel. Nenhum download é iniciado nesta versão.");
await app.RunAsync();

static bool TokenEquals(string? actual, string expected) => actual is not null &&
    CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(actual), Encoding.UTF8.GetBytes(expected));

public sealed record RegisterMedia(string Type, string MetaId, string VideoId, string Title, string FileName, int? Season, int? Episode);
