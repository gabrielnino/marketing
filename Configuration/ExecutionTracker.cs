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

        public string ExecutionFolder => Path.Combine(_outPath, $"{FolderName}_{TimeStamp}");
        private static string FolderName => "Execution";
        public string TimeStamp { get; }
        private string? ActiveTimeStamp
        {
            get
            {
                return GetCurrentFolder(FolderName);
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
                return folderName[(folder.Length + 1)..]; // +1 for underscore
            }

            return null;
        }
    }
}
