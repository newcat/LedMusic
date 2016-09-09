using System.Windows;

namespace LedMusic
{
    /// <summary>
    /// Interaction logic for RenameDialog.xaml
    /// </summary>
    public partial class RenameDialog : Window
    {
        public RenameDialog()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void btnRename_Click(object sender, RoutedEventArgs e)
        {
            if (textBox.Text == "")
            {
                DialogResult = false;
            }
            else
            {
                DialogResult = true;
            }
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            textBox.SelectAll();
            textBox.Focus();
        }
    }
}
