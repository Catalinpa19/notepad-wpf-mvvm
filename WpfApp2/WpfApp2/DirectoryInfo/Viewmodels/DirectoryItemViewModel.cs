using System.Collections.ObjectModel;

namespace WpfApp2
{
    public class DirectoryItemViewModel : BaseViewModel
    {
        private bool _isExpanded;

        public string FullPath { get; set; }
        public DirectoryItemType Type { get; set; }

        public string Name
        {
            get
            {
                if (Type == DirectoryItemType.Drive)
                    return FullPath;
                return DirectoryItem.GetFileFolderName(FullPath);
            }
        }

       

        public ObservableCollection<DirectoryItemViewModel?> Children { get; set; }

        public bool CanExpand => Type != DirectoryItemType.File;

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (SetProperty(ref _isExpanded, value) && value)
                    Expand();
            }
        }

        public DirectoryItemViewModel(string fullPath, DirectoryItemType type)
        {
            FullPath = fullPath;
            Type = type;
            Children = new ObservableCollection<DirectoryItemViewModel?>();

            if (CanExpand)
                Children.Add(null);
        }

        private void Expand()
        {
            if (!CanExpand)
                return;

            if (Children.Count != 1 || Children[0] != null)
                return;

            Children.Clear();

            var contents = DirectoryStructure.GetDirectoryContents(FullPath);
            foreach (var item in contents)
            {
                Children.Add(new DirectoryItemViewModel(item.FullPath, item.Type));
            }
        }

        public void Refresh()
        {
            if (!CanExpand)
                return;

            Children.Clear();

            var contents = DirectoryStructure.GetDirectoryContents(FullPath);
            foreach (var item in contents)
            {
                Children.Add(new DirectoryItemViewModel(item.FullPath, item.Type));
            }
        }
    }
}