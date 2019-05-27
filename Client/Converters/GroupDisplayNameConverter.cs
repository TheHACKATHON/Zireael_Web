using System;
using System.Globalization;
using System.Windows.Data;

namespace Client.Converters
{
    public class GroupDisplayNameConverter : IMultiValueConverter
    {
        public object Convert(
            object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is string userDisplayName && !string.IsNullOrEmpty(userDisplayName))
            {
                return userDisplayName;
            }
            else if (values[1] is string groupName && !string.IsNullOrEmpty(groupName))
            {
                return groupName;
            }

            return string.Empty;
        }

        public object[] ConvertBack(
            object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}