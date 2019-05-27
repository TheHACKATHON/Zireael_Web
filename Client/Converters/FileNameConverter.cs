using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Client.Converters
{
    public class FileNameConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int maxSymbs = 25;
            if (parameter is int max)
            {
                maxSymbs = max;
            }
            if (value is string fullFileName)
            {
                if (fullFileName.Length < maxSymbs)
                {
                    return fullFileName;
                }

                var newNameStart = fullFileName.Substring(0, (maxSymbs / 2) + 1);
                var newNameEnd = fullFileName.Substring(fullFileName.Length - ((maxSymbs / 2) + 4));

                return $"{newNameStart}...{newNameEnd}";
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}