using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LedMusic.Converters
{
    public class WidthToRectConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new Rect(0d, 0d, (double)value, 1000d);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((Rect)value).Width;
        }
    }
}
