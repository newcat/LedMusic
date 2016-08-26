using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LedMusic.Converters
{
    public class OffsetRectConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            return new Rect((double)value[0], -10d, (double)value[1], 1000d);
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
