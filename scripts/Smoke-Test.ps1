param([switch]$Published, [string]$PublishedDirectory = 'artifacts/win-x64')
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Net.Http
$workspace = Split-Path $PSScriptRoot -Parent
$runDir = Join-Path $workspace ('artifacts/smoke-' + [guid]::NewGuid().ToString('N'))
$mediaDir = Join-Path $runDir 'media'
New-Item -ItemType Directory -Path $mediaDir -Force | Out-Null
[IO.File]::WriteAllBytes((Join-Path $mediaDir 'sample.mp4'), [byte[]](0..255))
# Fixture HTTP de 256 bytes; não é evidência de reprodução de um vídeo real.
function Free-Port {
    $listener = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, 0)
    $listener.Start(); $port = $listener.LocalEndpoint.Port; $listener.Stop(); return $port
}
$addonPort = Free-Port
do { $adminPort = Free-Port } while ($adminPort -eq $addonPort)
$addon = "http://127.0.0.1:$addonPort"
$admin = "http://127.0.0.1:$adminPort"
$variables = @{ C4K_DATA_DIR=$runDir; C4K_MEDIA_ROOT=$mediaDir; C4K_ADDON_PORT="$addonPort"; C4K_ADMIN_PORT="$adminPort"; C4K_PUBLIC_URL=$addon }
$previous = @{}
foreach($name in $variables.Keys) { $previous[$name] = [Environment]::GetEnvironmentVariable($name); [Environment]::SetEnvironmentVariable($name, $variables[$name]) }
$handler = [Net.Http.HttpClientHandler]::new()
$handler.UseProxy = $false
$handler.AllowAutoRedirect = $false
$client = [Net.Http.HttpClient]::new($handler)
$client.Timeout = [TimeSpan]::FromSeconds(10)
$script:checks = 0
function Check($condition, $description) {
    if (!$condition) { throw "FAIL: $description" }
    $script:checks++
}
function Request($method, $url, $body=$null, $headers=@{}) {
    $request = [Net.Http.HttpRequestMessage]::new([Net.Http.HttpMethod]::new($method), $url)
    if ($null -ne $body) { $request.Content = [Net.Http.StringContent]::new(($body | ConvertTo-Json -Compress), [Text.Encoding]::UTF8, 'application/json') }
    foreach($name in $headers.Keys) { $request.Headers.TryAddWithoutValidation($name, $headers[$name]) | Out-Null }
    try {
        $response = $client.SendAsync($request).GetAwaiter().GetResult()
        $bytes = $response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult()
        $text = [Text.Encoding]::UTF8.GetString($bytes)
        return @{ Status=[int]$response.StatusCode; Body=$text; Bytes=$bytes; Headers=$response.Headers; ContentHeaders=$response.Content.Headers }
    } finally { $request.Dispose() }
}
function Start-App {
    if($Published) {
        $exe = Join-Path (Join-Path $workspace $PublishedDirectory) 'C4K.App.exe'
        if(!(Test-Path -LiteralPath $exe)) { throw 'Execute dotnet publish antes do teste -Published.' }
        return Start-Process -FilePath $exe -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $runDir 'stdout.txt') -RedirectStandardError (Join-Path $runDir 'stderr.txt')
    }
    $dll = Join-Path $workspace 'src/C4K.App/bin/Release/net10.0/C4K.App.dll'
    return Start-Process -FilePath 'dotnet' -ArgumentList ('"' + $dll + '"') -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $runDir 'stdout.txt') -RedirectStandardError (Join-Path $runDir 'stderr.txt')
}
function Wait-App {
    for($i=0;$i -lt 80;$i++) {
        if($script:appProcess.HasExited) { throw 'Host encerrou antes de ficar pronto. Consulte artifacts/smoke-*/stderr.txt.' }
        try { if((Request 'GET' "$addon/health/live").Status -eq 200) { return } } catch { }
        Start-Sleep -Milliseconds 150
    }
    throw 'Host não iniciou dentro do limite.'
}
$appProcess = $null
try {
    $appProcess = Start-App; Wait-App
    $auth = @{ 'X-C4K-Admin'='1' }
    Check ((Request 'GET' "$admin/admin/").Status -eq 200) 'Painel loopback'
    $panel = Request 'GET' "$admin/admin/"
    Check ($panel.Body.Contains('<h1>C4K</h1>') -and $panel.Body.Contains('X-C4K-Admin')) 'Painel e autenticação usam C4K'
    Check ((Request 'GET' "$addon/admin/").Status -eq 404) 'Admin não atende na porta LAN'
    Check ((Request 'GET' "$admin/admin/api/status").Status -eq 403) 'Admin exige header não simples'
    Check ((Request 'GET' "$admin/admin/api/status" $null @{ 'X-C4K-Admin'='1'; Origin='https://invalid.example' }).Status -eq 403) 'Admin recusa origem externa'
    Check ((Request 'GET' "$admin/admin/" $null @{ Host='invalid.example' }).Status -eq 404) 'Admin recusa DNS rebinding por Host'
    $status = (Request 'GET' "$admin/admin/api/status" $null $auth).Body | ConvertFrom-Json
    Check ($status.installUrl.StartsWith('stremio:///addons?addon=') -and [Uri]::UnescapeDataString($status.installUrl.Substring('stremio:///addons?addon='.Length)) -eq $status.desktopManifestUrl) 'Link Stremio usa endereço de teste no PC'
    Check ($status.browserInstallUrl.StartsWith('https://web.stremio.com/#/addons?addon=') -and [Uri]::UnescapeDataString($status.browserInstallUrl.Substring('https://web.stremio.com/#/addons?addon='.Length)) -eq $status.manifestUrl) 'Link navegador preserva manifest LAN'
    $manifest = Request 'GET' $status.manifestUrl
    Check ($manifest.Status -eq 200) 'Manifest privado'
    Check ($manifest.Headers.Contains('Access-Control-Allow-Origin')) 'CORS no addon'
    $preflight = Request 'OPTIONS' $status.manifestUrl $null @{ Origin='https://web.stremio.com'; 'Access-Control-Request-Method'='GET'; 'Access-Control-Request-Headers'='content-type' }
    Check ($preflight.Status -eq 204 -and ($preflight.Headers.GetValues('Access-Control-Allow-Headers') -join ',') -match '(?i)content-type') 'Preflight permite Content-Type para clientes JSON'
    Check ((Request 'GET' "$addon/a/invalid/manifest.json").Status -eq 404) 'Token addon inválido'
    $prefix = $status.manifestUrl.Replace('/manifest.json','')
    $manifestUri = [Uri]$status.manifestUrl
    Check ($manifestUri.AbsolutePath -match '^/[a-f0-9]{10}/manifest\.json$') 'Manifest tem código curto para TV'
    Check ((Request 'GET' "$addon/0000000000/manifest.json").Status -eq 404) 'Alias incorreto recusado'
    $privateSettings = Get-Content -LiteralPath (Join-Path $runDir 'secrets.json') -Raw | ConvertFrom-Json
    Check ((Request 'GET' "$addon/a/$($privateSettings.AddonToken)/manifest.json").Status -eq 200) 'Manifest longo anterior continua funcionando'
    $item = @{ type='movie'; metaId='c4k:sample'; videoId='c4k:sample'; title='Amostra de diagnóstico'; fileName='sample.mp4'; season=$null; episode=$null }
    Check ((Request 'POST' "$admin/admin/api/media" $item $auth).Status -eq 200) 'Cadastro persistido'
    $item.fileName = '../outside.mp4'
    Check ((Request 'POST' "$admin/admin/api/media" $item $auth).Status -eq 400) 'Traversal rejeitado'
    $catalog = (Request 'GET' "$prefix/catalog/movie/c4k-ready.json").Body | ConvertFrom-Json
    Check ($catalog.metas.Count -eq 1) 'Catálogo contém arquivo local'
    $legacyCatalog = (Request 'GET' "$prefix/catalog/movie/casa4k-ready.json").Body | ConvertFrom-Json
    Check ($legacyCatalog.metas.Count -eq 1) 'Catálogo salvo antes da renomeação continua válido'
    $manifestBody = $manifest.Body | ConvertFrom-Json
    Check ($manifestBody.name.StartsWith('C4K') -and $manifestBody.id -eq 'local.c4k.probe') 'Manifest apresenta identidade C4K'
    $stream = (Request 'GET' "$prefix/stream/movie/c4k:sample.json").Body | ConvertFrom-Json
    $url = $stream.streams[0].url
    $range = Request 'GET' $url $null @{ Range='bytes=10-19' }
    Check ($range.Status -eq 206 -and $range.Bytes.Length -eq 10 -and $range.Bytes[0] -eq 10) 'Range e bytes corretos'
    Check ($range.ContentHeaders.ContentRange.ToString() -eq 'bytes 10-19/256') 'Content-Range correto'
    Check ((Request 'HEAD' $url).Bytes.Length -eq 0) 'HEAD sem corpo'
    Check ((Request 'GET' $url $null @{ Range='bytes=999-1000' }).Status -eq 416) 'Range inválido'
    Check ((Request 'GET' $url $null @{ Range='bytes=-8' }).Bytes.Length -eq 8) 'Suffix Range'
    Check ((Request 'GET' ($url -replace '/m/[^/]+/','/m/invalid/')).Status -eq 404) 'Token mídia inválido'
    $before = ((Request 'GET' "$admin/admin/api/status" $null $auth).Body | ConvertFrom-Json).events.Count
    Check ($before -eq 0) 'Consultar stream e HEAD não geram evento'
    $libraryExtra = [Uri]::EscapeDataString('action=libraryAdd')
    $libraryResponse = Request 'GET' "$prefix/library/series/tt1234567/$libraryExtra.json"
    Check ($libraryResponse.Status -eq 200) ("Evento libraryAdd: " + $libraryResponse.Status + ' ' + $libraryResponse.Body)
    $playerExtra = [Uri]::EscapeDataString('action=start&currentTime=250&duration=1000')
    Check ((Request 'GET' "$prefix/player/series/tt1234567:1:3/$playerExtra.json").Status -eq 200) 'Evento player start'
    $invalidExtra = [Uri]::EscapeDataString('action=start&currentTime=-1&duration=1000')
    Check ((Request 'GET' "$prefix/player/series/tt1234567:1:3/$invalidExtra.json").Status -eq 400) 'Tempo negativo recusado'
    Check ((Request 'GET' "$prefix/library/series/tt1234567/unknown=1.json").Status -eq 400) 'Evento sem action recusado sem erro interno'
    $state = (Request 'GET' "$admin/admin/api/status" $null $auth).Body | ConvertFrom-Json
    Check ($state.events.Count -eq 2 -and $state.events[0].currentTime -eq 250) ('Eventos persistidos com campos: ' + ($state.events | ConvertTo-Json -Compress))
    Stop-Process -Id $appProcess.Id; $appProcess.WaitForExit(); $appProcess = Start-App; Wait-App
    $state = (Request 'GET' "$admin/admin/api/status" $null $auth).Body | ConvertFrom-Json
    Check ($state.events.Count -eq 2 -and $state.media.Count -eq 1 -and $state.manifestUrl -eq $status.manifestUrl) 'Estado e pareamento sobrevivem ao reinício'
    # Tracer de importação: origem fora da biblioteca, trabalho persistido e publicação com legenda.
    function Wait-Import($id) {
        for($attempt=0;$attempt -lt 120;$attempt++) {
            $job = (Request 'GET' "$admin/admin/api/imports/$id" $null $auth).Body | ConvertFrom-Json
            if($job.state -in @('Ready','Failed')) { return $job }
            Start-Sleep -Milliseconds 100
        }
        throw 'Importação não concluiu no limite do teste.'
    }
    $sourcePath = Join-Path $runDir 'outside.mp4'
    $subtitlePath = Join-Path $runDir 'outside.srt'
    [IO.File]::WriteAllBytes($sourcePath, [byte[]](0..255))
    [IO.File]::WriteAllText($subtitlePath, "1`n00:00:00,000 --> 00:00:01,000`nSão arquivos locais.`n", [Text.UTF8Encoding]::new($false))
    $sourceHash = (Get-FileHash -LiteralPath $sourcePath).Hash
    $importMedia = @{ type='movie'; metaId='c4k:imported'; videoId='c4k:imported'; title='Importação com legenda'; fileName='pending.mp4'; season=$null; episode=$null }
    $payload = @{ media=$importMedia; sourcePath=$sourcePath; subtitlePath=$subtitlePath; subtitleLang='por' }
    $queued = Request 'POST' "$admin/admin/api/imports" $payload $auth
    Check ($queued.Status -eq 202) 'Importação aceita fora da raiz da biblioteca'
    $jobId = ($queued.Body | ConvertFrom-Json).id
    $job = Wait-Import $jobId
    Check ($job.state -eq 'Ready' -and $job.copiedBytes -eq 256) ('Importação completa e publicada: '+$job.error)
    Check ($job.method -eq 'Hardlink') 'Mesmo disco importa sem duplicar dados'
    $again = (Request 'POST' "$admin/admin/api/imports" $payload $auth).Body | ConvertFrom-Json
    Check ($again.id -eq $jobId) 'Importação repetida é idempotente'
    $importedStream = (Request 'GET' "$prefix/stream/movie/c4k:imported.json").Body | ConvertFrom-Json
    Check ($importedStream.streams.Count -eq 1 -and $importedStream.streams[0].subtitles.Count -eq 1) 'Stream anuncia legenda local'
    $subtitleUrl = $importedStream.streams[0].subtitles[0].url
    $subtitleResponse = Request 'GET' $subtitleUrl
    Check ($subtitleResponse.Status -eq 200 -and $subtitleResponse.Body.Contains('São arquivos locais.')) 'Legenda servida em UTF-8'
    Check ((Request 'GET' ($subtitleUrl -replace '/s/[^/]+/','/s/invalid/')).Status -eq 404) 'Legenda exige token de mídia'
    Check ((Get-FileHash -LiteralPath $sourcePath).Hash -eq $sourceHash) 'Arquivo original preservado'
    $payload.forceCopy = $true
    $payload.media.metaId = 'c4k:copied'; $payload.media.videoId = 'c4k:copied'
    $copyId = ((Request 'POST' "$admin/admin/api/imports" $payload $auth).Body | ConvertFrom-Json).id
    $copyJob = Wait-Import $copyId
    Check ($copyJob.state -eq 'Ready' -and $copyJob.method -eq 'Copy') 'Cópia independente conclui e publica'
    $payload.sourcePath = '\\invalid-server\share\video.mp4'
    Check ((Request 'POST' "$admin/admin/api/imports" $payload $auth).Status -eq 400) 'Importação não acessa pasta UNC'
    $payload.sourcePath = $sourcePath
    $badSubtitle = Join-Path $runDir 'invalid.srt'
    [IO.File]::WriteAllBytes($badSubtitle, [byte[]](255,255,255))
    $payload.subtitlePath = $badSubtitle
    $payload.media.metaId='c4k:invalidsub'; $payload.media.videoId='c4k:invalidsub'
    $failedId = ((Request 'POST' "$admin/admin/api/imports" $payload $auth).Body | ConvertFrom-Json).id
    $failedJob = Wait-Import $failedId
    Check ($failedJob.state -eq 'Failed') 'Legenda inválida gera falha controlada'
    $unpublished = (Request 'GET' "$prefix/stream/movie/c4k:invalidsub.json").Body | ConvertFrom-Json
    Check ($unpublished.streams.Count -eq 0) 'Importação que falhou não publica vídeo parcial'
    [IO.File]::WriteAllText($badSubtitle, "WEBVTT`n`n00:00.000 --> 00:01.000`nCorrigido", [Text.UTF8Encoding]::new($false))
    Check ((Request 'POST' "$admin/admin/api/imports/$failedId/retry" $null $auth).Status -eq 200) 'Falha permite retry'
    Check ((Wait-Import $failedId).state -eq 'Ready') 'Retry recupera importação'
    $payload.subtitlePath=$null; $payload.forceCopy=$false
    $payload.media.type='series';$payload.media.metaId='tt1234567';$payload.media.videoId='tt1234567:1:1';$payload.media.season=1;$payload.media.episode=1
    $seriesJob = ((Request 'POST' "$admin/admin/api/imports" $payload $auth).Body | ConvertFrom-Json).id
    Check ((Wait-Import $seriesJob).state -eq 'Ready') 'Importação de episódio canônico'
    $episodeMeta = (Request 'GET' "$prefix/meta/series/tt1234567.json").Body | ConvertFrom-Json
    Check ($episodeMeta.meta.videos[0].episode -eq 1) 'Metadados de série preservam episódio'
    $libraryState = (Request 'GET' "$admin/admin/api/status" $null $auth).Body | ConvertFrom-Json
    $importedId = ($libraryState.media | Where-Object {$_.videoId -eq 'c4k:imported'}).id
    Check ((Request 'DELETE' "$admin/admin/api/media/$importedId" $null $auth).Status -eq 200) 'Retirada do catálogo'
    Check ((Request 'GET' $importedStream.streams[0].url).Status -eq 404) 'Mídia retirada não é mais servida'
    Check ((Get-FileHash -LiteralPath $sourcePath).Hash -eq $sourceHash) 'Retirada mantém original'
    $oldManifest = $libraryState.manifestUrl
    $rotation = Request 'POST' "$admin/admin/api/pairing/rotate" $null $auth
    $newManifest = ($rotation.Body | ConvertFrom-Json).manifestUrl
    Check ($rotation.Status -eq 200 -and $newManifest -ne $oldManifest) 'Novo pareamento gera nova URL'
    Check ((Request 'GET' $oldManifest).Status -eq 404 -and (Request 'GET' $newManifest).Status -eq 200) 'Links anteriores são revogados'
    Check ((Request 'GET' $subtitleUrl).Status -eq 404) 'Rotação revoga também link de legenda'
    Check ((Request 'POST' "$admin/admin/api/network" @{publicUrl=$addon} $auth).Status -eq 409) 'Configuração de ambiente não é sobrescrita pelo painel'
    Stop-Process -Id $appProcess.Id; $appProcess.WaitForExit(); $env:C4K_PUBLIC_URL=$null; $appProcess=Start-App; Wait-App
    Check ((Request 'POST' "$admin/admin/api/network" @{publicUrl=$addon} $auth).Status -eq 200) 'Endereço local salvo pelo painel'
    Check ((Request 'POST' "$admin/admin/api/network" @{publicUrl="http://203.0.113.1:$addonPort"} $auth).Status -eq 400) 'Endereço externo rejeitado'
    Stop-Process -Id $appProcess.Id; $appProcess.WaitForExit(); $appProcess=Start-App; Wait-App
    $persisted = (Request 'GET' "$admin/admin/api/status" $null $auth).Body | ConvertFrom-Json
    Check ($persisted.manifestUrl -eq $newManifest -and $persisted.imports.Count -eq 4) 'Endereço, importações e pareamento persistem após reinício'
    $nextRoot = Join-Path $runDir 'library-next'
    Check ((Request 'POST' "$admin/admin/api/library-root" @{root=$nextRoot} $auth).Status -eq 409) 'Pasta definida no ambiente não é sobrescrita'
    Stop-Process -Id $appProcess.Id; $appProcess.WaitForExit(); $env:C4K_MEDIA_ROOT=$null; $appProcess=Start-App; Wait-App
    Check ((Request 'POST' "$admin/admin/api/library-root" @{root=$nextRoot} $auth).Status -eq 409) 'Troca de pasta protege biblioteca existente'
    foreach($mediaItem in $persisted.media) { Request 'DELETE' "$admin/admin/api/media/$($mediaItem.id)" $null $auth | Out-Null }
    Check ((Request 'POST' "$admin/admin/api/library-root" @{root='\\invalid-server\share'} $auth).Status -eq 400) 'Pasta de biblioteca UNC recusada'
    Check ((Request 'POST' "$admin/admin/api/library-root" @{root=$nextRoot} $auth).Status -eq 200) 'Pasta da biblioteca configurada pelo painel'
    Stop-Process -Id $appProcess.Id; $appProcess.WaitForExit(); $appProcess=Start-App; Wait-App
    $rootSettings = (Request 'GET' "$admin/admin/api/status" $null $auth).Body | ConvertFrom-Json
    Check ($rootSettings.mediaRoot -eq $nextRoot -and $rootSettings.manifestUrl -eq $newManifest) 'Pasta e pareamento persistem após reinício'
    Check ((Get-FileHash -LiteralPath $sourcePath).Hash -eq $sourceHash) 'Configuração de pasta mantém arquivo original'
    Write-Output "PASS: $script:checks verificações HTTP reais. Dados de teste: $runDir"
} finally {
    if($appProcess -and !$appProcess.HasExited) { Stop-Process -Id $appProcess.Id; $appProcess.WaitForExit() }
    $client.Dispose()
    foreach($name in $previous.Keys) { [Environment]::SetEnvironmentVariable($name, $previous[$name]) }
}
