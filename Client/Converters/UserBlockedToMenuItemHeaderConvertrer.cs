using System;
using System.Globalization;
using System.Windows.Data;

namespace Client.Converters
{
    public class UserBlockedToMenuItemHeaderConvertrer : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isUserBlocked && isUserBlocked)
            {
                return "Разблокировать";
            }
            else
            {
                return "Заблокировать";
            }
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}