<#
Build-Installer.ps1

Goal (as requested):
- Delete unnecessary files from the installer output.
- Specifically remove: appsettings.json, credentials.json (and any appsettings.*.json)
- Keep only what is typically required to run YOUR current published output (based on your screenshot):
    - WhatsAppSender.exe (detected automatically)
    - selenium-manager\ (folder)
    - e_sqlite3.dll (native dependency)
    - run.cmd (convenience)

Important:
- This script uses an "allowlist" approach: it deletes EVERYTHING not explicitly allowed.
- If your app later needs more files (e.g., other native dlls, extra folders), add them to -KeepFiles/-KeepFolders.

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

    # Allowlist controls (add more if your app needs them)
    [string[]]$KeepFolders = @("selenium-manager"),
    [string[]]$KeepFiles = @("e_sqlite3.dll"), # exe is auto-kept; run.cmd is auto-kept

    # Files to explicitly delete (even if present)
    [string[]]$DeleteFiles = @("appsettings.json", "credentials.json"),
    [string[]]$DeletePatterns = @("appsettings.*.json", "*.pdb", "*.xml", "*.dbg", "*.dSYM"),

    # Optional: keep deps.json if it appears (framework-dependent sometimes needs it)
    [bool]$KeepDepsJson = $true
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function New-Timestamp { (Get-Date).ToString("yyyyMMdd_HHmmss") }

function Ensure-Dir([string]$Path) {
    if (-not (Test-Path $Path)) { New-Item -ItemType Directory -Path $Path | Out-Null }
}

function Remove-IfExists([string]$Path) {
    if (Test-Path $Path) { Remove-Item -LiteralPath $Path -Recurse -Force }
}

function Step([int]$Index, [int]$Total, [string]$Title, [string]$Detail) {
    $pct = [int](($Index / [double]$Total) * 100)
    Write-Progress -Activity $Title -Status $Detail -PercentComplete $pct
    $ts = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
    Write-Host ("[{0}] Step {1}/{2}: {3} - {4}" -f $ts, $Index, $Total, $Title, $Detail)
}

function Normalize-Name([string]$s) { $s.Trim().ToLowerInvariant() }

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
    $TOTAL_STEPS = 10
    $i = 0

    $i++; Step $i $TOTAL_STEPS "Initialize" "Validating inputs"
    if (-not (Test-Path -LiteralPath $ProjectPath)) { throw "Project file not found: $ProjectPath" }

    Write-Host "ProjectPath    : $ProjectPath"
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
    if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed with exit code $LASTEXITCODE" }

    $i++; Step $i $TOTAL_STEPS "Publish" "Detecting executable"
    $exe = Get-ChildItem -Path $PublishDir -Filter "*.exe" -File | Select-Object -First 1
    if (-not $exe) { throw "No executable found in publish output: $PublishDir" }

    $appName = [IO.Path]::GetFileNameWithoutExtension($exe.Name)
    $installerPath = Join-Path $InstallerRoot ("{0}_{1}" -f $appName, $ts)

    $i++; Step $i $TOTAL_STEPS "Installer" "Creating installer directory"
    Ensure-Dir $installerPath

    $i++; Step $i $TOTAL_STEPS "Installer" "Copying published files"
    Copy-Item -Path (Join-Path $PublishDir "*") -Destination $installerPath -Recurse -Force

    # Create run.cmd now (so it's included in allowlist)
    $i++; Step $i $TOTAL_STEPS "Convenience" "Creating run.cmd"
    $runCmd = Join-Path $installerPath "run.cmd"
    $runContent = "@echo off`r`ncd /d %~dp0`r`n`"%~dp0$($exe.Name)`"`r`n"
    Set-Content -LiteralPath $runCmd -Value $runContent -Encoding ASCII

    $i++; Step $i $TOTAL_STEPS "Cleanup" "Deleting known-unnecessary files (patterns + explicit)"
    # explicit deletes
    foreach ($name in $DeleteFiles) {
        $p = Join-Path $installerPath $name
        if (Test-Path -LiteralPath $p) {
            Write-Host "Deleting explicit: $p"
            Remove-Item -LiteralPath $p -Force
        }
    }

    # pattern deletes
    foreach ($pattern in $DeletePatterns) {
        Get-ChildItem -Path $installerPath -Recurse -File -Filter $pattern -ErrorAction SilentlyContinue |
            ForEach-Object {
                Write-Host "Deleting by pattern ($pattern): $($_.FullName)"
                Remove-Item -LiteralPath $_.FullName -Force
            }
    }

    $i++; Step $i $TOTAL_STEPS "Cleanup" "Allowlist cleanup (delete everything NOT required)"

    # Build allowlists (case-insensitive by normalizing)
    $keepFileNames = New-Object 'System.Collections.Generic.HashSet[string]'
    $keepFolderNames = New-Object 'System.Collections.Generic.HashSet[string]'

    # Always keep the detected exe + run.cmd
    $null = $keepFileNames.Add((Normalize-Name $exe.Name))
    $null = $keepFileNames.Add("run.cmd")

    # User-specified keep files
    foreach ($f in $KeepFiles) { if ($f) { $null = $keepFileNames.Add((Normalize-Name $f)) } }

    # deps.json handling (if present)
    if ($KeepDepsJson) {
        Get-ChildItem -Path $installerPath -File -Filter "*.deps.json" -ErrorAction SilentlyContinue |
            ForEach-Object { $null = $keepFileNames.Add((Normalize-Name $_.Name)) }
    }

    # Keep folders
    foreach ($d in $KeepFolders) { if ($d) { $null = $keepFolderNames.Add((Normalize-Name $d)) } }

    # Delete folders not in allowlist (top-level only)
    Get-ChildItem -Path $installerPath -Directory | ForEach-Object {
        $nameNorm = Normalize-Name $_.Name
        if (-not $keepFolderNames.Contains($nameNorm)) {
            Write-Host "Deleting folder (not required): $($_.FullName)"
            Remove-Item -LiteralPath $_.FullName -Recurse -Force
        }
    }

    # Delete files not in allowlist (top-level only)
    Get-ChildItem -Path $installerPath -File | ForEach-Object {
        $nameNorm = Normalize-Name $_.Name
        if (-not $keepFileNames.Contains($nameNorm)) {
            Write-Host "Deleting file (not required): $($_.FullName)"
            Remove-Item -LiteralPath $_.FullName -Force
        }
    }

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
        "KeptFolders   : $($KeepFolders -join ', ')",
        "KeptFiles     : $($KeepFiles -join ', ')",
        "DeletedFiles  : $($DeleteFiles -join ', ')",
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
