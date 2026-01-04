<#
Build-ProdBundle.ps1
Genera un bundle instalable de WhatsAppSender listo para Scheduler
- Publica (dotnet publish)
- Copia artefactos al bundle
- Incluye appsettings.json desde \file\appsettings.json hacia app\file\
- Limpia artefactos no deseados del app folder (PDB, selenium-manager, appsettings/credentials en raíz)
#>

param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [switch]$IncludeAppSettings = $true,
    [switch]$CleanPublishedArtifacts = $true
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
# Logging helpers
# -------------------------------
function Ensure-Dir([string]$Path) {
    if (-not (Test-Path -LiteralPath $Path)) {
        New-Item -ItemType Directory -Path $Path -Force | Out-Null
    }
}

function Log([string]$Message, [string]$Level = "INF") {
    $ts = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $line = "[$ts] [$Level] $Message"
    Write-Host $line
    $line | Out-File $logFile -Append -Encoding UTF8
}

# -------------------------------
# Safety helpers for deletions
# -------------------------------
function Assert-InsidePath {
    param(
        [Parameter(Mandatory=$true)][string]$BasePath,
        [Parameter(Mandatory=$true)][string]$TargetPath
    )

    $baseResolved = (Resolve-Path -LiteralPath $BasePath).Path.TrimEnd('\') + '\'
    $targetResolved = (Resolve-Path -LiteralPath $TargetPath).Path

    if (-not $targetResolved.StartsWith($baseResolved, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Safety check failed: '$targetResolved' is not inside '$baseResolved'"
    }
}

function Remove-IfExists {
    param([Parameter(Mandatory=$true)][string]$PathToRemove)

    if (Test-Path -LiteralPath $PathToRemove) {
        Assert-InsidePath -BasePath $appDir -TargetPath $PathToRemove
        Remove-Item -LiteralPath $PathToRemove -Recurse -Force -ErrorAction Stop
        Log "Deleted: $PathToRemove"
    } else {
        Log "Skip delete (not found): $PathToRemove"
    }
}

# -------------------------------
# Start
# -------------------------------
Ensure-Dir $bundleRoot
Ensure-Dir $logDir

Log "=== Build Production Bundle STARTED ==="
Log "Root: $root"
Log "Configuration: $Configuration"
Log "Runtime: $Runtime"
Log "IncludeAppSettings: $IncludeAppSettings"
Log "CleanPublishedArtifacts: $CleanPublishedArtifacts"
Log "PublishDir: $publishDir"
Log "BundleDir: $bundleDir"

# -------------------------------
# Step 1: Validate inputs
# -------------------------------
Log "Step 1/5: Validating inputs"

$csproj = Get-ChildItem -Path $root -Filter *.csproj -File -ErrorAction SilentlyContinue | Select-Object -First 1
if (-not $csproj) {
    throw "No .csproj found in script directory: $root"
}
Log "Project: $($csproj.FullName)"

# FIX: appsettings.json is located in $root\file\appsettings.json (not $root\appsettings.json)
$appSettingsSrc = Join-Path (Join-Path $root "file") "appsettings.json"

if ($IncludeAppSettings) {
    if (-not (Test-Path -LiteralPath $appSettingsSrc)) {
        throw "IncludeAppSettings is enabled, but appsettings.json not found at: $appSettingsSrc"
    }
    Log "appsettings.json found: $appSettingsSrc"
}

# -------------------------------
# Step 2: dotnet publish
# -------------------------------
Log "Step 2/5: Publishing project"

if (Test-Path -LiteralPath $publishDir) {
    Log "Cleaning previous publish folder: $publishDir"
    Remove-Item -LiteralPath $publishDir -Recurse -Force
}
Ensure-Dir $publishDir

$publishArgs = @(
    "publish", $csproj.FullName,
    "-c", $Configuration,
    "-r", $Runtime,
    "--self-contained", "false",
    "-o", $publishDir
)

Log "Running: dotnet $($publishArgs -join ' ')"
& dotnet @publishArgs 2>&1 | ForEach-Object { Log $_ "DOTNET" }

# Basic sanity check
$exe = Get-ChildItem -Path $publishDir -Filter *.exe -File -ErrorAction SilentlyContinue | Select-Object -First 1
if (-not $exe) {
    throw "Publish did not produce an .exe in: $publishDir"
}
Log "Publish OK. EXE: $($exe.Name)"

# -------------------------------
# Step 3: Create bundle structure
# -------------------------------
Log "Step 3/5: Creating bundle structure"

Ensure-Dir $bundleDir
Ensure-Dir $appDir
Ensure-Dir $fileDir
Ensure-Dir $logDir

# -------------------------------
# Step 4: Copy published app into bundle
# -------------------------------
Log "Step 4/5: Copying published output to bundle/app"

Copy-Item -Path (Join-Path $publishDir "*") -Destination $appDir -Recurse -Force

# Include appsettings.json into app\file\ (runtime config location)
if ($IncludeAppSettings) {
    $appSettingsDst = Join-Path $fileDir "appsettings.json"
    Copy-Item -LiteralPath $appSettingsSrc -Destination $appSettingsDst -Force
    Log "Copied appsettings.json to: $appSettingsDst"

    if (-not (Test-Path -LiteralPath $appSettingsDst)) {
        throw "appsettings.json missing in bundle/file"
    }
}

# -------------------------------
# Step 5: Cleanup published artifacts (matches your screenshot)
# -------------------------------
if ($CleanPublishedArtifacts) {
    Log "Step 5/5: Cleanup published artifacts (PDBs, selenium-manager, root appsettings/credentials)"

    # Delete all .pdb files anywhere under app
    $pdbs = Get-ChildItem -Path $appDir -Filter *.pdb -Recurse -File -ErrorAction SilentlyContinue
    foreach ($pdb in $pdbs) {
        Remove-IfExists -PathToRemove $pdb.FullName
    }

    # Delete selenium-manager folder (if present)
    Remove-IfExists -PathToRemove (Join-Path $appDir "selenium-manager")

    # Delete root-level appsettings.json and credentials.json (if present)
    # NOTE: This does NOT delete app\file\appsettings.json
    Remove-IfExists -PathToRemove (Join-Path $appDir "appsettings.json")
    Remove-IfExists -PathToRemove (Join-Path $appDir "credentials.json")
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
