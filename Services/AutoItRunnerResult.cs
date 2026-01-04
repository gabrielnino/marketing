using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public sealed class AutoItRunnerResult
    {
        public required int ExitCode { get; init; }
        public required bool TimedOut { get; init; }
        public required string StdOut { get; init; }
        public required string StdErr { get; init; }
        public required TimeSpan Duration { get; init; }
        public string? LogFilePath { get; init; }
    }
    public sealed class AutoItRunner
    {
        /// <summary>
        /// Runs AutoIt OUT-OF-PROCESS with a hard timeout.
        /// Works with:
        ///  - compiled script: myScript.exe (autoit compiled)
        ///  - or AutoIt3.exe + script.au3 (when useAutoItInterpreter=true)
        /// </summary>
        public async Task<AutoItRunnerResult> RunAsync(
            string scriptOrExePath,
            string? autoItInterpreterPath,        // e.g. C:\Program Files (x86)\AutoIt3\AutoIt3.exe
            string scriptArguments,               // arguments passed to script/exe (optional)
            TimeSpan timeout,
            string? workingDirectory = null,
            string? logFilePath = null,
            bool useAutoItInterpreter = false,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(scriptOrExePath))
                throw new ArgumentException("scriptOrExePath is required.", nameof(scriptOrExePath));

            if (!File.Exists(scriptOrExePath))
                throw new FileNotFoundException("AutoIt script/exe not found.", scriptOrExePath);

            if (useAutoItInterpreter)
            {
                if (string.IsNullOrWhiteSpace(autoItInterpreterPath))
                    throw new ArgumentException("autoItInterpreterPath is required when useAutoItInterpreter=true.", nameof(autoItInterpreterPath));
                if (!File.Exists(autoItInterpreterPath))
                    throw new FileNotFoundException("AutoIt interpreter not found.", autoItInterpreterPath);
            }

            workingDirectory ??= Path.GetDirectoryName(scriptOrExePath) ?? Environment.CurrentDirectory;

            // Build process start info
            string fileName;
            string arguments;

            if (useAutoItInterpreter)
            {
                // AutoIt3.exe "C:\path\script.au3" <args>
                fileName = autoItInterpreterPath!;
                arguments = Quote(scriptOrExePath) + (string.IsNullOrWhiteSpace(scriptArguments) ? "" : " " + scriptArguments);
            }
            else
            {
                // Compiled AutoIt executable
                fileName = scriptOrExePath;
                arguments = scriptArguments ?? string.Empty;
            }

            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var stdout = new StringBuilder();
            var stderr = new StringBuilder();

            using var proc = new Process { StartInfo = psi, EnableRaisingEvents = true };

            var tcsExit = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

            proc.OutputDataReceived += (_, e) =>
            {
                if (e.Data != null) stdout.AppendLine(e.Data);
            };
            proc.ErrorDataReceived += (_, e) =>
            {
                if (e.Data != null) stderr.AppendLine(e.Data);
            };
            proc.Exited += (_, __) =>
            {
                try { tcsExit.TrySetResult(proc.ExitCode); }
                catch { tcsExit.TrySetResult(-1); }
            };

            var start = DateTimeOffset.UtcNow;

            if (logFilePath != null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(logFilePath) ?? ".");
                File.AppendAllText(logFilePath, $"[{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}] START AutoIt: {fileName} {arguments}{Environment.NewLine}");
            }

            if (!proc.Start())
                throw new InvalidOperationException("Failed to start AutoIt process.");

            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            using var timeoutCts = new CancellationTokenSource(timeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            bool timedOut = false;
            int exitCode;

            try
            {
                // Wait for either process exit OR cancellation/timeout
                var completed = await Task.WhenAny(tcsExit.Task, Task.Delay(Timeout.Infinite, linkedCts.Token)).ConfigureAwait(false);

                if (completed == tcsExit.Task)
                {
                    exitCode = await tcsExit.Task.ConfigureAwait(false);
                }
                else
                {
                    // If we are here, it was cancelled or timed out
                    timedOut = timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested;

                    // Kill hard (includes child processes)
                    TryKillProcessTree(proc);

                    // Wait briefly to allow Exited event to fire / resources release
                    exitCode = await SafeWaitForExitCodeAsync(tcsExit, TimeSpan.FromSeconds(3)).ConfigureAwait(false);
                }
            }
            finally
            {
                // Ensure process is not left running
                TryKillProcessTree(proc);
            }

            var duration = DateTimeOffset.UtcNow - start;

            if (logFilePath != null)
            {
                File.AppendAllText(logFilePath,
                    $"[{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}] END AutoIt: ExitCode={exitCode} TimedOut={timedOut} Duration={duration}{Environment.NewLine}");
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

        private static string Quote(string s)
            => s.Contains(' ') || s.Contains('"') ? "\"" + s.Replace("\"", "\\\"") + "\"" : s;

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
            catch { /* ignore */ }
            return -1;
        }
    }
}
