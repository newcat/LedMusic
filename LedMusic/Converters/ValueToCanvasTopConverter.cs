using System;
using System.Globalization;
using System.Windows.Data;

namespace LedMusic.Converters
{
    class ValueToCanvasTopConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //(value - minValue) / (maxValue - minValue)
            double value = System.Convert.ToDouble(values[0]);
            double minValue = System.Convert.ToDouble(values[1]);
            double maxValue = System.Convert.ToDouble(values[2]);
            double percentage = (value - minValue) / (maxValue - minValue);
            return 80 - 80 * percentage - Math.Sqrt(200) * (1 - percentage);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
