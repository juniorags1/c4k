param(
    [string]$Address = '192.168.68.52',
    [int]$HttpsPort = 48242,
    [string]$DataDirectory = (Join-Path $env:LOCALAPPDATA 'C4K')
)
$ErrorActionPreference = 'Stop'
$ip = $null
$localAddresses = @(foreach ($adapter in [Net.NetworkInformation.NetworkInterface]::GetAllNetworkInterfaces()) {
    foreach ($unicast in $adapter.GetIPProperties().UnicastAddresses) { $unicast.Address.ToString() }
})
if (![Net.IPAddress]::TryParse($Address,[ref]$ip) -or $ip.AddressFamily -ne [Net.Sockets.AddressFamily]::InterNetwork -or
    $localAddresses -notcontains $Address) {
    throw 'Use um IPv4 configurado neste PC.'
}
if ($HttpsPort -lt 1024 -or $HttpsPort -gt 65535 -or $HttpsPort -in @(48240,48241)) { throw 'Porta HTTPS invalida.' }
New-Item -ItemType Directory -Path $DataDirectory -Force | Out-Null
$certificateDir = Join-Path $DataDirectory 'certificates'
New-Item -ItemType Directory -Path $certificateDir -Force | Out-Null
$tlsPath = Join-Path $DataDirectory 'tls.json'
$old = if (Test-Path -LiteralPath $tlsPath) { Get-Content -LiteralPath $tlsPath -Raw | ConvertFrom-Json } else { $null }
$root = $null; $server = $null
if ($old) {
    $root = Get-Item -LiteralPath ("Cert:\CurrentUser\My\" + $old.RootThumbprint) -ErrorAction SilentlyContinue
    if ($root -and (!$root.HasPrivateKey -or $root.NotAfter -lt (Get-Date).AddDays(90))) { $root = $null }
    if ($old.Address -eq $Address) {
        $server = Get-Item -LiteralPath ("Cert:\CurrentUser\My\" + $old.CertificateThumbprint) -ErrorAction SilentlyContinue
        if ($server -and (!$server.HasPrivateKey -or $server.NotAfter -lt (Get-Date).AddDays(30))) { $server = $null }
    }
}
if (!$root) {
    $root = New-SelfSignedCertificate -Type Custom -Subject 'CN=C4K Local Root CA' -FriendlyName 'C4K Local Root CA' `
        -CertStoreLocation 'Cert:\CurrentUser\My' -KeyAlgorithm RSA -KeyLength 3072 -HashAlgorithm SHA256 `
        -KeyExportPolicy NonExportable -KeyUsage CertSign,CRLSign -NotBefore (Get-Date).AddMinutes(-5) -NotAfter (Get-Date).AddYears(5) `
        -TextExtension @('2.5.29.19={critical}{text}ca=1&pathlength=0')
    $server = $null
}
if (!$server) {
    $server = New-SelfSignedCertificate -Type Custom -Subject ("CN=" + $Address) -FriendlyName 'C4K Local HTTPS' `
        -Signer $root -CertStoreLocation 'Cert:\CurrentUser\My' -KeyAlgorithm RSA -KeyLength 2048 -HashAlgorithm SHA256 `
        -KeyExportPolicy NonExportable -KeyUsage DigitalSignature,KeyEncipherment `
        -NotBefore (Get-Date).AddMinutes(-5) -NotAfter (Get-Date).AddYears(1) `
        -TextExtension @("2.5.29.17={text}IPAddress=$Address&IPAddress=127.0.0.1&DNS=localhost",'2.5.29.37={text}1.3.6.1.5.5.7.3.1','2.5.29.19={critical}{text}ca=0')
}
$publicPath = Join-Path $certificateDir 'C4K-Local-Root-CA.crt'
Export-Certificate -Cert $root -FilePath $publicPath -Type CERT -Force | Out-Null
# Instala somente a parte publica em CurrentUser\Root; as chaves permanecem nao exportaveis em My.
$publicRoot = [Security.Cryptography.X509Certificates.X509Certificate2]::new($publicPath)
$trust = [Security.Cryptography.X509Certificates.X509Store]::new('Root','CurrentUser')
try { $trust.Open([Security.Cryptography.X509Certificates.OpenFlags]::ReadWrite); $trust.Add($publicRoot) } finally { $trust.Close(); $publicRoot.Dispose() }
$utf8 = [Text.UTF8Encoding]::new($false)
$options = @{ CertificateThumbprint=$server.Thumbprint; RootThumbprint=$root.Thumbprint; Address=$Address; Port=$HttpsPort }
[IO.File]::WriteAllText($tlsPath+'.tmp',($options | ConvertTo-Json),$utf8)
Move-Item -LiteralPath ($tlsPath+'.tmp') -Destination $tlsPath -Force
$networkPath = Join-Path $DataDirectory 'network.json'
if ((Test-Path -LiteralPath $networkPath) -and !(Test-Path -LiteralPath (Join-Path $certificateDir 'network-before-https.json'))) {
    Copy-Item -LiteralPath $networkPath -Destination (Join-Path $certificateDir 'network-before-https.json')
}
[IO.File]::WriteAllText($networkPath+'.tmp',(@{ PublicUrl="https://${Address}:$HttpsPort" } | ConvertTo-Json),$utf8)
Move-Item -LiteralPath ($networkPath+'.tmp') -Destination $networkPath -Force
Write-Output ("HTTPS preparado: https://${Address}:$HttpsPort")
Write-Output ("Certificado publico para a TV: " + $publicPath)
Write-Output ("Validade do servidor: " + $server.NotAfter.ToString('yyyy-MM-dd'))
