using LedMusic.Viewmodels;
using System;
using System.Globalization;
using System.Windows.Data;

namespace LedMusic.Converters
{
    class FrameToCanvasLeftConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double trackWidth = MainModel.Instance.TrackWidth;
            double totalSeconds = BassEngine.Instance.ChannelLength;
            double FPS = GlobalProperties.Instance.FPS;
            return ((int)value / FPS) * (trackWidth / totalSeconds);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
