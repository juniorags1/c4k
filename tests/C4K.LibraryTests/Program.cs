using C4K;
using Microsoft.Data.Sqlite;
using System.Text.Json;
using System.Net;

var directory = Path.Combine(Path.GetTempPath(), "c4k-recovery-" + Guid.NewGuid().ToString("N"));
var root = Path.Combine(directory, "media");
Directory.CreateDirectory(root);
var checks = 0;
void Check(bool condition, string description)
{
    if (!condition) throw new Exception("FAIL: " + description);
    checks++;
}

// Banco no formato da versão 0.1: garante migração aditiva de uma instalação existente.
using (var old = new SqliteConnection($"Data Source={Path.Combine(directory, "c4k.db")}"))
{
    old.Open(); using var command = old.CreateCommand();
    command.CommandText = """
        CREATE TABLE media(id TEXT PRIMARY KEY,type TEXT NOT NULL,meta_id TEXT NOT NULL,video_id TEXT NOT NULL UNIQUE,
        title TEXT NOT NULL,file_name TEXT NOT NULL,season INTEGER,episode INTEGER);
        INSERT INTO media VALUES ('legacy','movie','c4k:legacy','c4k:legacy','Biblioteca anterior','legacy.mp4',NULL,NULL);
        """;
    command.ExecuteNonQuery();
}
var repository = new Repository(directory);
Check(repository.Media().Single().Id == "legacy", "Migração preserva o registro anterior");
var source = Path.Combine(directory, "source.mp4");
var bytes = Enumerable.Range(0, 4096).Select(n => (byte)(n % 251)).ToArray();
await File.WriteAllBytesAsync(source, bytes);
var request = new ImportRequest(new RegisterMedia("movie", "c4k:recovery", "c4k:recovery", "Recuperação", "pending.mp4", null, null), source, null, null, ForceCopy: true);
var job = repository.QueueImport(request);
repository.ImportState(job.Id, "Importing", 17, "Copy");
var partial = Path.Combine(root, job.Id + ".mp4.partial");
await File.WriteAllBytesAsync(partial, bytes[..17]);
Check(repository.Media().Count == 1, "Arquivo interrompido não está publicado");

// Novo repositório/worker, simulando retorno após encerramento do processo anterior.
var restarted = new Repository(directory);
using var worker = new ImportWorker(restarted, new MediaFiles(root));
await worker.StartAsync(CancellationToken.None);
using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
while (restarted.Imports(job.Id).Single().State is not ("Ready" or "Failed"))
    await Task.Delay(50, timeout.Token);
await worker.StopAsync(CancellationToken.None);
var finished = restarted.Imports(job.Id).Single();
Check(finished.State == "Ready" && finished.Method == "Copy", "Worker recupera importação interrompida");
var item = restarted.Media().Single(m => m.VideoId == "c4k:recovery");
var destination = Path.Combine(root, item.FileName);
Check((await File.ReadAllBytesAsync(destination)).SequenceEqual(bytes), "Publicação contém todos os bytes");
Check(!File.Exists(partial), "Parcial interrompido foi substituído");
Check((await File.ReadAllBytesAsync(source)).SequenceEqual(bytes), "Fonte foi preservada na recuperação");
await File.WriteAllBytesAsync(source, [7, 8, 9]);
Check((await File.ReadAllBytesAsync(destination)).SequenceEqual(bytes), "Cópia independente não muda com o original");
var automationDirectory = Path.Combine(directory, "automation"); Directory.CreateDirectory(automationDirectory);
var automation = new AutomationSettings(automationDirectory);
automation.Save(new(true, "https://torrentio.strem.fun/manifest.json", "http://127.0.0.1:8082", "admin", "fixture-secret"));
Check(automation.Password == "fixture-secret" && !File.ReadAllText(Path.Combine(automationDirectory, "automation.json")).Contains("fixture-secret"), "Senha protegida com DPAPI");
Check(!JsonSerializer.Serialize(automation.Public()).Contains("fixture-secret"), "Status não revela senha");
var downloads = new Repository(automationDirectory);
downloads.Record("library", "series", "tt1234567", "libraryAdd", null, null, null, true);
downloads.Record("library", "series", "tt1234567", "libraryAdd", null, null, null, true);
Check(downloads.Downloads().Single().Request.VideoId == "tt1234567:1:1", "LibraryAdd persiste e deduplica S1E1");
Check(new Repository(automationDirectory).Downloads().Count == 1, "Solicitação sobrevive reinício");
var hash = new string('a', 40);
using var result = JsonDocument.Parse(JsonSerializer.Serialize(new { streams = new[] {
    new { name="Torrentio\n1080p", title="👤 9999", infoHash=new string('b',40), fileIdx=0 },
    new { name="Torrentio\n4k", title="👤 12", infoHash=hash, fileIdx=1 },
    new { name="Torrentio\n4k", title="sem seeds", infoHash=new string('c',40), fileIdx=0 },
    new { name="Torrentio\n2160p", title="👤 4", infoHash=new string('d',40), fileIdx=0 } } }));
var candidate = TorrentSource.Select(result.RootElement);
Check(candidate?.Hash == hash && candidate.Seeds == 12 && candidate.FileIndex == 1, "Escolhe maior seed 4K, ignora 1080p e desconhecidos");
using var missing = JsonDocument.Parse("{\"streams\":[]}");
Check(TorrentSource.Select(missing.RootElement) is null, "Sem fonte não inventa qualidade");
var downloadRoot = Path.Combine(automationDirectory, "done"); Directory.CreateDirectory(downloadRoot);
var mediaFiles = new MediaFiles(downloadRoot);
var request2 = new DownloadRequest("movie", "tt7654321", "Fixture 4K", "magnet:?xt=urn:btih:"+hash, 1);
var queued = downloads.QueueDownload(request2);
var handler = new QbitFixture(downloadRoot, hash);
using var client = new QbitClient("http://127.0.0.1:8082", handler);
using var downloadWorker = new DownloadWorker(downloads, mediaFiles, automation);
await downloadWorker.Process(queued, client, CancellationToken.None);
var durable = downloads.Downloads().Single(j=>j.Id==queued.Id);
Check(durable.Hash == hash && handler.Adds == 1 && handler.AddBody.Contains("downloadPath=C%3A%5Cmedia%5Cnew") && handler.AddBody.Contains("stopped=true"), "Magnet e caminhos enviados; hash salvo antes do retorno");
await downloadWorker.Process(durable, client, CancellationToken.None);
Check(handler.Adds == 1 && downloads.Downloads().Single(j=>j.Id==queued.Id).Selected, "Recuperação por hash não duplica torrent; seleciona vídeo");
Check(handler.Commands.Any(c=>c.Contains("id=0") && c.Contains("priority=0")), "Desmarca outros arquivos do pacote");
durable = downloads.Downloads().Single(j=>j.Id==queued.Id);
await downloadWorker.Process(durable, client, CancellationToken.None);
Check(downloads.Media().Count == 0, "Arquivo incompleto não publicado");
handler.Complete = true; handler.Moving = true;
await File.WriteAllBytesAsync(Path.Combine(downloadRoot,"video.mp4"), [1,2,3,4]);
await downloadWorker.Process(durable, client, CancellationToken.None);
Check(downloads.Media().Count == 0, "Arquivo movendo não publicado");
handler.Moving = false;
await downloadWorker.Process(durable, client, CancellationToken.None);
Check(downloads.Media().Single().FileName == "video.mp4" && Directory.GetFiles(downloadRoot).Length == 1, "Publica arquivo original sem cópia");
Check(downloads.Downloads().Single(j=>j.Id==queued.Id).State == "Ready", "Conclusão persistida");
Check(mediaFiles.Resolve("../source.mp4") is null, "Traversal de publicação recusado");
var video = downloads.Media().Single();
await File.WriteAllBytesAsync(Path.Combine(downloadRoot, "other.mp4"), [5,6,7,8]);
using var deleting = new DownloadWorker(downloads, mediaFiles, automation, () => new QbitClient("http://127.0.0.1:8082", new QbitFixture(downloadRoot, hash) { Adds = 1, Complete = true }));
Check(await deleting.Delete(video.Id, CancellationToken.None), "Exclusão física de fixture gerenciada");
Check(!File.Exists(Path.Combine(downloadRoot,"video.mp4")) && File.Exists(Path.Combine(downloadRoot,"other.mp4")), "Apaga só o vídeo registrado, mantém outro arquivo do pacote");
Check(downloads.Media().Count == 0 && downloads.Downloads().Single(j=>j.Id==queued.Id).State == "Removed", "Exclusão retira catálogo e impede republicação");
foreach (var modern in new[] { true, false })
{
    using var authClient = new QbitClient("http://127.0.0.1:8082", new AuthenticationFixture(modern));
    Check(await authClient.Connect("fixture", "fixture", CancellationToken.None) == "v5.3.0", modern ? "Login 204 vazio confirma sessão" : "Login legado 200 confirma sessão");
    await authClient.Post("torrents/add", new() { ["urls"] = "fixture" }, CancellationToken.None);
    Check(true, modern ? "Magnet aceita sucesso 204 vazio" : "Magnet aceita sucesso legado 200/Ok.");
}
using (var failedAuth = new QbitClient("http://127.0.0.1:8082", new AuthenticationFixture(false, true)))
{
    var rejected = false;
    try { await failedAuth.Connect("fixture", "wrong", CancellationToken.None); }
    catch (InvalidOperationException) { rejected = true; }
    Check(rejected, "Login legado 200/Fails não vira falso sucesso");
}
foreach (var scenario in new[] {
    (HttpStatusCode.OK,"{\"success_count\":1,\"failure_count\":0,\"pending_count\":0}",true),
    (HttpStatusCode.Accepted,"{\"success_count\":0,\"failure_count\":0,\"pending_count\":1}",true),
    (HttpStatusCode.OK,"{\"success_count\":0,\"failure_count\":1,\"pending_count\":0}",false),
    (HttpStatusCode.OK,"unexpected",false) })
{
    using var addClient = new QbitClient("http://127.0.0.1:8082", new AddResultFixture(scenario.Item1,scenario.Item2));
    var accepted = true;
    try { await addClient.Post("torrents/add", new() { ["urls"]="fixture" }, CancellationToken.None); }
    catch (InvalidOperationException) { accepted = false; }
    Check(accepted == scenario.Item3, "Interpreta resultado JSON/sucesso assíncrono/recusa sem falso positivo");
}
using (var titleJson = JsonDocument.Parse("{\"meta\":{\"id\":\"tt12300742\",\"name\":\"Bugonia\",\"year\":\"2025\"}}"))
{
    Check(MediaNames.FromMeta(titleJson.RootElement,"tt12300742") == "Bugonia (2025)", "Título público com ano");
    Check(MediaNames.FromMeta(titleJson.RootElement,"tt7654321") is null, "Não associa título de outro IMDb");
}
Check(MediaNames.Display(new("series","tt1234567:2:3","Série")) == "Série — T02E03", "Nome do torrent distingue episódio");
Check(handler.AddBody.Contains("rename=Fixture+4K") && handler.AddBody.Contains("dn%3DFixture%25204K"), "Título enviado como display name e dn sem renomear arquivo");
Console.WriteLine($"PASS: {checks} verificações de migração, proteção, seleção e automação com API simulada.");

sealed class AddResultFixture(HttpStatusCode status, string body) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
        Task.FromResult(new HttpResponseMessage(status) { Content = new StringContent(body) });
}

sealed class AuthenticationFixture(bool modern, bool fail = false) : HttpMessageHandler
{
    private bool loggedIn;
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.RequestUri!.AbsolutePath.EndsWith("/login"))
        {
            loggedIn = !fail;
            return Task.FromResult(new HttpResponseMessage(modern ? HttpStatusCode.NoContent : HttpStatusCode.OK)
                { Content = new StringContent(modern ? "" : fail ? "Fails." : "Ok.") });
        }
        if (request.RequestUri.AbsolutePath.EndsWith("/add"))
            return Task.FromResult(new HttpResponseMessage(modern ? HttpStatusCode.NoContent : HttpStatusCode.OK) { Content = new StringContent(modern ? "" : "Ok.") });
        return Task.FromResult(new HttpResponseMessage(loggedIn ? HttpStatusCode.OK : HttpStatusCode.Forbidden) { Content = new StringContent(loggedIn ? "v5.3.0" : "Forbidden") });
    }
}

sealed class QbitFixture(string root, string hash) : HttpMessageHandler
{
    public int Adds; public string AddBody = ""; public bool Complete; public bool Moving;
    public List<string> Commands = [];
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var path = request.RequestUri!.AbsolutePath;
        var body = request.Content is null ? "" : await request.Content.ReadAsStringAsync(cancellationToken);
        if (request.Method == HttpMethod.Post) { Commands.Add(path+"?"+body); if(path.EndsWith("/add")){Adds++;AddBody=body;} return Response("Ok."); }
        if (path.EndsWith("/info")) return Response(JsonSerializer.Serialize(Adds==0 ? Array.Empty<object>() : new object[] {new { hash, tags="C4K", save_path=root, state=Moving?"moving":"stalledUP", amount_left=Complete?0:4 }}));
        if (path.EndsWith("/files")) return Response(JsonSerializer.Serialize(new[] {
            new { index=0,name="other.mp4",progress=0.0,size=4 },new { index=1,name="video.mp4",progress=Complete?1.0:0.5,size=4 } }));
        return Response("v5.3.0");
    }
    private static HttpResponseMessage Response(string text)=>new(HttpStatusCode.OK){Content=new StringContent(text)};
}
