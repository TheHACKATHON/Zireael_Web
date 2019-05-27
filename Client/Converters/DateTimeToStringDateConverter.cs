using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Client.Converters
{
    public class DateTimeToStringDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                var now = DateTime.Now;
                if (dateTime.Year.Equals(now.Year) && dateTime.DayOfYear.Equals(now.DayOfYear))
                {
                    return dateTime.ToShortTimeString();
                }

                return dateTime.ToShortDateString();
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}