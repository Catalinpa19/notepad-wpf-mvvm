namespace WpfApp2
{
    public class TextDocumentViewModel : BaseViewModel
    {
        private string _title = "File 1";
        private string _content = string.Empty;
        private string? _filePath;
        private bool _isModified;

        public string Title
        {
            get => _title;
            set
            {
                if (SetProperty(ref _title, value))
                    OnPropertyChanged(nameof(DisplayTitle));
            }
        }

        public string Content
        {
            get => _content;
            set
            {
                if (_content != value)
                {
                    _content = value;
                    IsModified = true;
                    OnPropertyChanged();
                }
            }
        }

        public string? FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }

        public bool IsModified
        {
            get => _isModified;
            set
            {
                if (SetProperty(ref _isModified, value))
                    OnPropertyChanged(nameof(DisplayTitle));
            }
        }

        public string DisplayTitle => IsModified ? $"{Title} *" : Title;
    }
}