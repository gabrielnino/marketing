<# 
Install-WhatsAppSender.ps1

Objetivo:
- Preparar carpetas según appsettings.json
- Copiar goku.png a WhatsApp:Message:ImageDirectory
- Instalar AutoIt en modo silencioso (autoit-v3-setup.exe)
- Logs: install.log (detallado) + step-by-step.log (paso a paso) + errors.jsonl (errores)
- Barra de progreso + mensajes descriptivos
- Reintentos + validación + rollback básico

Requisitos:
- Ejecutar como Administrador (para instalación y carpetas en C:\)
- Ejecutar desde la carpeta del paquete (_publish_prod) que contiene:
  - appsettings.json
  - autoit-v3-setup.exe
  - goku.png
#>

[CmdletBinding()]
param(
  [Parameter(Mandatory=$false)]
  [string]$PackageRoot = (Get-Location).Path,

  [Parameter(Mandatory=$false)]
  [string]$AppSettingsPath = (Join-Path (Get-Location).Path "appsettings.json"),

  [Parameter(Mandatory=$false)]
  [string]$LogsDir = (Join-Path (Get-Location).Path "logs"),

  [Parameter(Mandatory=$false)]
  [int]$RetryCount = 3,

  [Parameter(Mandatory=$false)]
  [int]$RetryDelaySeconds = 2
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$ProgressPreference = "Continue"

# ---------------------------
# Logging helpers
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
    try {
      return & $Action
    }
    catch {
      if ($i -ge $RetryCount) { throw }
      Write-Step "WARN: '$Name' falló (intento $i/$RetryCount). Reintentando en $RetryDelaySeconds s. Error: $($_.Exception.Message)"
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

function Track-DirIfCreated([string]$Path) {
  if (-not (Test-Path -LiteralPath $Path)) {
    New-Item -ItemType Directory -Path $Path -Force | Out-Null
    $CreatedDirs.Add($Path) | Out-Null
  }
}

function Rollback-Basic {
  Write-Step "Rollback: iniciando rollback básico..."
  # Remove copied files (only those created by this run)
  foreach ($f in $CopiedFiles) {
    try {
      if (Test-Path -LiteralPath $f) {
        Remove-Item -LiteralPath $f -Force
        Write-Step "Rollback: eliminado archivo copiado: $f"
      }
    } catch {
      Write-Step "Rollback WARN: no se pudo eliminar archivo $f. $($_.Exception.Message)"
    }
  }

  # Remove created dirs if empty
  foreach ($d in ($CreatedDirs | Sort-Object -Descending)) {
    try {
      if (Test-Path -LiteralPath $d) {
        $items = Get-ChildItem -LiteralPath $d -Force -ErrorAction SilentlyContinue
        if (-not $items -or $items.Count -eq 0) {
          Remove-Item -LiteralPath $d -Force
          Write-Step "Rollback: eliminado directorio vacío creado: $d"
        }
      }
    } catch {
      Write-Step "Rollback WARN: no se pudo eliminar directorio $d. $($_.Exception.Message)"
    }
  }

  if ($AutoItInstalledByScript) {
    Write-Step "Rollback NOTE: AutoIt fue instalado por este script. Desinstalación automática no aplicada (depende del instalador)."
    Write-Step "Rollback NOTE: Si necesitas revertir, desinstala AutoIt desde 'Apps & Features' o ejecuta el uninstaller de AutoIt."
  }
}

# ---------------------------
# Preflight checks
# ---------------------------
try {
  Set-Progress 2 "Instalación WhatsAppSender" "Preflight checks..."
  Write-Step "Inicio instalación. PackageRoot=$PackageRoot"
  Write-Step "LogsDir=$LogsDir"

  # Admin check
  $isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()
    ).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
  if (-not $isAdmin) { throw "Este script debe ejecutarse como Administrador." }

  if (-not (Test-Path -LiteralPath $AppSettingsPath)) {
    throw "No se encontró appsettings.json en: $AppSettingsPath"
  }

  $autoItSetup = Join-Path $PackageRoot "autoit-v3-setup.exe"
  if (-not (Test-Path -LiteralPath $autoItSetup)) {
    throw "No se encontró autoit-v3-setup.exe en: $autoItSetup"
  }

  $gokuSource = Join-Path $PackageRoot "goku.png"
  if (-not (Test-Path -LiteralPath $gokuSource)) {
    throw "No se encontró goku.png en: $gokuSource"
  }

  Write-Step "Preflight OK."
}
catch {
  Write-ErrorRecord "Preflight" $_.Exception
  throw
}

# ---------------------------
# Read config (appsettings.json)
# ---------------------------
# Nota: tomamos rutas desde el paquete (ej: OutFolder, DownloadFolder, DB, ImageDirectory). :contentReference[oaicite:0]{index=0}
$config = $null
try {
  Set-Progress 8 "Instalación WhatsAppSender" "Leyendo appsettings.json..."
  Write-Step "Leyendo configuración desde: $AppSettingsPath"

  $raw = Get-Content -LiteralPath $AppSettingsPath -Raw -Encoding UTF8
  $config = $raw | ConvertFrom-Json

  $outFolder      = $config.Paths.OutFolder
  $downloadFolder = $config.Paths.DownloadFolder

  # Parse DB path from connection string (Data Source=...;)
  $conn = $config.ConnectionStrings.DefaultConnection
  $dbPath = $null
  if ($conn -match "Data Source\s*=\s*([^;]+)") { $dbPath = $Matches[1].Trim() }

  $imageDir = $config.WhatsApp.Message.ImageDirectory
  $imageFileName = $config.WhatsApp.Message.ImageFileName

  if ([string]::IsNullOrWhiteSpace($outFolder))      { throw "Paths:OutFolder está vacío." }
  if ([string]::IsNullOrWhiteSpace($downloadFolder)) { throw "Paths:DownloadFolder está vacío." }
  if ([string]::IsNullOrWhiteSpace($dbPath))         { throw "No se pudo extraer Data Source del DefaultConnection." }
  if ([string]::IsNullOrWhiteSpace($imageDir))       { throw "WhatsApp:Message:ImageDirectory está vacío." }
  if ([string]::IsNullOrWhiteSpace($imageFileName))  { throw "WhatsApp:Message:ImageFileName está vacío." }

  Write-Step "Config OK: OutFolder=$outFolder; DownloadFolder=$downloadFolder; DbPath=$dbPath; ImageDir=$imageDir; ImageFileName=$imageFileName"
}
catch {
  Write-ErrorRecord "ReadConfig" $_.Exception
  throw
}

# ---------------------------
# Start transcript (detailed install log)
# ---------------------------
Start-Transcript -LiteralPath $InstallLog -Append | Out-Null

try {
  # ---------------------------
  # Step A: Ensure directory structure
  # ---------------------------
  Set-Progress 20 "Instalación WhatsAppSender" "Creando estructura de carpetas..."
  Write-Step "Paso A: creando/validando carpetas..."

  Invoke-Retry "Create OutFolder" {
    Track-DirIfCreated $outFolder
  } | Out-Null

  Invoke-Retry "Create DownloadFolder" {
    Track-DirIfCreated $downloadFolder
  } | Out-Null

  $dbDir = Split-Path -Parent $dbPath
  Invoke-Retry "Create DbFolder" {
    Track-DirIfCreated $dbDir
  } | Out-Null

  Invoke-Retry "Create ImageDirectory" {
    Track-DirIfCreated $imageDir
  } | Out-Null

  Write-Step "Paso A OK."

  # ---------------------------
  # Step B: Copy image
  # ---------------------------
  Set-Progress 40 "Instalación WhatsAppSender" "Copiando imagen goku.png..."
  Write-Step "Paso B: copiando imagen..."

  $gokuDest = Join-Path $imageDir $imageFileName
  Invoke-Retry "Copy goku.png" {
    Copy-Item -LiteralPath $gokuSource -Destination $gokuDest -Force
  } | Out-Null

  $CopiedFiles.Add($gokuDest) | Out-Null

  if (-not (Test-Path -LiteralPath $gokuDest)) {
    throw "Validación falló: no existe la imagen en destino: $gokuDest"
  }
  Write-Step "Paso B OK: imagen copiada a $gokuDest"

  # ---------------------------
  # Step C: Install AutoIt silently (if not installed)
  # ---------------------------
  Set-Progress 60 "Instalación WhatsAppSender" "Verificando/instalando AutoIt..."
  Write-Step "Paso C: verificación/instalación AutoIt..."

  function Test-AutoItInstalled {
    # Heurística: buscar AutoIt en registro
    $regPaths = @(
      "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\*",
      "HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*"
    )

    foreach ($p in $regPaths) {
      $apps = Get-ItemProperty -Path $p -ErrorAction SilentlyContinue
      foreach ($a in $apps) {
        if ($a.DisplayName -and $a.DisplayName -match "AutoIt") { return $true }
      }
    }

    # Fallback: exe typical paths
    $candidates = @(
      "$env:ProgramFiles\AutoIt3\AutoIt3.exe",
      "${env:ProgramFiles(x86)}\AutoIt3\AutoIt3.exe"
    )
    return ($candidates | Where-Object { Test-Path -LiteralPath $_ } | Measure-Object).Count -gt 0
  }

  $autoItAlready = Test-AutoItInstalled
  if ($autoItAlready) {
    Write-Step "AutoIt ya está instalado. Se omite instalación."
  } else {
    Write-Step "AutoIt no detectado. Instalando en modo silencioso..."

    # Nota: muchos installers de AutoIt aceptan /S (silencioso). Si tu build requiere otro switch, ajústalo aquí.
    # Probables switches: /S, /silent, /verysilent. Se usa /S por defecto.
    $args = "/S"

    Invoke-Retry "Install AutoIt" {
      $p = Start-Process -FilePath $autoItSetup -ArgumentList $args -Wait -PassThru -WindowStyle Hidden
      if ($p.ExitCode -ne 0) {
        throw "AutoIt installer retornó ExitCode=$($p.ExitCode)"
      }
    } | Out-Null

    Start-Sleep -Seconds 2

    if (-not (Test-AutoItInstalled)) {
      throw "Validación falló: AutoIt no quedó instalado (no se detecta en registro ni en Program Files)."
    }

    $AutoItInstalledByScript = $true
    Write-Step "AutoIt instalado correctamente."
  }

  # ---------------------------
  # Step D: Validate final state
  # ---------------------------
  Set-Progress 85 "Instalación WhatsAppSender" "Validando estado final..."
  Write-Step "Paso D: validación final..."

  # Check essential paths exist
  foreach ($p in @($outFolder, $downloadFolder, $dbDir, $imageDir)) {
    if (-not (Test-Path -LiteralPath $p)) { throw "Validación falló: carpeta requerida no existe: $p" }
  }
  if (-not (Test-Path -LiteralPath $gokuDest)) { throw "Validación falló: imagen no existe: $gokuDest" }

  Write-Step "Paso D OK."

  Set-Progress 100 "Instalación WhatsAppSender" "Instalación completada."
  Write-Step "Instalación completada OK."

  Write-Host ""
  Write-Host "Siguientes pasos (manuales):"
  Write-Host "1) Copiar toda la carpeta del publish a la máquina de producción (misma estructura)."
  Write-Host "2) Ejecutar este script en producción como Administrador."
  Write-Host "3) Crear la tarea programada (lo hacemos en el siguiente paso)."
}
catch {
  Write-ErrorRecord "InstallFlow" $_.Exception
  Write-Step "ERROR: $($_.Exception.Message)"
  Rollback-Basic
  throw
}
finally {
  Stop-Transcript | Out-Null
  Write-Progress -Activity "Instalación WhatsAppSender" -Completed
}

<# 
LOGS generados:
- logs\install.log        -> transcript detallado (stdout/stderr y acciones)
- logs\step-by-step.log   -> pasos ejecutados + warnings
- logs\errors.jsonl       -> errores estructurados (1 JSON por línea)

Rollback básico:
- elimina archivos copiados por este script
- elimina directorios creados por este script si quedan vacíos
- no desinstala AutoIt automáticamente (por seguridad); deja instrucción
#>
