$ErrorActionPreference = 'Stop'
$workspace = Split-Path $PSScriptRoot -Parent
$exe = Join-Path $workspace 'artifacts/win-x64/Casa4K.App.exe'
if (!(Test-Path -LiteralPath $exe)) {
    throw 'Publique primeiro: dotnet publish src/Casa4K.App -c Release -r win-x64 --self-contained true -o artifacts/win-x64'
}
if ($args -contains '-Background') {
    $pidFile = Join-Path $workspace 'artifacts/server.pid'
    if (Test-Path -LiteralPath $pidFile) {
        $existingId = [int]([IO.File]::ReadAllText($pidFile))
        $existing = Get-Process -Id $existingId -ErrorAction SilentlyContinue
        if ($existing -and $existing.Path -eq $exe) { Write-Output 'Casa4K já está iniciado.'; exit 0 }
    }
    $process = Start-Process -FilePath $exe -WindowStyle Hidden -PassThru
    [IO.File]::WriteAllText($pidFile, [string]$process.Id)
    Write-Output 'Casa4K iniciado. Painel: http://127.0.0.1:48241/admin/'
} else {
    & $exe
    exit $LASTEXITCODE
}
