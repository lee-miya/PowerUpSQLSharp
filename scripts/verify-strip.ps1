param(
    [Parameter(Mandatory = $true)]
    [string]$ExePath,

    [string[]]$MachineNames = @(),

    [switch]$CheckPeTimestamp
)

$ErrorActionPreference = "Stop"

function Get-PeTimestamp {
    param([byte[]]$Bytes)

    if ($Bytes.Length -lt 64) {
        return $null
    }

    if ($Bytes[0] -ne 0x4D -or $Bytes[1] -ne 0x5A) {
        return $null
    }

    $peOffset = [BitConverter]::ToInt32($Bytes, 0x3C)
    if ($peOffset -le 0 -or ($peOffset + 12) -gt $Bytes.Length) {
        return $null
    }

    return [BitConverter]::ToUInt32($Bytes, $peOffset + 8)
}

if (-not (Test-Path $ExePath)) {
    throw "File not found: $ExePath"
}

Write-Host "[*] Verifying strip: $ExePath"

$bytes = [System.IO.File]::ReadAllBytes($ExePath)
$text = [System.Text.Encoding]::ASCII.GetString($bytes)

$patterns = @(
    'C:\\Users\\',
    'C:/Users/',
    '.pdb',
    'DebuggableAttribute'
)

$failed = $false
foreach ($pattern in $patterns) {
    if ($text -match [regex]::Escape($pattern) -or $text -match $pattern) {
        Write-Host "[!] Found suspicious pattern: $pattern" -ForegroundColor Red
        $failed = $true
    }
}

if ($MachineNames.Count -eq 0) {
    if ($env:COMPUTERNAME) { $MachineNames += $env:COMPUTERNAME }
    if ($env:USERNAME) { $MachineNames += $env:USERNAME }
}

foreach ($name in ($MachineNames | Select-Object -Unique)) {
    if ([string]::IsNullOrWhiteSpace($name) -or $name.Length -lt 3) {
        continue
    }

    if ($text -match [regex]::Escape($name)) {
        Write-Host "[!] Found machine/user name in binary: $name" -ForegroundColor Red
        $failed = $true
    }
}

if ($CheckPeTimestamp) {
    $timestamp = Get-PeTimestamp -Bytes $bytes
    if ($null -ne $timestamp -and $timestamp -ne 0) {
        $epoch = [DateTimeOffset]::FromUnixTimeSeconds($timestamp).UtcDateTime
        Write-Host ("[!] PE TimeDateStamp is non-zero: 0x{0:X8} ({1:u})" -f $timestamp, $epoch) -ForegroundColor Red
        $failed = $true
    }
    else {
        Write-Host "[*] PE TimeDateStamp: deterministic (0)"
    }
}

$hash = (Get-FileHash $ExePath -Algorithm SHA256).Hash
Write-Host "[*] SHA256: $hash"

if ($failed) {
    throw "Strip verification failed."
}

Write-Host "[+] Strip verification passed." -ForegroundColor Green
