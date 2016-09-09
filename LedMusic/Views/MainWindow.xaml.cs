using LedMusic.Interfaces;
using LedMusic.Models;
using LedMusic.StaticStuff;
using LedMusic.Viewmodels;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace LedMusic
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private DispatcherTimer scrollTimer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
            waveformTimeline.RegisterSoundPlayer(BassEngine.Instance);

            sliderBPM.DataContext = GlobalProperties.Instance;
            sliderFPS.DataContext = GlobalProperties.Instance;
            sliderLedCount.DataContext = GlobalProperties.Instance;
            sliderBeatOffset.DataContext = GlobalProperties.Instance;

            MainModel.Instance.PropertyChanged += MainModel_PropertyChanged;

            scrollTimer.Interval = TimeSpan.FromSeconds(0.3);
            scrollTimer.Tick += ScrollTimer_Tick;
            scrollTimer.Start();

        }

        private void ScrollTimer_Tick(object sender, EventArgs e)
        {
            //TODO: Either the bring-into-view function of the playback indicator needs to be disabled while dragging
            //or the playback position needs to be adjusted accordingly

            if (Mouse.LeftButton == MouseButtonState.Released)
                return;

            if (Mouse.GetPosition(scrollViewer).X < 20)
            {
                BassEngine.Instance.ChannelPosition -= 0.001 * BassEngine.Instance.ChannelLength;
            }
            else if (Mouse.GetPosition(scrollViewer).X > scrollViewer.ActualWidth - 20)
            {
                BassEngine.Instance.ChannelPosition += 0.001 * BassEngine.Instance.ChannelLength;
            }
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
            MainModel.Instance.ChangeAnimatableParameter(e.NewValue, ((PropertyModel)(((Slider)sender).DataContext)).Name);
            MainModel.Instance.updatePreviewStrip();
        }

        private void lvLayers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (lvLayers.SelectedItem == null)
                return;

            IAnimatable a = (IAnimatable)(((Layer)lvLayers.SelectedItem).Generator);
            PropertiesHelper.updateAnimatableProperties(ref a);
            MainModel.Instance.CurrentAnimatable = a;
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Save)
            {
                MainModel.Instance.Save();
            } else if (e.Command == ApplicationCommands.SaveAs)
            {
                MainModel.Instance.SaveAs();
            } else if (e.Command == ApplicationCommands.Open)
            {
                MainModel.Instance.Open();
            }
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void scrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            MainModel.Instance.ScrollViewerHorizontalOffset = e.HorizontalOffset + 10d;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MainModel.Instance.updatePreviewStrip();
        }

        private void sliderLayerAlpha_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainModel.Instance.updatePreviewStrip();
        }

        private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                MainModel.Instance.SelectKeyframeRange();
            }

            if (e.OriginalSource.GetType() != typeof(Rectangle))
            {
                MainModel.Instance.UnselectAllKeyframes();

                if (BassEngine.Instance.ChannelLength == 0)
                    return;

                double secondWidth = MainModel.Instance.TrackWidth / BassEngine.Instance.ChannelLength;
                BassEngine.Instance.ChannelPosition = e.GetPosition(canvas).X / secondWidth;
            }
        }

        private void canvas_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                if (BassEngine.Instance.CanPlay)
                {
                    BassEngine.Instance.Play();
                }
                else if (BassEngine.Instance.CanPause)
                {
                    BassEngine.Instance.Pause();
                }
            }
            else if (e.Key == Key.Left && MainModel.Instance.CurrentFrame > 0)
            {
                MainModel.Instance.CurrentFrame -= 1;
            }
            else if (e.Key == Key.Right)
            {
                MainModel.Instance.CurrentFrame += 1;
            }
            else if (e.Key == Key.C && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                MainModel.Instance.CopyKeyframes();
            }
            else if (e.Key == Key.V && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                MainModel.Instance.PasteKeyframes();
            }
            else if (e.Key == Key.Delete)
            {
                MainModel.Instance.DeleteKeyframes();
            }
        }
    }
}
