#Requires -RunAsAdministrator

$ServiceName = "DHCPRelayAPI"
$ServiceDisplayName = "DHCP Relay API"
$ServiceDescription = "Eksponerer DHCP-leases og scopes som JSON via HTTP til AD-webappen."
$InstallPath = "C:\Services\DHCPRelay"
$ExeName = "DHCP.Relay.exe"
$Port = 8888

Write-Host "=== DHCP Relay API - Installation ===" -ForegroundColor Cyan

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$exeSource = Join-Path $scriptDir $ExeName

if (-not (Test-Path $exeSource)) {
    Write-Error "Kunne ikke finde $ExeName i samme mappe som dette script."
    exit 1
}

if (Get-Service -Name $ServiceName -ErrorAction SilentlyContinue) {
    Write-Host "Stopper eksisterende service..." -ForegroundColor Yellow
    Stop-Service -Name $ServiceName -Force
    sc.exe delete $ServiceName | Out-Null
    Start-Sleep -Seconds 2
}

if (-not (Test-Path $InstallPath)) {
    New-Item -ItemType Directory -Path $InstallPath | Out-Null
}

Write-Host "Kopierer filer til $InstallPath..." -ForegroundColor Yellow
Copy-Item -Path "$scriptDir\*" -Destination $InstallPath -Recurse -Force

Write-Host "Opretter Windows Service..." -ForegroundColor Yellow
$exePath = Join-Path $InstallPath $ExeName
New-Service -Name $ServiceName `
            -DisplayName $ServiceDisplayName `
            -Description $ServiceDescription `
            -BinaryPathName $exePath `
            -StartupType Automatic | Out-Null

Write-Host "Aabner firewall port $Port..." -ForegroundColor Yellow
New-NetFirewallRule -DisplayName "DHCP Relay API ($Port)" `
                    -Direction Inbound `
                    -Protocol TCP `
                    -LocalPort $Port `
                    -Action Allow `
                    -ErrorAction SilentlyContinue | Out-Null

Write-Host "Starter service..." -ForegroundColor Yellow
Start-Service -Name $ServiceName
Start-Sleep -Seconds 3

$status = (Get-Service -Name $ServiceName).Status
if ($status -eq "Running") {
    Write-Host "Service korer paa port $Port" -ForegroundColor Green
    Write-Host "Test: http://localhost:$Port/health" -ForegroundColor Cyan
} else {
    Write-Warning "Service startede ikke korrekt. Status: $status"
}
