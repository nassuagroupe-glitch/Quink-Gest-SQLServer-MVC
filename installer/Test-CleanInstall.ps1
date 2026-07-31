<#
Teste l'installateur QuinkGest sur une machine/VM propre :
installation silencieuse, verification des fichiers/raccourcis,
lancement de l'application, puis desinstallation et verification du nettoyage.

Usage : copier QuinkGest-Setup.msi et ce script sur la VM (meme dossier),
puis executer dans PowerShell : .\Test-CleanInstall.ps1
Aucun droit admin requis (installation per-user).
#>

param(
    [string]$MsiPath = (Join-Path $PSScriptRoot "QuinkGest-Setup.msi")
)

$ErrorActionPreference = "Stop"
$installDir = Join-Path $env:LocalAppData "QuinkGest"
$desktopShortcut = Join-Path ([Environment]::GetFolderPath("Desktop")) "QuinkGest.lnk"
$startMenuShortcut = Join-Path ([Environment]::GetFolderPath("Programs")) "QuinkGest.lnk"
$logDir = Join-Path $env:TEMP "QuinkGestInstallTest"
New-Item -ItemType Directory -Force -Path $logDir | Out-Null

function Write-Step($msg) { Write-Host "`n=== $msg ===" -ForegroundColor Cyan }

Write-Step "Informations systeme"
$os = Get-CimInstance Win32_OperatingSystem
Write-Host "OS: $($os.Caption) ($($os.OSArchitecture)) - Build $($os.BuildNumber)"

Write-Step "Verification MSI"
if (-not (Test-Path $MsiPath)) { throw "MSI introuvable: $MsiPath" }
Write-Host "MSI: $MsiPath"

Write-Step "Installation silencieuse"
$installLog = Join-Path $logDir "install.log"
$p = Start-Process msiexec.exe -ArgumentList "/i `"$MsiPath`" /qn /l*v `"$installLog`"" -Wait -PassThru
if ($p.ExitCode -ne 0) {
    Write-Host "ECHEC installation, code $($p.ExitCode). Voir $installLog" -ForegroundColor Red
    Get-Content $installLog -Tail 40
    exit 1
}
Write-Host "Installation OK (code 0)"

Write-Step "Verification des fichiers et raccourcis"
$checks = @(
    @{ Name = "Dossier install"; Path = $installDir },
    @{ Name = "Executable"; Path = (Join-Path $installDir "Quink-Gest.exe") },
    @{ Name = "Raccourci bureau"; Path = $desktopShortcut },
    @{ Name = "Raccourci menu demarrer"; Path = $startMenuShortcut }
)
$allOk = $true
foreach ($c in $checks) {
    $exists = Test-Path $c.Path
    if ($exists) { $status = "OK" } else { $status = "MANQUANT"; $allOk = $false }
    Write-Host "$($c.Name): $status ($($c.Path))"
}
if (-not $allOk) { Write-Host "Des fichiers/raccourcis manquent." -ForegroundColor Red }

Write-Step "Lancement de l'application"
$exe = Join-Path $installDir "Quink-Gest.exe"
$before = Get-Date
Start-Process $exe | Out-Null
Start-Sleep -Seconds 8
$running = Get-Process -Name "Quink-Gest" -ErrorAction SilentlyContinue
if ($running) {
    Write-Host "Application lancee (PID $($running.Id)), fenetre: '$($running.MainWindowTitle)', repond: $($running.Responding)" -ForegroundColor Green
} else {
    Write-Host "L'application ne tourne plus apres 8s (crash probable)." -ForegroundColor Red
}

Write-Step "Erreurs applicatives recentes (journal Windows)"
Get-WinEvent -FilterHashtable @{LogName='Application'; StartTime=$before} -ErrorAction SilentlyContinue |
    Where-Object { $_.ProviderName -match "\.NET|Application Error|Quink" } |
    Select-Object -First 10 TimeCreated, ProviderName, Id, LevelDisplayName, Message |
    Format-List

if ($running) {
    Stop-Process -Id $running.Id -Force
}

Write-Step "Desinstallation silencieuse"
$uninstallLog = Join-Path $logDir "uninstall.log"
$p2 = Start-Process msiexec.exe -ArgumentList "/x `"$MsiPath`" /qn /l*v `"$uninstallLog`"" -Wait -PassThru
if ($p2.ExitCode -ne 0) {
    Write-Host "ECHEC desinstallation, code $($p2.ExitCode). Voir $uninstallLog" -ForegroundColor Red
} else {
    Write-Host "Desinstallation OK"
}

Write-Step "Verification du nettoyage"
foreach ($c in $checks) {
    $exists = Test-Path $c.Path
    if (-not $exists) { $status = "OK (supprime)" } else { $status = "RESTE PRESENT" }
    Write-Host "$($c.Name): $status"
}

Write-Step "Resume"
Write-Host "Logs detailles dans: $logDir"
