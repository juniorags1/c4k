using C4K;
using Microsoft.Data.Sqlite;

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
Console.WriteLine($"PASS: {checks} verificações de migração e recuperação com SQLite e arquivos reais.");
