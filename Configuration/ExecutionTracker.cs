namespace Configuration
{
    public class ExecutionTracker
    {
        private readonly string _outPath;
        public ExecutionTracker(string outPath)
        {
            _outPath = outPath;
            TimeStamp = ActiveTimeStamp ?? DateTime.Now.ToString("yyyyMMdd_HHmmss");
        }

        public string ExecutionRunning => GiveName(ExecutionRunningName);
        public string ExecutionFinished => GiveName(ExecutionFinishedName);
        private string GiveName(string folderName)
        {
            return Path.Combine(_outPath, $"{folderName}_{TimeStamp}");
        }

        private static string ExecutionRunningName => "ExecutionRunning";
        private static string ExecutionFinishedName => "ExecutionFinished";
        public string TimeStamp { get; }
        private string? ActiveTimeStamp
        {
            get
            {
                return GetCurrentFolder(ExecutionRunningName);
            }
        }

        public string? GetCurrentFolder(string folder)
        {
            var current = _outPath;
            var pattern = $"{folder}_*";
            var directories = Directory.GetDirectories(current, $"{folder}_*");

            var lastDirectory = directories
                .OrderByDescending(dir => dir)
                .FirstOrDefault();

            if (lastDirectory == null)
            {
                return null;
            }

            var folderName = Path.GetFileName(lastDirectory);

            if (folderName != null && folderName.StartsWith($"{folder}_"))
            {
                return folderName[(folder.Length + 1)..];
            }

            return null;
        }

        private static bool TryDeleteDirectory(string path, out Exception? error)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    error = null;
                    return true;
                }

                // Remove read-only attribute where possible (best-effort)
                foreach (var file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        var attr = File.GetAttributes(file);
                        if ((attr & FileAttributes.ReadOnly) != 0)
                            File.SetAttributes(file, attr & ~FileAttributes.ReadOnly);
                    }
                    catch
                    {
                        // ignore
                    }
                }

                Directory.Delete(path, recursive: true);
                error = null;
                return true;
            }
            catch (Exception ex)
            {
                error = ex;
                return false;
            }
        }

        private static void CopyDirectoryRecursive(string sourceDir, string destDir, FinalizeReport report)
        {
            // Create all directories first
            foreach (var dir in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
            {
                var rel = Path.GetRelativePath(sourceDir, dir);
                var targetDir = Path.Combine(destDir, rel);
                try
                {
                    Directory.CreateDirectory(targetDir);
                }
                catch (Exception ex)
                {
                    report.CopyFailures.Add(new Failure(targetDir, ex));
                }
            }

            // Copy files
            foreach (var file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
            {
                var rel = Path.GetRelativePath(sourceDir, file);
                var targetFile = Path.Combine(destDir, rel);

                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(targetFile)!);
                    File.Copy(file, targetFile, overwrite: true);
                }
                catch (Exception ex)
                {
                    // Common: IOException (locked), UnauthorizedAccessException, PathTooLongException
                    report.CopyFailures.Add(new Failure(file, ex));
                }
            }
        }


        public FinalizeReport FinalizeByCopyThenDelete(bool overwriteFinishedIfExists = false)
        {
            var runningPath = ExecutionRunning;
            var finishedPath = ExecutionFinished;

            if (!Directory.Exists(runningPath))
                throw new DirectoryNotFoundException($"Running folder not found: {runningPath}");

            if (Directory.Exists(finishedPath))
            {
                if (!overwriteFinishedIfExists)
                    throw new IOException($"Finished folder already exists: {finishedPath}");

                // overwrite means: try delete finished first (best-effort)
                TryDeleteDirectory(finishedPath, out _);
            }

            Directory.CreateDirectory(finishedPath);

            var report = new FinalizeReport(runningPath, finishedPath);

            // 1) Copy all contents
            CopyDirectoryRecursive(
                sourceDir: runningPath,
                destDir: finishedPath,
                report: report);

            // 2) Attempt delete running folder (best-effort)
            if (!TryDeleteDirectory(runningPath, out var deleteError))
            {
                report.DeleteFailures.Add(new Failure(runningPath, deleteError));
            }

            return report;
        }

        public sealed class FinalizeReport
        {
            public FinalizeReport(string runningPath, string finishedPath)
            {
                RunningPath = runningPath;
                FinishedPath = finishedPath;
            }

            public string RunningPath { get; }
            public string FinishedPath { get; }

            public List<Failure> CopyFailures { get; } = [];
            public List<Failure> DeleteFailures { get; } = [];

            public bool IsClean => CopyFailures.Count == 0 && DeleteFailures.Count == 0;
        }

        public CleanupReport CleanupOrphanedRunningFolders()
        {
            var report = new CleanupReport();

            if (!Directory.Exists(_outPath))
                return report;

            var runningDirs = Directory.GetDirectories(_outPath, $"{ExecutionRunningName}_*");

            foreach (var runningDir in runningDirs)
            {
                var folderName = Path.GetFileName(runningDir);

                if (folderName is null)
                    continue;

                // Extract timestamp
                var timestamp = folderName[(ExecutionRunningName.Length + 1)..];

                var finishedDir = Path.Combine(
                    _outPath,
                    $"{ExecutionFinishedName}_{timestamp}");

                // Only delete Running if Finished exists
                if (!Directory.Exists(finishedDir))
                    continue;

                if (!TryDeleteDirectory(runningDir, out var error))
                {
                    report.DeleteFailures.Add(new Failure(runningDir, error!));
                }
                else
                {
                    report.DeletedRunningFolders.Add(runningDir);
                }
            }

            return report;
        }

        public sealed class CleanupReport
        {
            public List<string> DeletedRunningFolders { get; } = new();
            public List<Failure> DeleteFailures { get; } = new();

            public bool IsClean => DeleteFailures.Count == 0;
        }
        public sealed record Failure(string Path, Exception Exception);

    }
}
