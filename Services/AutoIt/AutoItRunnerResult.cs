using System.Diagnostics;
using System.Text;
using Configuration;
using Domain;
using Microsoft.Extensions.Logging;
using Services.Abstractions.AutoIt;


namespace Services.AutoIt
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
            var content = File.ReadAllText(scriptTemplete);
            Logger.LogDebug("AutoIt template size={Length} chars", content.Length);
            return content;
        }

        private void WriteScript(string scriptPath, string autoItScript)
        {
            Logger.LogInformation("Writing AutoIt script to {ScriptPath}", scriptPath);
            File.WriteAllText(scriptPath, autoItScript, new UTF8Encoding(false));
            Logger.LogInformation("AutoIt script written successfully. Size={Length} chars", autoItScript.Length);
        }

        public async Task<AutoItRunnerResult> RunAsync(
            TimeSpan timeout,
            string imagePath,
            bool useAutoItInterpreter = false,
            CancellationToken cancellationToken = default)
        {
            int step = 0;
            void LogStep(string message)
            {
                step++;
                Logger.LogInformation("Step {Step}: {Message}", step, message);
            }

            Logger.LogInformation("============================================================");
            Logger.LogInformation("AutoItRunner START");
            Logger.LogInformation(
                "timeout={Timeout} imagePath={ImagePath} useAutoItInterpreter={UseAutoItInterpreter} cancellationRequested={CancellationRequested}",
                timeout, imagePath, useAutoItInterpreter, cancellationToken.IsCancellationRequested);
            Logger.LogInformation("============================================================");

            // STEP 1
            LogStep("Resolving base paths and preparing AutoIt script.");

            var basePath = Directory.GetCurrentDirectory();
            Logger.LogDebug("CurrentDirectory={BasePath}", basePath);

            var scriptTemplete = Path.Combine(basePath, "whatsapp_upload.au3");
            var autoItScript = ReadTemplete(scriptTemplete);

            var scriptPath = Path.Combine(Path.GetTempPath(), $"whatsapp_upload_{Guid.NewGuid():N}.au3");
            Logger.LogInformation("Generated temp AutoIt script path {ScriptPath}", scriptPath);

            Logger.LogDebug("Replacing __FILE_TO_UPLOAD__ with {ImagePath}", imagePath);
            autoItScript = autoItScript.Replace("__FILE_TO_UPLOAD__", imagePath);

            var autoItInterpreterPath = Config.Paths.AutoItInterpreterPath;
            var autoItLogDir = Path.Combine(Config.Paths.OutFolder, "AutoItLog");

            Logger.LogInformation("autoItInterpreterPath={AutoItInterpreterPath}", autoItInterpreterPath);
            Logger.LogInformation("autoItLogDir={AutoItLogDir}", autoItLogDir);

            if (!Directory.Exists(autoItLogDir))
            {
                Logger.LogInformation("AutoItLog directory does not exist. Creating.");
                Directory.CreateDirectory(autoItLogDir);
            }

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var logFilePath = Path.Combine(autoItLogDir, $"AutoItRunner_{timestamp}.log");

            Logger.LogDebug("Replacing __AUTOIT_LOG_FILE__ with {LogFilePath}", logFilePath);
            autoItScript = autoItScript.Replace("__AUTOIT_LOG_FILE__", logFilePath);

            WriteScript(scriptPath, autoItScript);

            // STEP 2
            LogStep("Validating generated script and execution mode.");

            if (!File.Exists(scriptPath))
            {
                Logger.LogError("Generated AutoIt script missing at {ScriptPath}", scriptPath);
                throw new FileNotFoundException("Generated AutoIt script not found.", scriptPath);
            }

            if (!useAutoItInterpreter)
            {
                Logger.LogWarning(
                    "useAutoItInterpreter=false but script is .au3. Execution depends on file association.");
            }

            if (useAutoItInterpreter && !File.Exists(autoItInterpreterPath))
            {
                Logger.LogError("AutoIt interpreter not found at {Path}", autoItInterpreterPath);
                throw new FileNotFoundException("AutoIt interpreter not found.", autoItInterpreterPath);
            }

            // STEP 3
            LogStep("Building process command line.");

            string fileName;
            string arguments;

            if (useAutoItInterpreter)
            {
                fileName = autoItInterpreterPath!;
                arguments = scriptPath;
                Logger.LogInformation("Interpreter mode. FileName={FileName} Args={Arguments}", fileName, arguments);
            }
            else
            {
                fileName = scriptPath;
                arguments = imagePath ?? string.Empty;
                Logger.LogInformation("Direct mode. FileName={FileName} Args={Arguments}", fileName, arguments);
            }

            // STEP 4
            LogStep("Creating ProcessStartInfo.");

            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = autoItLogDir,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var stdout = new StringBuilder();
            var stderr = new StringBuilder();

            using var proc = new Process { StartInfo = psi, EnableRaisingEvents = true };
            var tcsExit = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

            // STEP 5
            LogStep("Attaching process event handlers.");

            proc.OutputDataReceived += (_, e) =>
            {
                if (e.Data != null)
                {
                    stdout.AppendLine(e.Data);
                    Logger.LogDebug("STDOUT: {Line}", e.Data);
                }
            };

            proc.ErrorDataReceived += (_, e) =>
            {
                if (e.Data != null)
                {
                    stderr.AppendLine(e.Data);
                    Logger.LogWarning("STDERR: {Line}", e.Data);
                }
            };

            proc.Exited += (_, __) =>
            {
                Logger.LogInformation("Process exited. ExitCode={ExitCode}", proc.ExitCode);
                tcsExit.TrySetResult(proc.ExitCode);
            };

            // STEP 6
            LogStep("Starting AutoIt process.");

            if (!proc.Start())
            {
                Logger.LogError("Process.Start() returned false.");
                throw new InvalidOperationException("Failed to start AutoIt process.");
            }

            Logger.LogInformation("Process started. PID={Pid}", proc.Id);
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            // STEP 7
            LogStep("Waiting for process completion.");

            using var timeoutCts = new CancellationTokenSource(timeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            bool timedOut = false;
            int exitCode;

            try
            {
                var completed = await Task.WhenAny(
                    tcsExit.Task,
                    Task.Delay(Timeout.Infinite, linkedCts.Token));

                if (completed == tcsExit.Task)
                {
                    exitCode = await tcsExit.Task;
                }
                else
                {
                    timedOut = timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested;

                    Logger.LogWarning(
                        "Process interrupted. TimedOut={TimedOut} ExternalCancel={ExternalCancel}",
                        timedOut, cancellationToken.IsCancellationRequested);

                    Logger.LogWarning("Killing AutoIt process tree.");
                    TryKillProcessTree(proc);

                    exitCode = await SafeWaitForExitCodeAsync(tcsExit, TimeSpan.FromSeconds(3));
                    if (exitCode == -1)
                        Logger.LogWarning("Exit code unresolved after forced termination.");
                }
            }
            finally
            {
                Logger.LogDebug("Final cleanup: ensuring process is terminated.");
                TryKillProcessTree(proc);
            }

            // STEP 8
            LogStep("Finalizing execution.");

            var duration = DateTimeOffset.UtcNow - DateTimeOffset.UtcNow.Add(-timeout);
            Logger.LogInformation(
                "AutoItRunner END. ExitCode={ExitCode} TimedOut={TimedOut} Duration={Duration}",
                exitCode, timedOut, duration);

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
                proc.Kill(entireProcessTree: true);
            }
            catch
            {
                // best-effort cleanup
            }
        }

        private static async Task<int> SafeWaitForExitCodeAsync(TaskCompletionSource<int> tcs, TimeSpan maxWait)
        {
            var completed = await Task.WhenAny(tcs.Task, Task.Delay(maxWait));
            return completed == tcs.Task ? await tcs.Task : -1;
        }
    }
}
