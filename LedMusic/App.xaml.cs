using LedMusic.Viewmodels;
using System.Windows;

namespace LedMusic
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {

            MainWindow mw = new MainWindow();
            MainWindow = mw;

            MainModel model = MainModel.Instance;
            mw.DataContext = model;
            mw.Show();
            mw.Closed += Mw_Closed;

        }

        private void Mw_Closed(object sender, System.EventArgs e)
        {
            SoundEngine.Instance.Stop();
            SoundEngine.Instance.CleanupPlayback();
        }
    }
}
