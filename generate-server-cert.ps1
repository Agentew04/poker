#Requires -Version 5.1
<#
.SYNOPSIS
    Regenerates the self-signed TLS certificate used by the InstaPoker server.

.DESCRIPTION
    Creates a new RSA 4096 key pair valid for 10 years and writes:
      - instapoker-server.crt  (public certificate, embedded in the client)
      - instapoker-server.pfx  (certificate + private key, embedded in the server)

    This rotates the key pair. Every client built against the previous public certificate will refuse
    to connect and will show an "update your client" message, so both projects must be rebuilt and
    redistributed after running this script.

.PARAMETER PfxPassword
    Password used to protect the generated PFX. Must match ServerTls.PfxPassword.

.PARAMETER Days
    Certificate validity in days. Defaults to 3650 (10 years).

.PARAMETER DnsNames
    DNS names added to the certificate SAN. Defaults to localhost and InstaPoker.Server.

.PARAMETER IpAddresses
    IP addresses added to the certificate SAN. Defaults to 127.0.0.1.

.PARAMETER Force
    Skips the confirmation prompt.

.EXAMPLE
    .\generate-server-cert.ps1

.EXAMPLE
    .\generate-server-cert.ps1 -Force -DnsNames localhost,InstaPoker.Server,my-host.local
#>
[CmdletBinding()]
param(
    [string] $PfxPassword = "InstaPoker-Server-2026",
    [int] $Days = 3650,
    [string[]] $DnsNames = @("localhost", "InstaPoker.Server"),
    [string[]] $IpAddresses = @("127.0.0.1"),
    [switch] $Force
)

$ErrorActionPreference = "Stop"

$projectRoot = $PSScriptRoot
$certPath = Join-Path $projectRoot "instapoker-server.crt"
$pfxPath = Join-Path $projectRoot "instapoker-server.pfx"
$keyPath = Join-Path $projectRoot "instapoker-server.key"

if (-not $Force) {
    Write-Host "This will REPLACE the current server certificate and key pair." -ForegroundColor Yellow
    Write-Host "Clients built against the current certificate will stop connecting until they are rebuilt."
    $answer = Read-Host "Type YES to continue"
    if ($answer -cne "YES") {
        Write-Host "Aborted."
        return
    }
}

$openssl = $env:OPENSSL
if (-not $openssl) {
    $command = Get-Command openssl -ErrorAction SilentlyContinue
    if ($command) {
        $openssl = $command.Source
    }
}
if (-not $openssl) {
    throw "openssl was not found. Install it or set the OPENSSL environment variable to the openssl executable."
}

$sanParts = @()
$sanParts += $DnsNames | ForEach-Object { "DNS:$_" }
$sanParts += $IpAddresses | ForEach-Object { "IP:$_" }
$san = $sanParts -join ","

Write-Host "Generating a new RSA 4096 self-signed certificate (valid for $Days days)..."
& $openssl req -x509 -newkey rsa:4096 -sha256 -days $Days -nodes `
    -keyout $keyPath -out $certPath `
    -subj "/CN=InstaPoker.Server" `
    -addext "subjectAltName=$san" `
    -addext "basicConstraints=critical,CA:FALSE" `
    -addext "keyUsage=critical,digitalSignature,keyEncipherment" `
    -addext "extendedKeyUsage=serverAuth"
if ($LASTEXITCODE -ne 0) {
    throw "openssl req failed with exit code $LASTEXITCODE"
}

try {
    & $openssl pkcs12 -export -out $pfxPath -inkey $keyPath -in $certPath `
        -passout "pass:$PfxPassword" -name "InstaPoker.Server"
    if ($LASTEXITCODE -ne 0) {
        throw "openssl pkcs12 failed with exit code $LASTEXITCODE"
    }
}
finally {
    # the private key only exists inside the PFX after this point
    if (Test-Path $keyPath) {
        Remove-Item $keyPath -Force
    }
}

& $openssl x509 -in $certPath -noout -subject -issuer -dates -fingerprint -sha256

if ($DnsNames -notcontains "localhost") {
    Write-Warning "The certificate does not include 'localhost'. The client connects to localhost by default."
}

Write-Host ""
Write-Host "Done:" -ForegroundColor Green
Write-Host "  $certPath  (embedded in InstaPoker.Client)"
Write-Host "  $pfxPath   (embedded in InstaPoker.Server)"
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Rebuild BOTH projects so the new certificate is embedded."
Write-Host "  2. If you changed -PfxPassword, update ServerTls.PfxPassword in the server project."
Write-Host "  3. Rebuild and redistribute every client; old clients will be told to update."
