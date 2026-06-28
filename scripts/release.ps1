param(
    [string]$Configuration = "Release",
    [string]$OutputDir = "dist",
    [switch]$SkipDeterministicCheck
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$solution = Join-Path $root "PowerUpSQLSharp.sln"
$builtExe = Join-Path $root "src\PowerUpSQLSharp.CLI\bin\$Configuration\net48\PowerUpSQLSharp.exe"
$builtCoreDll = Join-Path $root "src\PowerUpSQLSharp.CLI\bin\$Configuration\net48\PowerUpSQLSharp.Core.dll"
$dest = Join-Path $root $OutputDir
$artifact = Join-Path $dest "PowerUpSQLSharp.exe"
$verify = Join-Path $PSScriptRoot "verify-strip.ps1"

function Invoke-Build {
    param([string]$Label)

    Write-Host "[*] $Label ($Configuration)..."
    & $msbuild $solution /t:Rebuild /p:Configuration=$Configuration /p:Deterministic=true /v:m
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }

    if (-not (Test-Path $builtExe)) {
        throw "Build output not found: $builtExe"
    }
}

$msbuild = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" `
    -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe `
    2>$null | Select-Object -First 1

if (-not $msbuild) {
    $msbuild = "msbuild"
}

Invoke-Build -Label "Building PowerUpSQLSharp"

if (Test-Path $builtCoreDll) {
    Write-Host "[!] Expected single-file exe, but found dependency: $builtCoreDll" -ForegroundColor Yellow
    Write-Host "[!] Costura.Fody may not have embedded PowerUpSQLSharp.Core.dll" -ForegroundColor Yellow
}

New-Item -ItemType Directory -Force -Path $dest | Out-Null
Copy-Item $builtExe $artifact -Force

Write-Host "[+] Release artifact: $artifact"

if (-not (Test-Path $verify)) {
    throw "Verification script not found: $verify"
}

& $verify -ExePath $artifact -CheckPeTimestamp

if (-not $SkipDeterministicCheck) {
    $firstHash = (Get-FileHash $artifact -Algorithm SHA256).Hash
    Write-Host "[*] Deterministic build check (B1-084)..."

    Invoke-Build -Label "Rebuilding for hash comparison"

    $secondHash = (Get-FileHash $builtExe -Algorithm SHA256).Hash
    if ($firstHash -ne $secondHash) {
        throw "Deterministic build check failed. SHA256 mismatch: $firstHash vs $secondHash"
    }

    Write-Host "[+] Deterministic build check passed." -ForegroundColor Green
    Copy-Item $builtExe $artifact -Force
}

Write-Host "[+] Release pipeline completed." -ForegroundColor Green
