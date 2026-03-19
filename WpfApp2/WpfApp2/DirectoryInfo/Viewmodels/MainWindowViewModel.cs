using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace WpfApp2
{
    public class MainWindowViewModel : BaseViewModel
    {
        private TextDocumentViewModel? _selectedDocument;
        private bool _searchInAllTabs;
        private bool _isFolderExplorerVisible = true;
        private int _newFileCounter = 1;
        private string? _copiedFolderPath;

        public ObservableCollection<TextDocumentViewModel> Documents { get; set; }
        public ObservableCollection<DirectoryItemViewModel> DirectoryItems { get; set; }

        public TextDocumentViewModel? SelectedDocument
        {
            get => _selectedDocument;
            set
            {
                if (SetProperty(ref _selectedDocument, value))
                    CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool SearchInAllTabs
        {
            get => _searchInAllTabs;
            set
            {
                if (SetProperty(ref _searchInAllTabs, value))
                    OnPropertyChanged(nameof(SearchInSelectedTab));
            }
        }

        public bool SearchInSelectedTab
        {
            get => !SearchInAllTabs;
            set
            {
                if (value)
                {
                    SearchInAllTabs = false;
                    OnPropertyChanged(nameof(SearchInSelectedTab));
                }
            }
        }

        public bool IsFolderExplorerVisible
        {
            get => _isFolderExplorerVisible;
            set
            {
                if (SetProperty(ref _isFolderExplorerVisible, value))
                    OnPropertyChanged(nameof(IsStandardView));
            }
        }

        public bool IsStandardView
        {
            get => !IsFolderExplorerVisible;
            set
            {
                if (value)
                    IsFolderExplorerVisible = false;
            }
        }

        public RelayCommand NewFileCommand { get; }
        public RelayCommand OpenFileCommand { get; }
        public RelayCommand SaveFileCommand { get; }
        public RelayCommand SaveAsFileCommand { get; }
        public RelayCommand CloseFileCommand { get; }
        public RelayCommand CloseAllFilesCommand { get; }
        public RelayCommand ExitCommand { get; }

        public RelayCommand SetAllTabsScopeCommand { get; }
        public RelayCommand SetSelectedTabScopeCommand { get; }

        public RelayCommand ShowStandardViewCommand { get; }
        public RelayCommand ShowFolderExplorerCommand { get; }

        public RelayCommand NewFileInFolderCommand { get; }
        public RelayCommand CopyPathCommand { get; }
        public RelayCommand CopyFolderCommand { get; }
        public RelayCommand PasteFolderCommand { get; }

        public MainWindowViewModel()
        {
            Documents = new ObservableCollection<TextDocumentViewModel>();
            DirectoryItems = new ObservableCollection<DirectoryItemViewModel>();

            NewFileCommand = new RelayCommand(_ => NewFile());
            OpenFileCommand = new RelayCommand(_ => OpenFile());
            SaveFileCommand = new RelayCommand(_ => SaveFile(), _ => SelectedDocument != null);
            SaveAsFileCommand = new RelayCommand(_ => SaveFileAs(), _ => SelectedDocument != null);
            CloseFileCommand = new RelayCommand(_ => CloseSelectedFile(), _ => Documents.Count > 1 && SelectedDocument != null);
            CloseAllFilesCommand = new RelayCommand(_ => CloseAllFiles(), _ => Documents.Count > 1);
            ExitCommand = new RelayCommand(_ => Application.Current.MainWindow?.Close());

            SetAllTabsScopeCommand = new RelayCommand(_ => SearchInAllTabs = true);
            SetSelectedTabScopeCommand = new RelayCommand(_ => SearchInSelectedTab = true);

            ShowStandardViewCommand = new RelayCommand(_ => IsFolderExplorerVisible = false);
            ShowFolderExplorerCommand = new RelayCommand(_ => IsFolderExplorerVisible = true);

            NewFileInFolderCommand = new RelayCommand(
                p => NewFileInFolder(p as DirectoryItemViewModel),
                p => p is DirectoryItemViewModel item && item.Type != DirectoryItemType.File);

            CopyPathCommand = new RelayCommand(
                p => CopyPath(p as DirectoryItemViewModel),
                p => p is DirectoryItemViewModel item && item.Type != DirectoryItemType.File);

            CopyFolderCommand = new RelayCommand(
                p => CopyFolder(p as DirectoryItemViewModel),
                p => p is DirectoryItemViewModel item && item.Type != DirectoryItemType.File);

            PasteFolderCommand = new RelayCommand(
                p => PasteFolder(p as DirectoryItemViewModel),
                p => CanPasteFolder(p as DirectoryItemViewModel));

            LoadDrives();
            NewFile();
        }

        public void LoadDrives()
        {
            DirectoryItems.Clear();

            foreach (var drive in DirectoryStructure.GetLogicalDrives())
            {
                DirectoryItems.Add(new DirectoryItemViewModel(drive.FullPath, drive.Type));
            }
        }

        public void NewFile()
        {
            var doc = new TextDocumentViewModel
            {
                Title = $"File {_newFileCounter++}",
                Content = string.Empty,
                FilePath = null,
                IsModified = false
            };

            Documents.Add(doc);
            SelectedDocument = doc;
            CommandManager.InvalidateRequerySuggested();
        }

        public void OpenFile()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|C# files (*.cs)|*.cs|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
                OpenFileFromPath(dialog.FileName);
        }

        public void OpenFileFromPath(string path)
        {
            try
            {
                if (!File.Exists(path))
                    return;

                var existingDoc = Documents.FirstOrDefault(d =>
                    string.Equals(d.FilePath, path, StringComparison.OrdinalIgnoreCase));

                if (existingDoc != null)
                {
                    SelectedDocument = existingDoc;
                    return;
                }

                var content = File.ReadAllText(path);

                var doc = new TextDocumentViewModel
                {
                    Title = Path.GetFileName(path),
                    FilePath = path,
                    Content = content,
                    IsModified = false
                };

                Documents.Add(doc);
                SelectedDocument = doc;
                CommandManager.InvalidateRequerySuggested();
            }
            catch
            {
                MessageBox.Show(
                    "Fisierul nu poate fi deschis.",
                    "Open file",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        public bool SaveFile()
        {
            if (SelectedDocument == null)
                return false;

            if (string.IsNullOrWhiteSpace(SelectedDocument.FilePath))
                return SaveFileAs();

            File.WriteAllText(SelectedDocument.FilePath!, SelectedDocument.Content);
            SelectedDocument.IsModified = false;
            SelectedDocument.Title = Path.GetFileName(SelectedDocument.FilePath);
            return true;
        }

        public bool SaveFileAs()
        {
            if (SelectedDocument == null)
                return false;

            var dialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|C# files (*.cs)|*.cs|All files (*.*)|*.*",
                FileName = SelectedDocument.Title
            };

            if (dialog.ShowDialog() != true)
                return false;

            File.WriteAllText(dialog.FileName, SelectedDocument.Content);
            SelectedDocument.FilePath = dialog.FileName;
            SelectedDocument.Title = Path.GetFileName(dialog.FileName);
            SelectedDocument.IsModified = false;
            return true;
        }

        public bool ConfirmSaveForDocument(TextDocumentViewModel document)
        {
            if (!document.IsModified)
                return true;

            SelectedDocument = document;

            var result = MessageBox.Show(
                $"Fisierul '{document.Title}' are modificari nesalvate.\nVrei sa il salvezi?",
                "Confirmare salvare",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Cancel)
                return false;

            if (result == MessageBoxResult.No)
                return true;

            return SaveFile();
        }

        public void CloseSelectedFile()
        {
            if (SelectedDocument == null)
                return;

            if (Documents.Count <= 1)
            {
                MessageBox.Show(
                    "Trebuie sa ramana cel putin un fisier deschis.",
                    "Close file",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var doc = SelectedDocument;

            if (!ConfirmSaveForDocument(doc))
                return;

            Documents.Remove(doc);
            SelectedDocument = Documents.LastOrDefault();

            if (SelectedDocument == null)
                NewFile();

            CommandManager.InvalidateRequerySuggested();
        }

        public void CloseAllFiles()
        {
            if (Documents.Count == 0)
            {
                NewFile();
                return;
            }

            var docsToClose = Documents.ToList();

            foreach (var doc in docsToClose)
            {
                if (!ConfirmSaveForDocument(doc))
                    return;
            }

            Documents.Clear();
            NewFile();
            CommandManager.InvalidateRequerySuggested();
        }

        public bool ConfirmCloseApplication()
        {
            foreach (var doc in Documents.ToList())
            {
                if (!ConfirmSaveForDocument(doc))
                    return false;
            }

            return true;
        }

        public void FindText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            var docs = SearchInAllTabs
                ? Documents.ToList()
                : Documents.Where(d => d == SelectedDocument).ToList();

            if (docs.Count == 0)
                return;

            foreach (var doc in docs)
            {
                var index = doc.Content.IndexOf(text, StringComparison.OrdinalIgnoreCase);
                if (index >= 0)
                {
                    SelectedDocument = doc;
                    MessageBox.Show(
                        $"Text gasit in '{doc.Title}' la pozitia {index + 1}.",
                        "Find",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }
            }

            MessageBox.Show("Textul cautat nu a fost gasit.", "Find", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void ReplaceText(string findText, string replaceText)
        {
            if (string.IsNullOrWhiteSpace(findText))
                return;

            var docs = SearchInAllTabs
                ? Documents.ToList()
                : Documents.Where(d => d == SelectedDocument).ToList();

            foreach (var doc in docs)
            {
                var index = doc.Content.IndexOf(findText, StringComparison.OrdinalIgnoreCase);
                if (index >= 0)
                {
                    doc.Content = doc.Content.Remove(index, findText.Length).Insert(index, replaceText);
                    doc.IsModified = true;
                    SelectedDocument = doc;

                    MessageBox.Show(
                        $"Prima aparitie a fost inlocuita in '{doc.Title}'.",
                        "Replace",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }
            }

            MessageBox.Show("Textul cautat nu a fost gasit.", "Replace", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void ReplaceAllText(string findText, string replaceText)
        {
            if (string.IsNullOrWhiteSpace(findText))
                return;

            var docs = SearchInAllTabs
                ? Documents.ToList()
                : Documents.Where(d => d == SelectedDocument).ToList();

            int totalReplacements = 0;

            foreach (var doc in docs)
            {
                int count = 0;
                string content = doc.Content;
                int index = content.IndexOf(findText, StringComparison.OrdinalIgnoreCase);

                while (index >= 0)
                {
                    content = content.Remove(index, findText.Length).Insert(index, replaceText);
                    count++;
                    index = content.IndexOf(findText, index + replaceText.Length, StringComparison.OrdinalIgnoreCase);
                }

                if (count > 0)
                {
                    doc.Content = content;
                    doc.IsModified = true;
                    totalReplacements += count;
                }
            }

            MessageBox.Show(
                $"Replace All terminat. Inlocuiri efectuate: {totalReplacements}.",
                "Replace All",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void NewFileInFolder(DirectoryItemViewModel? item)
        {
            if (item == null || item.Type == DirectoryItemType.File)
                return;

            try
            {
                var fullPath = DirectoryStructure.CreateNewTextFile(item.FullPath);
                item.IsExpanded = true;
                item.Refresh();
                OpenFileFromPath(fullPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Nu s-a putut crea fisierul.\n{ex.Message}", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CopyPath(DirectoryItemViewModel? item)
        {
            if (item == null || item.Type == DirectoryItemType.File)
                return;

            try
            {
                Clipboard.SetText(item.FullPath);
                MessageBox.Show("Calea a fost copiata.", "Copy path", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch
            {
                MessageBox.Show("Calea nu a putut fi copiata.", "Copy path", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CopyFolder(DirectoryItemViewModel? item)
        {
            if (item == null || item.Type == DirectoryItemType.File)
                return;

            _copiedFolderPath = item.FullPath;
            CommandManager.InvalidateRequerySuggested();

            MessageBox.Show(
                $"Folderul '{new DirectoryInfo(item.FullPath).Name}' a fost copiat in memorie.",
                "Copy folder",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private bool CanPasteFolder(DirectoryItemViewModel? target)
        {
            return target != null
                   && target.Type != DirectoryItemType.File
                   && !string.IsNullOrWhiteSpace(_copiedFolderPath)
                   && Directory.Exists(_copiedFolderPath);
        }

        private void PasteFolder(DirectoryItemViewModel? target)
        {
            if (target == null || target.Type == DirectoryItemType.File)
                return;

            if (string.IsNullOrWhiteSpace(_copiedFolderPath) || !Directory.Exists(_copiedFolderPath))
            {
                MessageBox.Show(
                    "Nu exista niciun folder copiat.",
                    "Paste folder",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            try
            {
                var sourceFullPath = Path.GetFullPath(_copiedFolderPath);
                var targetFullPath = Path.GetFullPath(target.FullPath);

                if (string.Equals(sourceFullPath, targetFullPath, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show(
                        "Nu poti lipi folderul in acelasi folder.",
                        "Paste folder",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                if (targetFullPath.StartsWith(sourceFullPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show(
                        "Nu poti lipi folderul intr-un subfolder al lui.",
                        "Paste folder",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var sourceName = new DirectoryInfo(sourceFullPath).Name;
                var destinationPath = Path.Combine(targetFullPath, sourceName);

                if (Directory.Exists(destinationPath))
                    destinationPath = GetUniqueDirectoryPath(destinationPath);

                DirectoryStructure.CopyDirectory(sourceFullPath, destinationPath);

                target.IsExpanded = true;
                target.Refresh();

                MessageBox.Show(
                    "Folder lipit cu succes.",
                    "Paste folder",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Eroare la lipirea folderului.\n{ex.Message}",
                    "Paste folder",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private string GetUniqueDirectoryPath(string basePath)
        {
            if (!Directory.Exists(basePath))
                return basePath;

            var parentPath = Path.GetDirectoryName(basePath)!;
            var folderName = Path.GetFileName(basePath);
            int counter = 1;

            string newPath;
            do
            {
                newPath = Path.Combine(parentPath, $"{folderName}_Copy{counter}");
                counter++;
            }
            while (Directory.Exists(newPath));

            return newPath;
        }
    }
}