<#
Build-Installer.ps1

Creates a clean installer folder from a dotnet publish output.

Features
- dotnet publish (single-file, framework-dependent)
- Step-by-step console messages
- Progress bar (Write-Progress)
- Explicit cleanup of unnecessary files
- Explicit deletion of highlighted *.pdb files
- Canonical appsettings.json handling (resolved relative to .csproj folder + publish fallback)
- run.cmd generation
- Full execution log (transcript)
- Dedicated error log
- Summary log

Logs:
  .\_installer\_logs\
#>

[CmdletBinding()]
param(
    [string]$ProjectPath = ".\WhatsAppSender.csproj",
    [ValidateSet("Debug","Release")]
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [bool]$SelfContained = $false,
    [string]$PublishDir = ".\_publish_prod",
    [string]$InstallerRoot = ".\_installer",

    # appsettings handling (can be relative or absolute; relative is resolved against .csproj directory)
    [string]$AppSettingsFile = ".\appsettings.json",
    [string]$AppSettingsProductionFile = ".\appsettings.Production.json",
    [bool]$IncludeProductionSettingsIfExists = $true,

    # generic cleanup
    [string[]]$RemoveExtensions = @(".pdb",".xml",".dbg",".dSYM"),
    [string[]]$RemovePatterns = @(
        "appsettings.Development.json",
        "appsettings.*.Development.json",
        "*.deps.json"
    ),
    [bool]$KeepDepsJson = $true
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# -------------------------------
# helpers
# -------------------------------
function New-Timestamp { (Get-Date).ToString("yyyyMMdd_HHmmss") }

function Ensure-Dir([string]$Path) {
    if (-not (Test-Path $Path)) {
        New-Item -ItemType Directory -Path $Path | Out-Null
    }
}

function Remove-IfExists([string]$Path) {
    if (Test-Path $Path) {
        Remove-Item -LiteralPath $Path -Recurse -Force
    }
}

function Step([int]$Index, [int]$Total, [string]$Title, [string]$Detail) {
    $pct = [int](($Index / [double]$Total) * 100)
    Write-Progress -Activity $Title -Status $Detail -PercentComplete $pct
    $ts = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
    Write-Host ("[{0}] Step {1}/{2}: {3} - {4}" -f $ts, $Index, $Total, $Title, $Detail)
}

function Resolve-SettingsPath([string]$path, [string]$baseDir) {
    if ([string]::IsNullOrWhiteSpace($path)) { return $null }

    # Absolute path
    if ([IO.Path]::IsPathRooted($path)) {
        return (Test-Path -LiteralPath $path) ? $path : $null
    }

    # Relative path: support ".\file.json" and "sub\file.json"
    $rel = ($path -replace '^[.\\\/]+','')   # remove leading .\ or ./ or slashes
    $candidate = Join-Path $baseDir $rel
    if (Test-Path -LiteralPath $candidate) { return $candidate }

    # fallback: try current directory as-is
    if (Test-Path -LiteralPath $path) { return $path }

    return $null
}

# -------------------------------
# init + logging
# -------------------------------
$ts = New-Timestamp
Ensure-Dir $InstallerRoot
$logDir = Join-Path $InstallerRoot "_logs"
Ensure-Dir $logDir

$transcriptPath = Join-Path $logDir ("BuildInstaller_{0}.transcript.log" -f $ts)
$errorPath      = Join-Path $logDir ("BuildInstaller_{0}.errors.log" -f $ts)
$summaryPath    = Join-Path $logDir ("BuildInstaller_{0}.summary.txt" -f $ts)

Start-Transcript -Path $transcriptPath -Append | Out-Null

$installerPath = $null
$success = $false

try {
    $TOTAL_STEPS = 11
    $i = 0

    $i++; Step $i $TOTAL_STEPS "Initialize" "Validating inputs"
    if (-not (Test-Path -LiteralPath $ProjectPath)) {
        throw "Project file not found: $ProjectPath"
    }

    $projectDir = Split-Path -Parent (Resolve-Path -LiteralPath $ProjectPath)

    Write-Host "ProjectDir     : $projectDir"
    Write-Host "PublishDir     : $PublishDir"
    Write-Host "InstallerRoot  : $InstallerRoot"
    Write-Host "TranscriptLog  : $transcriptPath"
    Write-Host "ErrorLog       : $errorPath"
    Write-Host ""

    $i++; Step $i $TOTAL_STEPS "Publish" "Cleaning publish directory"
    Remove-IfExists $PublishDir
    Ensure-Dir $PublishDir

    $i++; Step $i $TOTAL_STEPS "Publish" "Running dotnet publish"
    $selfArg = if ($SelfContained) { "true" } else { "false" }

    $publishArgs = @(
        "publish", $ProjectPath,
        "-c", $Configuration,
        "-r", $Runtime,
        "--self-contained", $selfArg,
        "-p:PublishSingleFile=true",
        "-p:PublishTrimmed=false",
        "-o", $PublishDir
    )

    Write-Host "dotnet $($publishArgs -join ' ')"
    & dotnet @publishArgs
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed with exit code $LASTEXITCODE"
    }

    $i++; Step $i $TOTAL_STEPS "Publish" "Detecting executable"
    $exe = Get-ChildItem -Path $PublishDir -Filter "*.exe" -File | Select-Object -First 1
    if (-not $exe) {
        throw "No executable found in publish output: $PublishDir"
    }

    $appName = [IO.Path]::GetFileNameWithoutExtension($exe.Name)
    $installerPath = Join-Path $InstallerRoot ("{0}_{1}" -f $appName, $ts)

    $i++; Step $i $TOTAL_STEPS "Installer" "Creating installer directory"
    Ensure-Dir $installerPath

    $i++; Step $i $TOTAL_STEPS "Installer" "Copying published files"
    Copy-Item -Path (Join-Path $PublishDir "*") -Destination $installerPath -Recurse -Force

    $i++; Step $i $TOTAL_STEPS "Cleanup" "Removing files by extension / pattern"
    if ($KeepDepsJson -and ($RemovePatterns -contains "*.deps.json")) {
        $RemovePatterns = $RemovePatterns | Where-Object { $_ -ne "*.deps.json" }
    }

    foreach ($ext in $RemoveExtensions) {
        Get-ChildItem -Path $installerPath -Recurse -File |
            Where-Object { $_.Extension -ieq $ext } |
            ForEach-Object {
                Write-Host "Deleting $($_.FullName)"
                Remove-Item -LiteralPath $_.FullName -Force
            }
    }

    foreach ($pattern in $RemovePatterns) {
        Get-ChildItem -Path $installerPath -Recurse -File -Filter $pattern -ErrorAction SilentlyContinue |
            ForEach-Object {
                Write-Host "Deleting $($_.FullName)"
                Remove-Item -LiteralPath $_.FullName -Force
            }
    }

    $i++; Step $i $TOTAL_STEPS "Cleanup" "Deleting highlighted PDB files explicitly"
    $HighlightedFilesToDelete = @(
        "WhatsAppSender.pdb",
        "Bootstrapper.pdb",
        "Commands.pdb",
        "Infrastructure.pdb",
        "Application.pdb",
        "Persistence.pdb",
        "Services.pdb",
        "Configuration.pdb",
        "Domain.pdb"
    )

    foreach ($name in $HighlightedFilesToDelete) {
        $candidate = Join-Path $installerPath $name
        if (Test-Path -LiteralPath $candidate) {
            Write-Host "Deleting highlighted file: $candidate"
            Remove-Item -LiteralPath $candidate -Force
        }
    }

    $i++; Step $i $TOTAL_STEPS "Config" "Copying appsettings.json"

    # Resolve appsettings relative to project directory (robust to current working dir)
    $resolvedAppSettings  = Resolve-SettingsPath $AppSettingsFile $projectDir
    $resolvedProdSettings = Resolve-SettingsPath $AppSettingsProductionFile $projectDir

    $destAppSettings = Join-Path $installerPath "appsettings.json"

    # If source isn't found, fallback to the already-copied published appsettings.json
    if (-not $resolvedAppSettings) {
        Write-Host "WARNING: Source appsettings.json not found at '$AppSettingsFile' (resolved against: $projectDir)."
        Write-Host "WARNING: Using published appsettings.json if present in installer folder."
        if (-not (Test-Path -LiteralPath $destAppSettings)) {
            throw "No appsettings.json found in installer folder either. Provide -AppSettingsFile with the correct path."
        }
    }
    else {
        if (Test-Path -LiteralPath $destAppSettings) {
            Remove-Item -LiteralPath $destAppSettings -Force
        }

        Write-Host "Copying appsettings from: $resolvedAppSettings"
        Copy-Item -LiteralPath $resolvedAppSettings -Destination $destAppSettings -Force
    }

    if ($IncludeProductionSettingsIfExists -and $resolvedProdSettings) {
        Write-Host "Copying appsettings.Production.json from: $resolvedProdSettings"
        Copy-Item -LiteralPath $resolvedProdSettings `
            -Destination (Join-Path $installerPath "appsettings.Production.json") -Force
    }

    $i++; Step $i $TOTAL_STEPS "Convenience" "Creating run.cmd"
    $runCmd = Join-Path $installerPath "run.cmd"
    $runContent = "@echo off`r`ncd /d %~dp0`r`n`"%~dp0$($exe.Name)`"`r`n"
    Set-Content -LiteralPath $runCmd -Value $runContent -Encoding ASCII

    $i++; Step $i $TOTAL_STEPS "Summary" "Collecting stats"
    $files = Get-ChildItem -Path $installerPath -Recurse -File
    $sizeMB = [math]::Round((($files | Measure-Object Length -Sum).Sum) / 1MB, 2)

    $summary = @(
        "Build Installer Summary",
        "Timestamp     : $ts",
        "Project       : $ProjectPath",
        "Executable    : $($exe.Name)",
        "InstallerPath : $installerPath",
        "Files         : $($files.Count)",
        "Size (MB)     : $sizeMB",
        "TranscriptLog : $transcriptPath",
        "ErrorLog      : $errorPath",
        "SummaryLog    : $summaryPath"
    )

    Set-Content -LiteralPath $summaryPath -Value ($summary -join "`r`n") -Encoding UTF8

    Write-Progress -Activity "Completed" -Status "Done" -PercentComplete 100
    $success = $true

    Write-Host ""
    Write-Host ($summary -join "`r`n")
}
catch {
    $err = $_
    $tsErr = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")

    $errorText = @(
        "[$tsErr] ERROR",
        "Message : $($err.Exception.Message)",
        "Type    : $($err.Exception.GetType().FullName)",
        "Script  : $($err.InvocationInfo.ScriptName)",
        "Line    : $($err.InvocationInfo.ScriptLineNumber)",
        "Command : $($err.InvocationInfo.Line)",
        "",
        "Stack:",
        $err.ScriptStackTrace,
        ""
    ) -join "`r`n"

    Add-Content -LiteralPath $errorPath -Value $errorText -Encoding UTF8

    Write-Progress -Activity "Failed" -Status $err.Exception.Message -PercentComplete 100
    Write-Host ""
    Write-Host "FAILED: $($err.Exception.Message)"
    Write-Host "See error log: $errorPath"
    throw
}
finally {
    Stop-Transcript | Out-Null
    if ($success) {
        Write-Host "SUCCESS: Installer created at $installerPath"
    } else {
        Write-Host "FAILED: See logs in $logDir"
    }
}
