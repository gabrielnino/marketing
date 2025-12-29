<#
Install-WhatsAppSender.ps1

End-to-end installer for production "raw" deployment folder.

What it does:
1) Checks/installs .NET 8 SDK x64 (from builds.dotnet.microsoft.com) if missing
2) Reads appsettings.json to get folder paths and image target
3) Creates required directories
4) Copies goku.png to configured ImageDirectory/ImageFileName
5) Installs AutoIt silently (autoit-v3-setup.exe) if missing
6) Writes logs:
   - logs\install.log        (full transcript)
   - logs\step-by-step.log   (step trace)
   - logs\errors.jsonl       (structured errors)
7) Basic rollback (copied files + empty created dirs)

Run as Administrator.
Tested for Windows PowerShell 5.1 compatibility (ASCII-only, no smart quotes).
#>

[CmdletBinding()]
param(
  [string]$PackageRoot = (Get-Location).Path,
  [string]$AppSettingsPath = (Join-Path (Get-Location).Path "appsettings.json"),
  [string]$LogsDir = (Join-Path (Get-Location).Path "logs"),
  [int]$RetryCount = 3,
  [int]$RetryDelaySeconds = 2
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$ProgressPreference = "Continue"

# Ensure TLS 1.2 for downloads on WinPS 5.1
try {
  [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
} catch { }

# ---------------------------
# Helpers: filesystem
# ---------------------------
function Ensure-Dir([string]$Path) {
  if ([string]::IsNullOrWhiteSpace($Path)) { throw "Ensure-Dir received empty path." }
  if (-not (Test-Path -LiteralPath $Path)) {
    New-Item -ItemType Directory -Path $Path -Force | Out-Null
  }
}

# ---------------------------
# Helpers: logging
# ---------------------------
Ensure-Dir $LogsDir
$InstallLog = Join-Path $LogsDir "install.log"
$StepLog    = Join-Path $LogsDir "step-by-step.log"
$ErrorsLog  = Join-Path $LogsDir "errors.jsonl"

function Write-Step([string]$Message) {
  $ts = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
  $line = "[$ts] $Message"
  Write-Host $line
  Add-Content -LiteralPath $StepLog -Value $line
}

function Write-ErrorRecord([string]$Step, [System.Exception]$Ex) {
  $obj = [pscustomobject]@{
    timestamp = (Get-Date).ToString("o")
    step      = $Step
    type      = $Ex.GetType().FullName
    message   = $Ex.Message
    stack     = $Ex.StackTrace
  }
  ($obj | ConvertTo-Json -Compress) | Add-Content -LiteralPath $ErrorsLog
}

function Invoke-Retry([string]$Name, [scriptblock]$Action) {
  for ($i=1; $i -le $RetryCount; $i++) {
    try { return & $Action }
    catch {
      if ($i -ge $RetryCount) { throw }
      Write-Step "WARN: '$Name' failed (attempt $i/$RetryCount). Retrying in $RetryDelaySeconds s. Error: $($_.Exception.Message)"
      Start-Sleep -Seconds $RetryDelaySeconds
    }
  }
}

function Set-Progress([int]$Percent, [string]$Activity, [string]$Status) {
  Write-Progress -Activity $Activity -Status $Status -PercentComplete $Percent
}

# ---------------------------
# Rollback tracking
# ---------------------------
$CreatedDirs = New-Object System.Collections.Generic.List[string]
$CopiedFiles = New-Object System.Collections.Generic.List[string]
$AutoItInstalledByScript = $false
$DotNetInstalledByScript = $false

function Track-DirIfCreated([string]$Path) {
  if (-not (Test-Path -LiteralPath $Path)) {
    New-Item -ItemType Directory -Path $Path -Force | Out-Null
    $CreatedDirs.Add($Path) | Out-Null
  }
}

function Rollback-Basic {
  Write-Step "Rollback: starting basic rollback..."

  foreach ($f in $CopiedFiles) {
    try {
      if (Test-Path -LiteralPath $f) {
        Remove-Item -LiteralPath $f -Force
        Write-Step "Rollback: removed copied file: $f"
      }
    } catch {
      Write-Step "Rollback WARN: could not remove file $f. $($_.Exception.Message)"
    }
  }

  foreach ($d in ($CreatedDirs | Sort-Object -Descending)) {
    try {
      if (Test-Path -LiteralPath $d) {
        $items = Get-ChildItem -LiteralPath $d -Force -ErrorAction SilentlyContinue
        if (-not $items -or $items.Count -eq 0) {
          Remove-Item -LiteralPath $d -Force
          Write-Step "Rollback: removed empty created dir: $d"
        }
      }
    } catch {
      Write-Step "Rollback WARN: could not remove dir $d. $($_.Exception.Message)"
    }
  }

  if ($AutoItInstalledByScript) {
    Write-Step "Rollback NOTE: AutoIt was installed by this script. Uninstall is not performed automatically."
  }
  if ($DotNetInstalledByScript) {
    Write-Step "Rollback NOTE: .NET SDK was installed by this script. Uninstall is not performed automatically."
  }
}

# ---------------------------
# Preflight
# ---------------------------
try {
  Set-Progress 2 "WhatsAppSender Install" "Preflight checks..."
  Write-Step "Install start. PackageRoot=$PackageRoot"
  Write-Step "LogsDir=$LogsDir"

  $isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).
    IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
  if (-not $isAdmin) { throw "This script must run as Administrator." }

  if (-not (Test-Path -LiteralPath $AppSettingsPath)) { throw "Missing appsettings.json: $AppSettingsPath" }

  $autoItSetup = Join-Path $PackageRoot "autoit-v3-setup.exe"
  if (-not (Test-Path -LiteralPath $autoItSetup)) { throw "Missing autoit-v3-setup.exe: $autoItSetup" }

  $gokuSource = Join-Path $PackageRoot "goku.png"
  if (-not (Test-Path -LiteralPath $gokuSource)) { throw "Missing goku.png: $gokuSource" }

  Write-Step "Preflight OK."
}
catch {
  Write-ErrorRecord "Preflight" $_.Exception
  throw
}

Start-Transcript -LiteralPath $InstallLog -Append | Out-Null

try {
  # ------------------------------------------------------------
  # Step 0: Check & Install .NET 8 (SDK) x64 if missing
  # ------------------------------------------------------------
  Set-Progress 5 "WhatsAppSender Install" "Checking .NET 8..."
  Write-Step "Step 0: checking .NET 8 installation..."

  $dotnetExe = "C:\Program Files\dotnet\dotnet.exe"

  function Test-DotNet8Installed {
    if (Test-Path -LiteralPath $dotnetExe) {
      try {
        $r = & $dotnetExe --list-runtimes 2>$null
        if ($r -match "Microsoft\.NETCore\.App\s+8\.") { return $true }
      } catch { }
    }

    # Fallback: PATH lookup
    try {
      $cmd = Get-Command dotnet -ErrorAction SilentlyContinue
      if ($null -ne $cmd) {
        $r2 = & $cmd.Source --list-runtimes 2>$null
        if ($r2 -match "Microsoft\.NETCore\.App\s+8\.") { return $true }
      }
    } catch { }

    return $false
  }

  if (Test-DotNet8Installed) {
    Write-Step ".NET 8 already installed."
  } else {
    Write-Step ".NET 8 not detected. Installing .NET 8 SDK x64 silently..."

    $dotnetUrl  = "https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.416/dotnet-sdk-8.0.416-win-x64.exe"
    $dotnetPath = Join-Path $PackageRoot "dotnet-sdk-8.0.416-win-x64.exe"

    Invoke-Retry "Download .NET 8 SDK" {
      Write-Step "Downloading: $dotnetUrl"
      Invoke-WebRequest -Uri $dotnetUrl -OutFile $dotnetPath
    } | Out-Null

    if (-not (Test-Path -LiteralPath $dotnetPath)) {
      throw "Download failed: .NET installer not found at $dotnetPath"
    }

    Invoke-Retry "Install .NET 8 SDK" {
      Write-Step "Installing .NET 8 SDK (silent)..."
      $p = Start-Process -FilePath $dotnetPath -ArgumentList @("/install","/quiet","/norestart") -Wait -PassThru -WindowStyle Hidden
      if ($p.ExitCode -ne 0) { throw "dotnet installer ExitCode=$($p.ExitCode)" }
    } | Out-Null

    # Ensure PATH for current session
    if (Test-Path -LiteralPath $dotnetExe) {
      $env:PATH = "C:\Program Files\dotnet;$env:PATH"
    }

    if (-not (Test-DotNet8Installed)) {
      throw ".NET install completed but .NET 8 runtime still not detected."
    }

    $DotNetInstalledByScript = $true
    Write-Step ".NET 8 installed successfully."
  }

  # ------------------------------------------------------------
  # Read config (appsettings.json)
  # ------------------------------------------------------------
  Set-Progress 10 "WhatsAppSender Install" "Reading appsettings.json..."
  Write-Step "Reading config: $AppSettingsPath"

  $raw = Get-Content -LiteralPath $AppSettingsPath -Raw -Encoding UTF8
  $config = $raw | ConvertFrom-Json

  $outFolder      = $config.Paths.OutFolder
  $downloadFolder = $config.Paths.DownloadFolder

  $conn = $config.ConnectionStrings.DefaultConnection
  $dbPath = $null
  if ($conn -match "Data Source\s*=\s*([^;]+)") { $dbPath = $Matches[1].Trim() }

  $imageDir = $config.WhatsApp.Message.ImageDirectory
  $imageFileName = $config.WhatsApp.Message.ImageFileName

  if ([string]::IsNullOrWhiteSpace($outFolder))      { throw "Paths:OutFolder is empty." }
  if ([string]::IsNullOrWhiteSpace($downloadFolder)) { throw "Paths:DownloadFolder is empty." }
  if ([string]::IsNullOrWhiteSpace($dbPath))         { throw "Could not parse DB path from DefaultConnection (Data Source=...;)." }
  if ([string]::IsNullOrWhiteSpace($imageDir))       { throw "WhatsApp:Message:ImageDirectory is empty." }
  if ([string]::IsNullOrWhiteSpace($imageFileName))  { throw "WhatsApp:Message:ImageFileName is empty." }

  Write-Step "Config OK: OutFolder=$outFolder; DownloadFolder=$downloadFolder; DbPath=$dbPath; ImageDir=$imageDir; ImageFileName=$imageFileName"

  # ------------------------------------------------------------
  # Step A: Create folder structure
  # ------------------------------------------------------------
  Set-Progress 25 "WhatsAppSender Install" "Creating folder structure..."
  Write-Step "Step A: creating/validating folders..."

  Invoke-Retry "Create OutFolder"      { Track-DirIfCreated $outFolder } | Out-Null
  Invoke-Retry "Create DownloadFolder" { Track-DirIfCreated $downloadFolder } | Out-Null

  $dbDir = Split-Path -Parent $dbPath
  Invoke-Retry "Create DbFolder"       { Track-DirIfCreated $dbDir } | Out-Null

  Invoke-Retry "Create ImageDirectory" { Track-DirIfCreated $imageDir } | Out-Null

  Write-Step "Step A OK."

  # ------------------------------------------------------------
  # Step B: Copy image
  # ------------------------------------------------------------
  Set-Progress 45 "WhatsAppSender Install" "Copying goku.png..."
  Write-Step "Step B: copying image..."

  $gokuDest = Join-Path $imageDir $imageFileName
  Invoke-Retry "Copy goku.png" { Copy-Item -LiteralPath $gokuSource -Destination $gokuDest -Force } | Out-Null
  $CopiedFiles.Add($gokuDest) | Out-Null

  if (-not (Test-Path -LiteralPath $gokuDest)) { throw "Validation failed: image not found at destination: $gokuDest" }
  Write-Step "Step B OK: image copied to $gokuDest"

  # ------------------------------------------------------------
  # Step C: Install AutoIt silently (if not installed)
  # ------------------------------------------------------------
  Set-Progress 65 "WhatsAppSender Install" "Checking/installing AutoIt..."
  Write-Step "Step C: AutoIt check/install..."

  function Test-AutoItInstalled {
    $regPaths = @(
      "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\*",
      "HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*"
    )

    foreach ($p in $regPaths) {
      $apps = Get-ItemProperty -Path $p -ErrorAction SilentlyContinue
      foreach ($a in $apps) {
        $dnProp = $a.PSObject.Properties["DisplayName"]
        if ($null -ne $dnProp) {
          $dn = [string]$dnProp.Value
          if ($dn -match "AutoIt") { return $true }
        }
      }
    }

    $candidates = @(
      "$env:ProgramFiles\AutoIt3\AutoIt3.exe",
      "${env:ProgramFiles(x86)}\AutoIt3\AutoIt3.exe"
    )
    return ($candidates | Where-Object { Test-Path -LiteralPath $_ } | Measure-Object).Count -gt 0
  }

  if (Test-AutoItInstalled) {
    Write-Step "AutoIt already installed. Skipping."
  } else {
    Write-Step "AutoIt not detected. Installing silently (argument fallback)..."

    $installArgsCandidates = @(
      @("/S"),
      @("/silent"),
      @("/verysilent"),
      @("/SILENT"),
      @("/VERYSILENT")
    )

    $installed = $false
    foreach ($candidate in $installArgsCandidates) {
      try {
        Write-Step "Trying AutoIt install args: $($candidate -join ' ')"
        $p = Start-Process -FilePath $autoItSetup -ArgumentList $candidate -Wait -PassThru -WindowStyle Hidden
        if ($p.ExitCode -ne 0) { throw "ExitCode=$($p.ExitCode)" }

        Start-Sleep -Seconds 2
        if (Test-AutoItInstalled) { $installed = $true; break }
      }
      catch {
        Write-Step "WARN: install attempt with '$($candidate -join ' ')' failed: $($_.Exception.Message)"
      }
    }

    if (-not $installed) {
      throw "AutoIt silent install failed with all tested switches."
    }

    $AutoItInstalledByScript = $true
    Write-Step "AutoIt installed successfully."
  }

  # ------------------------------------------------------------
  # Step D: Final validation
  # ------------------------------------------------------------
  Set-Progress 90 "WhatsAppSender Install" "Final validation..."
  Write-Step "Step D: final validation..."

  foreach ($p in @($outFolder, $downloadFolder, $dbDir, $imageDir)) {
    if (-not (Test-Path -LiteralPath $p)) { throw "Validation failed: required folder missing: $p" }
  }
  if (-not (Test-Path -LiteralPath $gokuDest)) { throw "Validation failed: image missing: $gokuDest" }

  Write-Step "Step D OK."
  Set-Progress 100 "WhatsAppSender Install" "Install completed."
  Write-Step "Install completed OK."
}
catch {
  Write-ErrorRecord "InstallFlow" $_.Exception
  Write-Step "ERROR: $($_.Exception.Message)"
  Rollback-Basic
  throw
}
finally {
  try { Stop-Transcript | Out-Null } catch { }
  Write-Progress -Activity "WhatsAppSender Install" -Completed
}

<#
Logs:
- logs\install.log
- logs\step-by-step.log
- logs\errors.jsonl
#>
