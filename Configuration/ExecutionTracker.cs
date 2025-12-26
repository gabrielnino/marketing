namespace Configuration
{
    public class ExecutionTracker
    {
        // 1) Constants
        private const string ExecutionRunningName = "ExecutionRunning";
        private const string ExecutionFinishedName = "ExecutionFinished";

        // 2) Fields
        private readonly string _outPath;

        // 3) Ctor
        public ExecutionTracker(string outPath)
        {
            _outPath = outPath;
            TimeStamp = ActiveTimeStamp ?? DateTime.Now.ToString("yyyyMMdd_HHmmss");
        }

        // 4) Public properties
        public string TimeStamp { get; }
        public string ExecutionRunning => BuildPath(ExecutionRunningName, TimeStamp);

        // 5) Public methods
        public FinalizeReport FinalizeByCopyThenDelete(bool overwriteFinishedIfExists = false)
        {
            var runningPath = ExecutionRunning;
            var finishedPath = ExecutionFinished;

            if (!Directory.Exists(runningPath))
                throw new DirectoryNotFoundException($"Folder not found: {runningPath}");

            if (Directory.Exists(finishedPath))
            {
                if (!overwriteFinishedIfExists)
                    throw new IOException($"Folder already exists: {finishedPath}");

                TryDeleteDirectory(finishedPath, out _);
            }

            Directory.CreateDirectory(finishedPath);

            var report = new FinalizeReport(runningPath, finishedPath);

            CopyDirectoryRecursive(runningPath, finishedPath, report);

            if (!TryDeleteDirectory(runningPath, out var deleteError) && deleteError is not null)
                report.DeleteFailures.Add(new Failure(runningPath, deleteError));

            return report;
        }

        public CleanupReport CleanupOrphanedRunningFolders()
        {
            var report = new CleanupReport();

            if (!Directory.Exists(_outPath))
                return report;

            foreach (var runningDir in Directory.GetDirectories(_outPath, $"{ExecutionRunningName}_*"))
            {
                var ts = ExtractTimeStampFromFolder(runningDir, ExecutionRunningName);
                if (ts is null)
                    continue;

                var finishedDir = BuildPath(ExecutionFinishedName, ts);
                if (!Directory.Exists(finishedDir))
                    continue;

                if (!TryDeleteDirectory(runningDir, out var error) && error is not null)
                    report.DeleteFailures.Add(new Failure(runningDir, error));
                else
                    report.DeletedRunningFolders.Add(runningDir);
            }

            return report;
        }

        // 6) Private properties
        private string ExecutionFinished => BuildPath(ExecutionFinishedName, TimeStamp);
        private string? ActiveTimeStamp => GetLatestTimeStamp();

        // 7) Private helpers
        private string BuildPath(string prefix, string timeStamp)
            => Path.Combine(_outPath, $"{prefix}_{timeStamp}");

        private string? GetLatestTimeStamp()
        {
            if (!Directory.Exists(_outPath))
                return null;

            var folderRunning = $"{ExecutionRunningName}_*";
            var pathRunning = Directory.GetDirectories(_outPath, folderRunning);
            var folderTimeStamp = $"{pathRunning[0].Split("\\").Last().Split('_')[1]}_{pathRunning[0].Split("\\").Last().Split('_')[2]}";
            var pathFinished = Directory.GetDirectories(_outPath, $"{ExecutionFinishedName}_{folderTimeStamp}");
            if (pathFinished.Length != 0)
                return null;

            var last = pathRunning.OrderByDescending(d => d).FirstOrDefault();
            if (last is null)
                return null;

            var name = Path.GetFileName(last);
            if (name is null || !name.StartsWith($"{ExecutionRunningName}_"))
                return null;

            return name[(ExecutionRunningName.Length + 1)..];
        }

        private static string? ExtractTimeStampFromFolder(string fullPath, string prefix)
        {
            var name = Path.GetFileName(fullPath);
            if (name is null || !name.StartsWith($"{prefix}_"))
                return null;

            return name[(prefix.Length + 1)..];
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

                // Normaliza atributos en directorios
                foreach (var dir in Directory.GetDirectories(path, "*", SearchOption.AllDirectories))
                {
                    try { File.SetAttributes(dir, FileAttributes.Normal); } catch { }
                }

                // Normaliza atributos en archivos
                foreach (var file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
                {
                    try { File.SetAttributes(file, FileAttributes.Normal); } catch { }
                }

                // Directorio raíz
                try { File.SetAttributes(path, FileAttributes.Normal); } catch { }

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
            foreach (var dir in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
            {
                var rel = Path.GetRelativePath(sourceDir, dir);
                var targetDir = Path.Combine(destDir, rel);

                try { Directory.CreateDirectory(targetDir); }
                catch (Exception ex) { report.CopyFailures.Add(new Failure(targetDir, ex)); }
            }

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
                    report.CopyFailures.Add(new Failure(file, ex));
                }
            }
        }

        // 8) Nested types
        public sealed record Failure(string Path, Exception Exception);
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

        public List<ExecutionTracker.Failure> CopyFailures { get; } = [];
        public List<ExecutionTracker.Failure> DeleteFailures { get; } = [];

        public bool IsClean => CopyFailures.Count == 0 && DeleteFailures.Count == 0;
    }

    public sealed class CleanupReport
    {
        public List<string> DeletedRunningFolders { get; } = [];
        public List<ExecutionTracker.Failure> DeleteFailures { get; } = [];

        public bool IsClean => DeleteFailures.Count == 0;
    }
}
