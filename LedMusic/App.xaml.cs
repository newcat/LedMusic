using LedMusic.Generators;
using LedMusic.Models;
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

            model.Layers.Add(new Layer(0, new DotGenerator()));
            model.Layers.Add(new Layer(1, new DotGenerator()));
            model.Layers.Add(new Layer(2, new DotGenerator()));
            model.Layers.Sort();

        }
    }
}
