using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Client.Converters
{
    public class VerifyConvertrer : IMultiValueConverter
    {
        public object Convert(
            object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.ToList().All(v => v is bool b && b))
            {
                return true;
            }

            return false;
        }

        public object[] ConvertBack(
            object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}