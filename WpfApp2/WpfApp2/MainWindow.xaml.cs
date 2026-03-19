using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfApp2
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();

            CommandBindings.Add(new CommandBinding(ApplicationCommands.Find, Find_Executed));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Replace, Replace_Executed));
        }

        private MainWindowViewModel? ViewModel => DataContext as MainWindowViewModel;

        private void TreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is TreeViewItem treeViewItem && treeViewItem.DataContext is DirectoryItemViewModel item)
            {
                if (item.Type == DirectoryItemType.File)
                {
                    ViewModel?.OpenFileFromPath(item.FullPath);
                    e.Handled = true;
                }
            }
        }

        private void Find_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Find_Click(sender, e);
        }

        private void Replace_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Replace_Click(sender, e);
        }

        private void Find_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
                return;

            var window = new FindReplaceWindow("Find", false)
            {
                Owner = this
            };

            if (window.ShowDialog() == true)
                ViewModel.FindText(window.FindTextValue);
        }

        private void Replace_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
                return;

            var window = new FindReplaceWindow("Replace", true)
            {
                Owner = this
            };

            if (window.ShowDialog() == true)
                ViewModel.ReplaceText(window.FindTextValue, window.ReplaceTextValue);
        }

        private void ReplaceAll_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
                return;

            var window = new FindReplaceWindow("Replace All", true)
            {
                Owner = this
            };

            if (window.ShowDialog() == true)
                ViewModel.ReplaceAllText(window.FindTextValue, window.ReplaceTextValue);
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            var about = new AboutWindow
            {
                Owner = this
            };
            about.ShowDialog();
        }

        private void Window_Closing(object? sender, CancelEventArgs e)
        {
            if (ViewModel == null)
                return;

            if (!ViewModel.ConfirmCloseApplication())
                e.Cancel = true;
        }
    }
}