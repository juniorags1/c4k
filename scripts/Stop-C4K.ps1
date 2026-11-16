$ErrorActionPreference = 'Stop'
$workspace = Split-Path $PSScriptRoot -Parent
$exe = Join-Path $workspace 'artifacts/win-x64/C4K.App.exe'
$pidFile = Join-Path $workspace 'artifacts/server.pid'
if (!(Test-Path -LiteralPath $pidFile)) { Write-Output 'Nenhum processo registrado pelo launcher.'; exit 0 }
$registeredId = [int]([IO.File]::ReadAllText($pidFile))
$process = Get-Process -Id $registeredId -ErrorAction SilentlyContinue
if ($process) {
    if ($process.Path -ne $exe) { throw 'O PID pertence a outro executável. Processo preservado.' }
    Stop-Process -Id $registeredId
}
Remove-Item -LiteralPath $pidFile
Write-Output 'C4K encerrado.'
