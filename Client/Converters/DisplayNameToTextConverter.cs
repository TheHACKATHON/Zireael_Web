using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Client.Converters
{
    public class DisplayNameToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var maxNameLength = 20;
            if (value is string message)
            {
                if (message.Length < maxNameLength)
                {
                    return message;
                }

                return message.Remove(maxNameLength - 3) + "...";
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}