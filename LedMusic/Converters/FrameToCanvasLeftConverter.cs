using LedMusic.Viewmodels;
using System;
using System.Globalization;
using System.Windows.Data;

namespace LedMusic.Converters
{
    class FrameToCanvasLeftConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            double trackWidth = (double)value[1];
            double totalSeconds = BassEngine.Instance.ChannelLength;
            double FPS = GlobalProperties.Instance.FPS;
            return ((int)value[0] / FPS) * (trackWidth / totalSeconds);
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
