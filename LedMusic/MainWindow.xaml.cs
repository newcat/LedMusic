using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LedMusic.Viewmodels;
using LedMusic.Models;

namespace LedMusic
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            waveformTimeline.RegisterSoundPlayer(BassEngine.Instance);

            sliderBPM.DataContext = GlobalProperties.Instance;
            sliderFPS.DataContext = GlobalProperties.Instance;
            sliderLedCount.DataContext = GlobalProperties.Instance;
            sliderBeatOffset.DataContext = GlobalProperties.Instance;

            MainModel.Instance.PropertyChanged += MainModel_PropertyChanged;

        }

        private void MainModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "TrackWidth" || e.PropertyName == "PlayerPosition")
                scrollPositionIndicatorIntoView();
        }

        private void btnIncreaseZoom_Click(object sender, RoutedEventArgs e)
        {
            MainModel.Instance.TrackWidth *= 2;
        }

        private void btnDecreaseZoom_Click(object sender, RoutedEventArgs e)
        {
            MainModel.Instance.TrackWidth *= 0.5;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void scrollPositionIndicatorIntoView()
        {

            double viewportWidth = scrollViewer.ActualWidth;
            double horizontalOffset = scrollViewer.HorizontalOffset;
            double playerPosition = MainModel.Instance.PlayerPosition;

            //Check if it is already in view
            double startPosition = horizontalOffset;
            double endPosition = horizontalOffset + viewportWidth;

            if (playerPosition < startPosition || playerPosition > endPosition)
            {
                scrollViewer.ScrollToHorizontalOffset(playerPosition - 0.1 * viewportWidth);
            }

        }

        private void sliderValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainModel.Instance.ChangeGeneratorParameter(e.NewValue, ((PropertyModel)(((Slider)sender).DataContext)).Name);
            MainModel.Instance.updatePreviewStrip();
        }

        private void lvLayers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ((Layer)lvLayers.SelectedItem).updateGenProperties();
        }
    }
}
