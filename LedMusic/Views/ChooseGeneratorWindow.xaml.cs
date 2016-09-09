using System.Windows;

namespace LedMusic
{
    /// <summary>
    /// Interaction logic for ChooseGeneratorWindow.xaml
    /// </summary>
    public partial class ChooseGeneratorWindow : Window
    {
        public ChooseGeneratorWindow()
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
            if (lbGenerators.SelectedItem == null)
            {
                DialogResult = false;
            } else
            {
                DialogResult = true;
            }
            Close();
        }
    }
}
