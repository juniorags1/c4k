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
    # Troca real de raiz com dados isolados, sem override de ambiente.
    Stop-Process -Id $appProcess.Id; $appProcess.WaitForExit()
    [Environment]::SetEnvironmentVariable('C4K_MEDIA_ROOT', $null)
    [IO.File]::WriteAllText((Join-Path $runDir 'library.json'), (@{Root=$mediaDir} | ConvertTo-Json))
    $appProcess=Start-App; Wait-App
    $otherRoot=Join-Path $runDir 'other-completed'
    Check ((Request 'POST' "$admin/admin/api/library-root" @{root=$otherRoot} $auth).Status -eq 409) 'Troca recusa destino sem v?deos cadastrados'
    New-Item -ItemType Directory -Path $otherRoot -Force | Out-Null
    Copy-Item -LiteralPath (Join-Path $mediaDir 'sample.mp4') -Destination $otherRoot
    Check ((Request 'POST' "$admin/admin/api/library-root" @{root=$otherRoot} $auth).Status -eq 200) 'Troca aceita biblioteca existente com arquivo no destino'
    Stop-Process -Id $appProcess.Id; $appProcess.WaitForExit(); $appProcess=Start-App; Wait-App
    $relocated=(Request 'GET' "$admin/admin/api/status" $null $auth).Body | ConvertFrom-Json
    Check ($relocated.mediaRoot -eq $otherRoot -and $relocated.media.Count -eq 1 -and $relocated.manifestUrl -eq $status.manifestUrl) 'Pasta de completos persiste com biblioteca e manifest preservados'
    Check ((Request 'POST' "$admin/admin/api/library-root" @{root=$mediaDir} $auth).Status -eq 200) 'Restaura pasta da fixture'
    . (Join-Path $PSScriptRoot 'Smoke-Automation-Checks.ps1')
    Write-Output "PASS: $script:checks HTTP checks. Data: $runDir"
} finally {
    if($appProcess -and !$appProcess.HasExited) { Stop-Process -Id $appProcess.Id; $appProcess.WaitForExit() }
    $client.Dispose()
    foreach($name in $previous.Keys) { [Environment]::SetEnvironmentVariable($name, $previous[$name]) }
}
