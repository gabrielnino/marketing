<#
Build-ProdBundle.ps1
Genera un bundle instalable de WhatsAppSender listo para Scheduler
#>

param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [switch]$IncludeAppSettings = $true
)

$ErrorActionPreference = "Stop"

# -------------------------------
# Paths base
# -------------------------------
$root = Split-Path -Parent $MyInvocation.MyCommand.Path

$publishDir = Join-Path $root "_publish_prod"
$bundleRoot = Join-Path $root "_prod_bundle"
$timestamp  = Get-Date -Format "yyyyMMdd_HHmmss"

$bundleDir  = Join-Path $bundleRoot "WhatsAppSender_$timestamp"
$appDir     = Join-Path $bundleDir "app"
$fileDir    = Join-Path $appDir "file"
$logDir     = Join-Path $bundleDir "logs"

$logFile    = Join-Path $logDir "BuildProdBundle.log"

# -------------------------------
# 🔑 CREATE FOLDERS FIRST
# -------------------------------
New-Item $bundleRoot -ItemType Directory -Force | Out-Null
New-Item $bundleDir  -ItemType Directory -Force | Out-Null
New-Item $appDir     -ItemType Directory -Force | Out-Null
New-Item $fileDir    -ItemType Directory -Force | Out-Null
New-Item $logDir     -ItemType Directory -Force | Out-Null

# -------------------------------
# Logging helper (SAFE NOW)
# -------------------------------
function Log($msg) {
    $line = "[{0}] {1}" -f (Get-Date -Format "yyyy-MM-dd HH:mm:ss"), $msg
    Write-Host $line
    $line | Out-File $logFile -Append -Encoding UTF8
}

# -------------------------------
# Start
# -------------------------------
Log "=== Build Production Bundle START ==="
Log "Root: $root"

# -------------------------------
# 1. Cleanup
# -------------------------------
Log "Step 1: Cleanup"
Remove-Item $publishDir -Recurse -Force -ErrorAction SilentlyContinue

# -------------------------------
# 2. dotnet publish
# -------------------------------
Log "Step 2: dotnet publish"

dotnet publish `
    "$root\WhatsAppSender.csproj" `
    -c $Configuration `
    -r $Runtime `
    --self-contained false `
    -p:PublishSingleFile=true `
    -p:PublishTrimmed=false `
    -o $publishDir `
    >> $logFile 2>&1

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed"
}

# -------------------------------
# 3. Copy published output
# -------------------------------
Log "Step 3: Copy published output"
Copy-Item "$publishDir\*" $appDir -Recurse -Force

# -------------------------------
# 4. Config handling
# -------------------------------
Log "Step 4: Config handling"

if ($IncludeAppSettings) {

    $srcAppSettings = Join-Path $root "file\appsettings.json"
    $srcCreds       = Join-Path $root "file\credentials.json"

    if (!(Test-Path $srcAppSettings)) {
        throw "IncludeAppSettings=true but file/appsettings.json not found"
    }

    Copy-Item $srcAppSettings $fileDir -Force
    Log "Copied appsettings.json"

    if (Test-Path $srcCreds) {
        Copy-Item $srcCreds $fileDir -Force
        Log "Copied credentials.json"
    }
}

# -------------------------------
# 5. Selenium Manager (optional)
# -------------------------------
Log "Step 5: Selenium Manager"

$seleniumManager = Join-Path $root "selenium-manager"
if (Test-Path $seleniumManager) {
    Copy-Item $seleniumManager (Join-Path $appDir "selenium-manager") -Recurse -Force
    Log "Selenium manager copied"
} else {
    Log "Selenium manager not found (skipped)"
}

# -------------------------------
# 6. run.cmd (Scheduler entrypoint)
# -------------------------------
Log "Step 6: Creating run.cmd"

@"
@echo off
set BASEDIR=%~dp0
cd /d %BASEDIR%\app
WhatsAppSender.exe
"@ | Out-File (Join-Path $bundleDir "run.cmd") -Encoding ASCII

# -------------------------------
# 7. Validation
# -------------------------------
Log "Step 7: Validation"

$exe = Join-Path $appDir "WhatsAppSender.exe"
if (!(Test-Path $exe)) {
    throw "WhatsAppSender.exe not found in bundle"
}

if ($IncludeAppSettings) {
    if (!(Test-Path (Join-Path $fileDir "appsettings.json"))) {
        throw "appsettings.json missing in bundle/file"
    }
}

# -------------------------------
# Done
# -------------------------------
Log "=== Build Production Bundle COMPLETED ==="
Log "Bundle path: $bundleDir"

Write-Host ""
Write-Host "✅ PRODUCTION BUNDLE READY:"
Write-Host $bundleDir
Write-Host ""
