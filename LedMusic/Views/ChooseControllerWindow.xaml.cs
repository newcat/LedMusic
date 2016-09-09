using System.Windows;

namespace LedMusic
{
    /// <summary>
    /// Interaction logic for ChooseControllerWindow.xaml
    /// </summary>
    public partial class ChooseControllerWindow : Window
    {
        public ChooseControllerWindow()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void btnChoose_Click(object sender, RoutedEventArgs e)
        {
            if (lbControllers.SelectedItem == null)
            {
                DialogResult = false;
            }
            else
            {
                DialogResult = true;
            }
            Close();
        }
    }
}
