using LedMusic.Interfaces;
using LedMusic.Models;
using LedMusic.Viewmodels;
using System;
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

            MainModel.Instance.PropertyChanged += MainModel_PropertyChanged;

            scrollTimer.Interval = TimeSpan.FromSeconds(0.3);
            scrollTimer.Tick += ScrollTimer_Tick;
            scrollTimer.Start();

        }

        private void ScrollTimer_Tick(object sender, EventArgs e)
        {
            //TODO: Either the bring-into-view function of the playback indicator needs to be disabled while dragging
            //or the playback position needs to be adjusted accordingly

            if (Mouse.LeftButton == MouseButtonState.Released || !scrollViewer.IsMouseOver)
                return;

            if (Mouse.GetPosition(scrollViewer).X < 20)
            {
                SoundEngine.Instance.Position -= TimeSpan.FromMilliseconds(100);
            }
            else if (Mouse.GetPosition(scrollViewer).X > scrollViewer.ActualWidth - 20)
            {
                SoundEngine.Instance.Position += TimeSpan.FromMilliseconds(100);
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

                if (SoundEngine.Instance.Length == TimeSpan.Zero)
                    return;

                double secondWidth = MainModel.Instance.TrackWidth / SoundEngine.Instance.Length.TotalSeconds;
                SoundEngine.Instance.Position = TimeSpan.FromSeconds(e.GetPosition(canvas).X / secondWidth);
            }
        }

        private void canvas_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                if (SoundEngine.Instance.CanPlay)
                {
                    SoundEngine.Instance.Play();
                }
                else if (SoundEngine.Instance.CanPause)
                {
                    SoundEngine.Instance.Pause();
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

        private void lvLayers_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (lvLayers.SelectedItem == null)
                return;

            if (lvLayers.SelectedItem is IAnimatable)
            {
                MainModel.Instance.CurrentAnimatable = (IAnimatable)lvLayers.SelectedItem;
            } else if (lvLayers.SelectedItem is Layer)
            {
                MainModel.Instance.CurrentAnimatable = (IAnimatable)(((Layer)lvLayers.SelectedItem).Generator);
            }

        }
    }
}
