param([string]$PublishedDirectory='artifacts/c4k-https-next',[string]$TlsConfigPath='artifacts/tls-fixture/tls.json')
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Net.Http
$workspace=Split-Path $PSScriptRoot -Parent
$runDir=Join-Path $workspace ('artifacts/tls-smoke-'+[guid]::NewGuid().ToString('N'))
$media=Join-Path $runDir 'done'
New-Item -ItemType Directory -Path $media -Force | Out-Null
[IO.File]::WriteAllBytes((Join-Path $media 'sample.mp4'),[byte[]](0..255))
function Free-Port { $listener=[Net.Sockets.TcpListener]::new([Net.IPAddress]::Any,0);$listener.Start();$port=$listener.LocalEndpoint.Port;$listener.Stop();return $port }
$ports=@();while($ports.Count -lt 3){$port=Free-Port;if($ports -notcontains $port){$ports+=@($port)}}
$tls=Get-Content -LiteralPath (Join-Path $workspace $TlsConfigPath) -Raw | ConvertFrom-Json
$tls.Port=$ports[2]
[IO.File]::WriteAllText((Join-Path $runDir 'tls.json'),($tls|ConvertTo-Json),[Text.UTF8Encoding]::new($false))
$https="https://$($tls.Address):$($tls.Port)"
$http="http://$($tls.Address):$($ports[0])"
$admin="http://127.0.0.1:$($ports[1])"
$variables=@{C4K_DATA_DIR=$runDir;C4K_MEDIA_ROOT=$media;C4K_ADDON_PORT=[string]$ports[0];C4K_ADMIN_PORT=[string]$ports[1];C4K_PUBLIC_URL=$https}
$previous=@{};foreach($name in $variables.Keys){$previous[$name]=[Environment]::GetEnvironmentVariable($name);[Environment]::SetEnvironmentVariable($name,$variables[$name])}
$handler=[Net.Http.HttpClientHandler]::new();$handler.UseProxy=$false
# Validacao TLS normal do Windows: nao ignorar erros de certificado.
$client=[Net.Http.HttpClient]::new($handler);$client.Timeout=[TimeSpan]::FromSeconds(10)
$checks=0;$process=$null
function Check($ok,$label){if(!$ok){throw "FAIL: $label"};$script:checks++}
function Request($method,$url,$body=$null,$headers=@{}){
    $request=[Net.Http.HttpRequestMessage]::new([Net.Http.HttpMethod]::new($method),$url)
    if($body){$request.Content=[Net.Http.StringContent]::new(($body|ConvertTo-Json -Compress),[Text.Encoding]::UTF8,'application/json')}
    foreach($name in $headers.Keys){$request.Headers.TryAddWithoutValidation($name,$headers[$name])|Out-Null}
    try{$r=$client.SendAsync($request).GetAwaiter().GetResult();$bytes=$r.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();return @{Status=[int]$r.StatusCode;Body=[Text.Encoding]::UTF8.GetString($bytes);Bytes=$bytes;Headers=$r.Headers}}
    finally{$request.Dispose()}
}
try{
    $exe=Join-Path (Join-Path $workspace $PublishedDirectory) 'C4K.App.exe'
    $process=Start-Process -FilePath $exe -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $runDir 'stdout.txt') -RedirectStandardError (Join-Path $runDir 'stderr.txt')
    $ready=$false;for($i=0;$i -lt 80;$i++){if($process.HasExited){throw 'HTTPS host encerrou; veja stderr em artifacts/tls-smoke-*'};try{if((Request GET "$admin/admin/position").Status -eq 404){$ready=$true;break}}catch{};Start-Sleep -Milliseconds 150}
    if(!$ready){throw 'Host nao iniciou.'}
    $auth=@{'X-C4K-Admin'='1'}
    $status=(Request GET "$admin/admin/api/status" $null $auth).Body|ConvertFrom-Json
    Check ($status.manifestUrl.StartsWith($https) -and $status.desktopManifestUrl -eq $status.manifestUrl) 'Manifest unico PC/TV com HTTPS'
    Check ((Request GET $status.manifestUrl).Status -eq 200) 'HTTPS com certificado confiavel e IP SAN valido'
    Check ((Request GET "$https/admin/").Status -eq 404) 'Admin indisponivel na porta HTTPS LAN'
    $path=([Uri]$status.manifestUrl).AbsolutePath
    Check ((Request GET ($http+$path)).Status -eq 200) 'Manifest HTTP anterior permanece disponivel'
    $certificate=Request GET "$http/ca.crt"
    Check ($certificate.Status -eq 200) 'CA publica pode ser baixada por HTTP'
    $public=[Security.Cryptography.X509Certificates.X509Certificate2]::new([byte[]]$certificate.Bytes)
    Check (!$public.HasPrivateKey -and $public.Thumbprint -eq $tls.RootThumbprint) 'Download inclui somente CA publica correta'
    $public.Dispose()
    Check ((Request GET "$https/ca.crt").Status -eq 200) 'CA publica tambem disponivel em HTTPS'
    $item=@{type='movie';metaId='c4k:tls';videoId='c4k:tls';title='TLS fixture';fileName='sample.mp4';season=$null;episode=$null}
    Check ((Request POST "$admin/admin/api/media" $item $auth).Status -eq 200) 'Cadastro de fixture isolada'
    $base=$status.manifestUrl.Replace('/manifest.json','')
    $stream=(Request GET "$base/stream/movie/c4k:tls.json").Body|ConvertFrom-Json
    Check ($stream.streams[0].url.StartsWith($https)) 'Manifest HTTPS anuncia video HTTPS local'
    $range=Request GET $stream.streams[0].url $null @{Range='bytes=10-19'}
    Check ($range.Status -eq 206 -and $range.Bytes.Length -eq 10 -and $range.Bytes[0] -eq 10) 'Range HTTPS correto'
    $legacyBase=$http+([Uri]$base).AbsolutePath
    $legacy=(Request GET "$legacyBase/stream/movie/c4k:tls.json").Body|ConvertFrom-Json
    Check ($legacy.streams[0].url.StartsWith($http)) 'Teste HTTP anuncia video HTTP e nao depende da CA na TV'
    Check ((Request GET $legacy.streams[0].url $null @{Range='bytes=0-3'}).Status -eq 206) 'Range HTTP preservado'
    $preflight=Request OPTIONS $status.manifestUrl $null @{Origin='https://web.stremio.com';'Access-Control-Request-Method'='GET'}
    Check ($preflight.Status -eq 204 -and $preflight.Headers.Contains('Access-Control-Allow-Origin')) 'CORS e preflight HTTPS'
    Check ($status.localHttps.fingerprint.Length -eq 64) 'Fingerprint publico SHA256 no painel'
    Write-Output "PASS: $checks HTTPS checks with normal certificate validation. Data: $runDir"
}finally{
    if($process -and !$process.HasExited){Stop-Process -Id $process.Id;$process.WaitForExit()}
    $client.Dispose();foreach($name in $previous.Keys){[Environment]::SetEnvironmentVariable($name,$previous[$name])}
}
