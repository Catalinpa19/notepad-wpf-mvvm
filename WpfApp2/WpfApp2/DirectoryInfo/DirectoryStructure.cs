using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WpfApp2
{
    public static class DirectoryStructure
    {
        public static List<DirectoryItem> GetLogicalDrives()
        {
            return Environment.GetLogicalDrives()
                .Select(drive => new DirectoryItem
                {
                    FullPath = drive,
                    Type = DirectoryItemType.Drive
                })
                .ToList();
        }

        public static List<DirectoryItem> GetDirectoryContents(string fullPath)
        {
            var items = new List<DirectoryItem>();

            try
            {
                foreach (var dir in Directory.GetDirectories(fullPath))
                {
                    items.Add(new DirectoryItem
                    {
                        FullPath = dir,
                        Type = DirectoryItemType.Folder
                    });
                }
            }
            catch
            {
            }

            try
            {
                foreach (var file in Directory.GetFiles(fullPath))
                {
                    items.Add(new DirectoryItem
                    {
                        FullPath = file,
                        Type = DirectoryItemType.File
                    });
                }
            }
            catch
            {
            }

            return items.OrderBy(i => i.Type == DirectoryItemType.File ? 1 : 0)
                        .ThenBy(i => i.Name)
                        .ToList();
        }

        public static string CreateNewTextFile(string directoryPath)
        {
            var baseName = "NewFile";
            var extension = ".txt";
            var index = 0;
            string fullPath;

            do
            {
                var fileName = index == 0 ? $"{baseName}{extension}" : $"{baseName}{index}{extension}";
                fullPath = Path.Combine(directoryPath, fileName);
                index++;
            }
            while (File.Exists(fullPath));

            File.WriteAllText(fullPath, string.Empty);
            return fullPath;
        }

        public static void CopyDirectory(string sourceDir, string destinationDir)
        {
            var dir = new DirectoryInfo(sourceDir);

            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {sourceDir}");

            Directory.CreateDirectory(destinationDir);

            foreach (var file in dir.GetFiles())
            {
                var targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath, true);
            }

            foreach (var subDir in dir.GetDirectories())
            {
                var newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir);
            }
        }
    }
}