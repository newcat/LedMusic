using LedMusic.Models;
using LedMusic.Viewmodels;
using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace LedMusic
{
    /// <summary>
    /// Interaction logic for KeyframeTrack.xaml
    /// </summary>
    public partial class KeyframeTrack : UserControl
    {
        public KeyframeTrack()
        {
            InitializeComponent();
        }

        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isShiftPressed())
            {
                ((Keyframe)((Rectangle)sender).DataContext).IsSelected = true;
                MainModel.Instance.SelectKeyframeRange();
            } else if (isCtrlPressed())
            {
                ((Keyframe)((Rectangle)sender).DataContext).IsSelected = true;
            } else
            {
                ((Keyframe)((Rectangle)sender).DataContext).IsSelected = true;
            }
            ((Rectangle)sender).CaptureMouse();
            BassEngine.Instance.ChannelPosition = ((Keyframe)((Rectangle)sender).DataContext).Frame / (double)GlobalProperties.Instance.FPS;

        }

        private void Rectangle_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!(isShiftPressed() || isCtrlPressed()))
            {
                MainModel.Instance.UnselectAllKeyframes();
                ((Keyframe)((Rectangle)sender).DataContext).IsSelected = true;
            }
            ((Rectangle)sender).ReleaseMouseCapture();
        }

        private bool isShiftPressed() { return Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift); }
        private bool isCtrlPressed() { return Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl); }

        private void Rectangle_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (BassEngine.Instance.ChannelLength == 0)
                    return;

                //TODO: Fix the issue that the y coord seemingly also affects the delta
                //(Width of a second) * (duration of a frame)
                double frameWidth = (MainModel.Instance.TrackWidth / BassEngine.Instance.ChannelLength) * (1.0 / GlobalProperties.Instance.FPS);
                double xDelta = e.GetPosition((Rectangle)sender).X; // - 0.5 * Math.Sqrt(200);
                Debug.WriteLine(Convert.ToInt32(xDelta / frameWidth));
                if (Math.Abs(xDelta) > frameWidth)
                    MainModel.Instance.MoveKeyframes(Convert.ToInt32(xDelta / frameWidth));
            }
        }
    }
}
