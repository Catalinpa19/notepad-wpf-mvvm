using System.Windows;

namespace WpfApp2
{
    public partial class FindReplaceWindow : Window
    {
        public string FindTextValue => FindTextBox.Text;
        public string ReplaceTextValue => ReplaceTextBox.Text;

        public FindReplaceWindow(string title, bool showReplaceBox)
        {
            InitializeComponent();
            Title = title;

            ReplaceLabel.Visibility = showReplaceBox ? Visibility.Visible : Visibility.Collapsed;
            ReplaceTextBox.Visibility = showReplaceBox ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}