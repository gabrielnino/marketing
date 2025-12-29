<#
Install-WhatsAppSender.ps1

Production installer for WhatsAppSender deployment folder.

Adds:
- .NET 8 Desktop Runtime check/install (optional but recommended)
- Google Chrome x64 check/install (NEW)
- AutoIt silent install
- Folder creation from appsettings.json
- Copy goku.png to configured target
- Logs: install.log, step-by-step.log, errors.jsonl
- Basic rollback (files/empty dirs)

Run as Administrator. Windows PowerShell 5.1 compatible.
#>

[CmdletBinding()]
param(
  [string]$PackageRoot = (Get-Location).Path,
  [string]$AppSettingsPath = (Join-Path (Get-Location).Path "appsettings.json"),
  [string]$LogsDir = (Join-Path (Get-Location).Path "logs"),
  [int]$RetryCount = 3,
  [int]$RetryDelaySeconds = 2,

  # If you already install .NET elsewhere, set to $false.
  [bool]$EnsureDotNet8 = $true,

  # NEW: ensure Chrome is present
  [bool]$EnsureChrome = $true
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$ProgressPreference = "Continue"

try { [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12 } catch { }

# ---------------------------
# Helpers
# ---------------------------
function Ensure-Dir([string]$Path) {
  if ([string]::IsNullOrWhiteSpace($Path)) { throw "Ensure-Dir received empty path." }
  if (-not (Test-Path -LiteralPath $Path)) {
    New-Item -ItemType Directory -Path $Path -Force | Out-Null
  }
}

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
$ChromeInstalledByScript = $false

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

  if ($AutoItInstalledByScript)  { Write-Step "Rollback NOTE: AutoIt installed by script. Auto-uninstall not performed." }
  if ($DotNetInstalledByScript)  { Write-Step "Rollback NOTE: .NET installed by script. Auto-uninstall not performed." }
  if ($ChromeInstalledByScript)  { Write-Step "Rollback NOTE: Chrome installed by script. Auto-uninstall not performed." }
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
  # Step 0: Ensure .NET 8 (Desktop Runtime recommended)
  # ------------------------------------------------------------
  if ($EnsureDotNet8) {
    Set-Progress 5 "WhatsAppSender Install" "Checking .NET 8..."
    Write-Step "Step 0: checking .NET 8..."

    $dotnetExe = "C:\Program Files\dotnet\dotnet.exe"

    function Test-DotNet8Installed {
      if (Test-Path -LiteralPath $dotnetExe) {
        try {
          $r = & $dotnetExe --list-runtimes 2>$null
          if ($r -match "Microsoft\.NETCore\.App\s+8\.") { return $true }
        } catch { }
      }
      return $false
    }

    if (Test-DotNet8Installed) {
      Write-Step ".NET 8 already installed."
    } else {
      # You requested this URL earlier; leaving it as-is.
      $dotnetUrl  = "https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.416/dotnet-sdk-8.0.416-win-x64.exe"
      $dotnetPath = Join-Path $PackageRoot "dotnet-sdk-8.0.416-win-x64.exe"

      Write-Step ".NET 8 not detected. Downloading + installing..."

      Invoke-Retry "Download .NET 8" {
        Invoke-WebRequest -Uri $dotnetUrl -OutFile $dotnetPath
      } | Out-Null

      if (-not (Test-Path -LiteralPath $dotnetPath)) { throw "Download failed: .NET installer not found at $dotnetPath" }

      Invoke-Retry "Install .NET 8" {
        $p = Start-Process -FilePath $dotnetPath -ArgumentList @("/install","/quiet","/norestart") -Wait -PassThru -WindowStyle Hidden
        if ($p.ExitCode -ne 0) { throw "dotnet installer ExitCode=$($p.ExitCode)" }
      } | Out-Null

      if (Test-Path -LiteralPath $dotnetExe) { $env:PATH = "C:\Program Files\dotnet;$env:PATH" }

      if (-not (Test-DotNet8Installed)) { throw ".NET install completed but .NET 8 runtime still not detected." }

      $DotNetInstalledByScript = $true
      Write-Step ".NET 8 installed successfully."
    }
  }

  # ------------------------------------------------------------
  # Step 0.5: Ensure Google Chrome (x64) - NEW
  # ------------------------------------------------------------
  if ($EnsureChrome) {
    Set-Progress 12 "WhatsAppSender Install" "Checking Google Chrome..."
    Write-Step "Step 0.5: checking Google Chrome..."

    $chromeExe = "C:\Program Files\Google\Chrome\Application\chrome.exe"

    function Test-ChromeInstalled {
      return (Test-Path -LiteralPath $chromeExe)
    }

    if (Test-ChromeInstalled) {
      try {
        $ver = & $chromeExe --version 2>$null
        Write-Step "Chrome detected: $ver"
      } catch {
        Write-Step "Chrome detected (version not readable)."
      }
    } else {
      Write-Step "Chrome not detected. Installing Chrome Enterprise x64..."

      $chromeMsiUrl  = "https://dl.google.com/dl/chrome/install/googlechromestandaloneenterprise64.msi"
      $chromeMsiPath = Join-Path $PackageRoot "googlechromestandaloneenterprise64.msi"

      Invoke-Retry "Download Chrome MSI" {
        Write-Step "Downloading Chrome MSI..."
        Invoke-WebRequest -Uri $chromeMsiUrl -OutFile $chromeMsiPath
      } | Out-Null

      if (-not (Test-Path -LiteralPath $chromeMsiPath)) {
        throw "Chrome MSI download failed: $chromeMsiPath not found."
      }

      Invoke-Retry "Install Chrome MSI" {
        Write-Step "Installing Chrome (msiexec /i ... /qn)..."
        $p = Start-Process -FilePath "msiexec.exe" -ArgumentList @("/i", "`"$chromeMsiPath`"", "/qn", "/norestart") -Wait -PassThru -WindowStyle Hidden
        if ($p.ExitCode -ne 0) { throw "Chrome MSI install ExitCode=$($p.ExitCode)" }
      } | Out-Null

      if (-not (Test-ChromeInstalled)) {
        throw "Chrome install completed but chrome.exe not found at expected path: $chromeExe"
      }

      try {
        $ver = & $chromeExe --version 2>$null
        Write-Step "Chrome installed successfully: $ver"
      } catch {
        Write-Step "Chrome installed successfully."
      }

      $ChromeInstalledByScript = $true
    }
  }

  # ------------------------------------------------------------
  # Read config (appsettings.json)
  # ------------------------------------------------------------
  Set-Progress 20 "WhatsAppSender Install" "Reading appsettings.json..."
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
  Set-Progress 35 "WhatsAppSender Install" "Creating folder structure..."
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
  Set-Progress 50 "WhatsAppSender Install" "Copying goku.png..."
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
  # Step D: Final validation + Chrome presence
  # ------------------------------------------------------------
  Set-Progress 90 "WhatsAppSender Install" "Final validation..."
  Write-Step "Step D: final validation..."

  foreach ($p in @($outFolder, $downloadFolder, $dbDir, $imageDir)) {
    if (-not (Test-Path -LiteralPath $p)) { throw "Validation failed: required folder missing: $p" }
  }
  if (-not (Test-Path -LiteralPath $gokuDest)) { throw "Validation failed: image missing: $gokuDest" }

  if ($EnsureChrome) {
    $chromeExe = "C:\Program Files\Google\Chrome\Application\chrome.exe"
    if (-not (Test-Path -LiteralPath $chromeExe)) {
      throw "Validation failed: Chrome not found at: $chromeExe"
    }
  }

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
