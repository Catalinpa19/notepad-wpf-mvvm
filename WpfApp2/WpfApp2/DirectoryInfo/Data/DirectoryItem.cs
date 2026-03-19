namespace WpfApp2
{
    public class DirectoryItem
    {
        public string FullPath { get; set; } = string.Empty;
        public DirectoryItemType Type { get; set; }

        public string Name => GetFileFolderName(FullPath);

        public static string GetFileFolderName(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;

            var normalized = path.Replace('/', '\\');

            if (normalized.EndsWith("\\") && normalized.Length > 3)
                normalized = normalized.Substring(0, normalized.Length - 1);

            var lastIndex = normalized.LastIndexOf('\\');

            if (lastIndex < 0)
                return normalized;

            if (lastIndex == normalized.Length - 1)
                return normalized;

            return normalized.Substring(lastIndex + 1);
        }
    }
}