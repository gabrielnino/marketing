using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Configuration;
using Domain.WhatsApp;
using Microsoft.Extensions.Logging;
using Services.Interfaces;

namespace Services
{
    public sealed class AutoItRunner(AppConfig config, ILogger<AutoItRunner> logger) : IAutoItRunner
    {
        private AppConfig Config { get; } = config;
        private ILogger<AutoItRunner> Logger { get; } = logger;

        private string ReadTemplete(string scriptTemplete)
        {
            if (!File.Exists(scriptTemplete))
            {
                Logger.LogError("AutoIt script template not found at {TemplatePath}", scriptTemplete);
                throw new FileNotFoundException("AutoIt script template not found.", scriptTemplete);
            }
            Logger.LogInformation("Reading AutoIt script template from {TemplatePath}", scriptTemplete);
            return File.ReadAllText(scriptTemplete);
        }

        private void WriteScript(string scriptPath,string autoItScript)
        {
            Logger.LogInformation("Writing AutoIt script to {ScriptPath}", scriptPath);
            File.WriteAllText(scriptPath, autoItScript, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        public async Task<AutoItRunnerResult> RunAsync(
            TimeSpan timeout,
            string imagePath,
            bool useAutoItInterpreter = false,
            CancellationToken cancellationToken = default)
        {
            // ------------------------------------------------------------
            // Step helper (same pattern: step-by-step logs)
            // ------------------------------------------------------------
            int step = 0;
            void LogStep(string message, params object[] args)
            {
                step++;
                Logger.LogInformation("Step {Step}: " + message, step, args);
            }

            Logger.LogInformation("============================================================");
            Logger.LogInformation("AutoItRunner START");
            Logger.LogInformation("timeout={Timeout} imagePath={ImagePath} useAutoItInterpreter={UseAutoItInterpreter} cancellationRequested={CancellationRequested}",
                timeout, imagePath, useAutoItInterpreter, cancellationToken.IsCancellationRequested);
            Logger.LogInformation("============================================================");

            // ------------------------------------------------------------
            // STEP 1: Resolve paths / folders
            // ------------------------------------------------------------
            LogStep("Resolving base paths and log folder.");

            var basePath = Directory.GetCurrentDirectory();
            var scriptTemplete = Path.Combine(basePath, "whatsapp_upload.au3");
            var autoItScript = ReadTemplete(scriptTemplete);
           
            var scriptPath = Path.Combine(Path.GetTempPath(), $"whatsapp_upload_{Guid.NewGuid():N}.au3");
            var scriptAutoIt = scriptPath;

            autoItScript = autoItScript.Replace("__FILE_TO_UPLOAD__", imagePath);
          
            var autoItInterpreterPath = Config.Paths.AutoItInterpreterPath;

            var autoItLogDir = Path.Combine(Config.Paths.OutFolder, "AutoItLog");
             
            Logger.LogInformation("basePath={BasePath}", basePath);
            Logger.LogInformation("scriptOrExePath={ScriptOrExePath}", scriptAutoIt);
            Logger.LogInformation("autoItInterpreterPath={AutoItInterpreterPath}", autoItInterpreterPath);
            Logger.LogInformation("autoItLogDir={AutoItLogDir}", autoItLogDir);

            if (!Directory.Exists(autoItLogDir))
            {
                Logger.LogInformation("AutoItLog directory does not exist. Creating: {AutoItLogDir}", autoItLogDir);
                Directory.CreateDirectory(autoItLogDir);
            }
            else
            {
                Logger.LogInformation("AutoItLog directory exists.");
            }

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var logFilePath = Path.Combine(autoItLogDir, $"AutoItRunner_{timestamp}.log");
            autoItScript = autoItScript.Replace("__AUTOIT_LOG_FILE__", logFilePath);
            WriteScript(scriptAutoIt, autoItScript);
            Logger.LogInformation("logFilePath={LogFilePath}", logFilePath);

            // ------------------------------------------------------------
            // STEP 2: Validate inputs
            // ------------------------------------------------------------
            LogStep("Validating inputs and required files.");

            if (string.IsNullOrWhiteSpace(scriptAutoIt))
            {
                Logger.LogError("Validation failed: scriptOrExePath is null/empty.");
                throw new ArgumentException("scriptOrExePath is required.", nameof(scriptAutoIt));
            }

            if (!File.Exists(scriptAutoIt))
            {
                Logger.LogError("Validation failed: AutoIt script/exe not found at {Path}", scriptAutoIt);
                throw new FileNotFoundException("AutoIt script/exe not found.", scriptAutoIt);
            }

            if (useAutoItInterpreter)
            {
                Logger.LogInformation("useAutoItInterpreter=true. Validating interpreter path.");
                if (string.IsNullOrWhiteSpace(autoItInterpreterPath))
                {
                    Logger.LogError("Validation failed: autoItInterpreterPath is null/empty while useAutoItInterpreter=true.");
                    throw new ArgumentException(
                        "autoItInterpreterPath is required when useAutoItInterpreter=true.",
                        nameof(autoItInterpreterPath));
                }

                if (!File.Exists(autoItInterpreterPath))
                {
                    Logger.LogError("Validation failed: AutoIt interpreter not found at {Path}", autoItInterpreterPath);
                    throw new FileNotFoundException("AutoIt interpreter not found.", autoItInterpreterPath);
                }
            }
            else
            {
                Logger.LogInformation("useAutoItInterpreter=false. Will run compiled script/exe directly (or .au3 if supported by environment).");
            }

            // ------------------------------------------------------------
            // STEP 3: Build command line
            // ------------------------------------------------------------
            LogStep("Building process command line.");

            string fileName;
            string arguments;

            if (useAutoItInterpreter)
            {
                // AutoIt3.exe "C:\path\script.au3" <args>
                fileName = autoItInterpreterPath!;
                // NOTE: Keeping original behavior; if you intended to pass imagePath, change this line accordingly.
                arguments = scriptAutoIt;

                Logger.LogInformation("Mode=Interpreter. fileName={FileName}", fileName);
                Logger.LogInformation("Mode=Interpreter. script={Script}", scriptAutoIt);
                Logger.LogInformation("Mode=Interpreter. arguments={Arguments}", arguments);
            }
            else
            {
                // Compiled AutoIt executable
                fileName = scriptAutoIt;
                arguments = imagePath ?? string.Empty;

                Logger.LogInformation("Mode=Direct. fileName={FileName}", fileName);
                Logger.LogInformation("Mode=Direct. arguments={Arguments}", arguments);
            }

            // ------------------------------------------------------------
            // STEP 4: Build ProcessStartInfo
            // ------------------------------------------------------------
            LogStep("Creating ProcessStartInfo.");

            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                // NOTE: Keeping original behavior; WorkingDirectory typically should be a directory, not a file path.
                WorkingDirectory = autoItLogDir,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            Logger.LogInformation("ProcessStartInfo: FileName={FileName}", psi.FileName);
            Logger.LogInformation("ProcessStartInfo: Arguments={Arguments}", psi.Arguments);
            Logger.LogInformation("ProcessStartInfo: WorkingDirectory={WorkingDirectory}", psi.WorkingDirectory);
            Logger.LogInformation("ProcessStartInfo: UseShellExecute={UseShellExecute} CreateNoWindow={CreateNoWindow} RedirectStdOut={RedirectStdOut} RedirectStdErr={RedirectStdErr}",
                psi.UseShellExecute, psi.CreateNoWindow, psi.RedirectStandardOutput, psi.RedirectStandardError);

            var stdout = new StringBuilder();
            var stderr = new StringBuilder();

            using var proc = new Process { StartInfo = psi, EnableRaisingEvents = true };
            var tcsExit = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

            // ------------------------------------------------------------
            // STEP 5: Attach handlers
            // ------------------------------------------------------------
            LogStep("Attaching process output/error/exit handlers.");

            proc.OutputDataReceived += (_, e) =>
            {
                if (e.Data != null)
                {
                    stdout.AppendLine(e.Data);
                    Logger.LogDebug("AutoIt STDOUT: {Line}", e.Data);
                }
            };

            proc.ErrorDataReceived += (_, e) =>
            {
                if (e.Data != null)
                {
                    stderr.AppendLine(e.Data);
                    Logger.LogDebug("AutoIt STDERR: {Line}", e.Data);
                }
            };

            proc.Exited += (_, __) =>
            {
                try
                {
                    Logger.LogInformation("Process exited. ExitCode={ExitCode}", proc.ExitCode);
                    tcsExit.TrySetResult(proc.ExitCode);
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Exited handler failed to read ExitCode. Returning -1.");
                    tcsExit.TrySetResult(-1);
                }
            };

            // ------------------------------------------------------------
            // STEP 6: Write start log file + start process
            // ------------------------------------------------------------
            LogStep("Writing start record to log file and starting process.");

            var start = DateTimeOffset.UtcNow;

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(logFilePath) ?? ".");
                File.AppendAllText(
                    logFilePath,
                    $"[{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}] START AutoIt: {fileName} {arguments}{Environment.NewLine}");
                Logger.LogInformation("Wrote START line to log file: {LogFilePath}", logFilePath);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to write START line to log file: {LogFilePath}", logFilePath);
            }

            Logger.LogInformation("Starting process...");
            if (!proc.Start())
            {
                Logger.LogError("Process.Start() returned false.");
                throw new InvalidOperationException("Failed to start AutoIt process.");
            }

            Logger.LogInformation("Process started. PID={Pid}", proc.Id);

            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            // ------------------------------------------------------------
            // STEP 7: Wait for completion or timeout/cancel
            // ------------------------------------------------------------
            LogStep("Waiting for process completion or timeout/cancellation.");

            using var timeoutCts = new CancellationTokenSource(timeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            bool timedOut = false;
            int exitCode;

            try
            {
                Logger.LogInformation("Wait started. timeout={Timeout} cancellationRequested={CancellationRequested}",
                    timeout, cancellationToken.IsCancellationRequested);

                var completed = await Task.WhenAny(
                        tcsExit.Task,
                        Task.Delay(Timeout.Infinite, linkedCts.Token))
                    .ConfigureAwait(false);

                if (completed == tcsExit.Task)
                {
                    Logger.LogInformation("Process completed before timeout/cancel.");
                    exitCode = await tcsExit.Task.ConfigureAwait(false);
                }
                else
                {
                    timedOut = timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested;

                    Logger.LogWarning("Wait interrupted. timedOut={TimedOut} externalCancel={ExternalCancelRequested}",
                        timedOut, cancellationToken.IsCancellationRequested);

                    // Kill hard (includes child processes)
                    LogStep("Killing process tree due to timeout/cancellation.");
                    TryKillProcessTree(proc);

                    // Wait briefly to allow Exited event to fire / resources release
                    LogStep("Waiting briefly for exit code after kill.");
                    exitCode = await SafeWaitForExitCodeAsync(tcsExit, TimeSpan.FromSeconds(3)).ConfigureAwait(false);
                }
            }
            finally
            {
                // Ensure process is not left running
                LogStep("Final cleanup: ensuring process is not left running.");
                TryKillProcessTree(proc);
            }

            // ------------------------------------------------------------
            // STEP 8: Finalize and persist END line
            // ------------------------------------------------------------
            LogStep("Finalizing result and writing END record.");

            var duration = DateTimeOffset.UtcNow - start;

            Logger.LogInformation("AutoItRunner END. ExitCode={ExitCode} TimedOut={TimedOut} Duration={Duration}",
                exitCode, timedOut, duration);

            try
            {
                File.AppendAllText(
                    logFilePath,
                    $"[{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}] END AutoIt: ExitCode={exitCode} TimedOut={timedOut} Duration={duration}{Environment.NewLine}");
                Logger.LogInformation("Wrote END line to log file: {LogFilePath}", logFilePath);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to write END line to log file: {LogFilePath}", logFilePath);
            }

            return new AutoItRunnerResult
            {
                ExitCode = exitCode,
                TimedOut = timedOut,
                StdOut = stdout.ToString(),
                StdErr = stderr.ToString(),
                Duration = duration,
                LogFilePath = logFilePath
            };
        }

        private static void TryKillProcessTree(Process proc)
        {
            try
            {
                if (proc.HasExited) return;

                // .NET 5+ supports Kill(entireProcessTree: true)
                proc.Kill(entireProcessTree: true);
            }
            catch
            {
                // swallow: we only want best-effort cleanup
            }
        }

        private static async Task<int> SafeWaitForExitCodeAsync(TaskCompletionSource<int> tcs, TimeSpan maxWait)
        {
            try
            {
                var completed = await Task.WhenAny(tcs.Task, Task.Delay(maxWait)).ConfigureAwait(false);
                if (completed == tcs.Task) return await tcs.Task.ConfigureAwait(false);
            }
            catch
            {
                // ignore
            }

            return -1;
        }
    }
}
