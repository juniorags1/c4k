# Executado pelo Smoke-Test.ps1 com processo e pastas isolados.
$config = @{ enabled=$false; provider='https://torrentio.strem.fun/manifest.json'; qbitUrl='http://127.0.0.1:8082'; username='fixture'; password='test-only-secret' }
Check ((Request 'POST' "$admin/admin/api/automation" $config $auth).Status -eq 200) 'Configuração de automação salva'
$automationStatus = Request 'GET' "$admin/admin/api/status" $null $auth
Check (!$automationStatus.Body.Contains('test-only-secret') -and !$automationStatus.Body.Contains('PasswordProtected')) 'Status não revela credenciais'
$storedConfig = [IO.File]::ReadAllText((Join-Path $runDir 'automation.json'))
Check (!$storedConfig.Contains('test-only-secret')) 'Credencial protegida no disco'
$config.qbitUrl='http://192.0.2.1:8082'
Check ((Request 'POST' "$admin/admin/api/automation" $config $auth).Status -eq 400) 'qBittorrent limitado ao PC local'
$wish = @{type='movie';videoId='tt7654321';title='Fixture';magnet=$null;fileIndex=$null}
Check ((Request 'POST' "$admin/admin/api/downloads" $wish $auth).Status -eq 202) 'Fila manual persistente'
Check ((Request 'POST' "$admin/admin/api/downloads" $wish $auth).Status -eq 202) 'Repetição idempotente'
$state = (Request 'GET' "$admin/admin/api/status" $null $auth).Body | ConvertFrom-Json
Check ($state.downloads.Count -eq 1 -and $state.downloads[0].state -eq 'Queued') 'Automação desativada não inicia download'
$outside = Join-Path $runDir 'outside.mp4';[IO.File]::WriteAllBytes($outside,[byte[]](0..255))
$local = @{ media=@{type='movie';metaId='c4k:direct';videoId='c4k:direct';title='Direto';fileName='pending.mp4';season=$null;episode=$null};sourcePath=$outside }
Check ((Request 'POST' "$admin/admin/api/imports" $local $auth).Status -eq 400) 'Importação externa sem cópia recusada'
$direct = Join-Path $mediaDir 'direct.mp4';[IO.File]::WriteAllBytes($direct,[byte[]](0..255));$local.sourcePath=$direct
Check ((Request 'POST' "$admin/admin/api/imports" $local $auth).Status -eq 200) 'Cadastro direto sem cópia'
$state = (Request 'GET' "$admin/admin/api/status" $null $auth).Body | ConvertFrom-Json
Check (($state.media | Where-Object videoId -eq 'c4k:direct').fileName -eq 'direct.mp4') 'Catálogo referencia o arquivo original'
Check ((Get-ChildItem -LiteralPath $mediaDir -File).Count -eq 2) 'Nenhuma duplicação de vídeo'
$newManifest = (Request 'POST' "$admin/admin/api/pairing/rotate" $null $auth).Body | ConvertFrom-Json
Check ((Request 'GET' $status.manifestUrl).Status -eq 404 -and (Request 'GET' $newManifest.manifestUrl).Status -eq 200) 'Rotação revoga links anteriores'
Stop-Process -Id $appProcess.Id; $appProcess.WaitForExit(); $appProcess = Start-App; Wait-App
$state = (Request 'GET' "$admin/admin/api/status" $null $auth).Body | ConvertFrom-Json
Check ($state.downloads.Count -eq 1 -and $state.automation.hasPassword -and $state.media.Count -eq 2) 'Configuração, fila e mídia persistem'
$posterPrefix = $state.manifestUrl.Replace('/manifest.json','')
$posterMovie = @{type='movie';metaId='tt12300742';videoId='tt12300742';title='Bugonia (2025)';fileName='direct.mp4';season=$null;episode=$null}
Check ((Request 'POST' "$admin/admin/api/media" $posterMovie $auth).Status -eq 200) 'Cadastro de filme com IMDb para capa'
$posterCatalog = (Request 'GET' "$posterPrefix/catalog/movie/c4k-ready.json").Body | ConvertFrom-Json
$canonical = $posterCatalog.metas | Where-Object id -eq 'tt12300742'
Check ($canonical.poster -eq 'https://images.metahub.space/poster/medium/tt12300742/img' -and $canonical.posterShape -eq 'poster') 'Catálogo anuncia capa HTTPS da obra'
Check ($null -eq ($posterCatalog.metas | Where-Object id -eq 'c4k:direct').poster) 'ID local não recebe capa de outra obra'
$posterSeries = @{type='series';metaId='tt1234567';videoId='tt1234567:1:2';title='Serie fixture';fileName='direct.mp4';season=1;episode=2}
Check ((Request 'POST' "$admin/admin/api/media" $posterSeries $auth).Status -eq 200) 'Cadastro de episódio para capa da série'
$seriesMeta = (Request 'GET' "$posterPrefix/meta/series/tt1234567.json").Body | ConvertFrom-Json
Check ($seriesMeta.meta.poster -eq 'https://images.metahub.space/poster/medium/tt1234567/img' -and $seriesMeta.meta.videos[0].episode -eq 2) 'Detalhes usam capa da série e preservam episódio'
$watchedExtra = [Uri]::EscapeDataString('action=watched&videoId=tt1234567:1:2')
Check ((Request 'GET' "$posterPrefix/library/series/tt1234567/$watchedExtra.json").Status -eq 200) 'Evento watched aceita episódio específico'
$watchedStatus = (Request 'GET' "$admin/admin/api/status" $null $auth).Body | ConvertFrom-Json
Check ($watchedStatus.events[0].changedVideos -eq 'tt1234567:1:2' -and $watchedStatus.watchedPlans.Count -eq 0) 'Evento preserva episódio e respeita automação desativada'
$listPage=Request 'GET' "$admin/list"
Check ($listPage.Status -eq 200 -and $listPage.Body.Contains('Biblioteca C4K')) 'Tela dedicada /list na porta administrativa'
Check ((Request 'GET' "$admin/list/").Status -eq 200) 'Tela /list aceita barra final'
Check ((Request 'GET' "$addon/list").Status -eq 404) 'Tela de gestão não exposta na porta LAN'
Check ((Request 'GET' "$admin/list" $null @{Host='invalid.example'}).Status -eq 404) 'Tela /list protege Host'
Check ((Request 'GET' "$admin/admin/api/library").Status -eq 403) 'API de biblioteca exige header administrativo'
$library=(Request 'GET' "$admin/admin/api/library" $null $auth).Body | ConvertFrom-Json
Check ($library.items.Count -eq 4 -and $library.totalBytes -eq 512) 'Resumo deduplica caminhos e informa total dos arquivos'
Check (($library.items | Where-Object videoId -eq 'tt12300742').sizeBytes -eq 256) 'Listagem informa tamanho do vídeo'
$main=Request 'GET' "$admin/admin/"
Check ($main.Body.Contains('href="/list"') -and !$main.Body.Contains('id="library"') -and !$main.Body.Contains('id="local"')) 'Página principal aponta para gestão separada'

$config.qbitUrl='http://127.0.0.1:8082'
$config.newDirectory=Join-Path $runDir 'custom-incomplete'
Check ((Request 'POST' "$admin/admin/api/automation" $config $auth).Status -eq 200) 'Pasta de incompletos configur?vel pela API'
Stop-Process -Id $appProcess.Id; $appProcess.WaitForExit(); $appProcess=Start-App; Wait-App
$customState=(Request 'GET' "$admin/admin/api/status" $null $auth).Body | ConvertFrom-Json
Check ($customState.automation.newDirectory -eq $config.newDirectory) 'Pasta de incompletos persiste ap?s rein?cio'
$config.newDirectory='relative'
Check ((Request 'POST' "$admin/admin/api/automation" $config $auth).Status -eq 400) 'Pasta de incompletos relativa recusada'
Check ((Request 'POST' "$admin/admin/api/library-root" @{root=$otherRoot} $auth).Status -eq 409) 'Troca de completos bloqueada com download pendente'
Check ($main.Body.Contains('id="newDirectory"') -and $main.Body.Contains('id="completedDirectory"')) 'Painel exp?e ambas as pastas'

$cleanupPrefix=$customState.manifestUrl.Replace('/manifest.json','')
$watchedExtra=[Uri]::EscapeDataString('action=watched')
Check ((Request 'GET' "$cleanupPrefix/library/movie/tt7654321/$watchedExtra.json").Status -eq 200) 'Watched de filme aceito'
$cleanStatus=(Request 'GET' "$admin/admin/api/status" $null $auth).Body | ConvertFrom-Json
$scheduled=@($cleanStatus.cleanup | Where-Object videoId -eq 'tt7654321')
Check ($scheduled.Count -eq 1 -and ([DateTimeOffset]::Parse($scheduled[0].deleteAfter)-[DateTimeOffset]::Parse($scheduled[0].watchedAt)).TotalHours -eq 72) 'HTTP agenda limpeza em 72 horas com downloads desativados'
$unwatchedExtra=[Uri]::EscapeDataString('action=unwatched')
Check ((Request 'GET' "$cleanupPrefix/library/movie/tt7654321/$unwatchedExtra.json").Status -eq 200) 'Unwatched de filme aceito'
$cleanStatus=(Request 'GET' "$admin/admin/api/status" $null $auth).Body | ConvertFrom-Json
Check (@($cleanStatus.cleanup | Where-Object videoId -eq 'tt7654321').Count -eq 0) 'Unwatched cancela limpeza pela API'

Check ((Request 'POST' "$admin/admin/api/downloads/cancel" @{id='movie:tt7654321'}).Status -eq 403) 'Cancelar exige autenticação administrativa'
Check ((Request 'POST' "$admin/admin/api/downloads/cancel" @{id='unknown'} $auth).Status -eq 404) 'Cancelar tarefa inexistente retorna 404'
Check ((Request 'POST' "$admin/admin/api/downloads/cancel" @{id='movie:tt7654321'} $auth).Status -eq 409) 'Cancelar rejeita tarefa ativa'
Check ($main.Body.Contains("n('button','Cancelar')") -and $main.Body.Contains("api('downloads/cancel'")) 'Painel apresenta Cancelar para downloads com falha'
