using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Client.ServiceReference;

namespace Client.Converters
{
    public class MessageToPreviewTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MessageWCF message)
            {
                if (message is MessageFileWCF messageFile)
                {
                    return $"File, {new FileNameConverter().Convert(messageFile.File.Name, null, 15, null)}";
                }

                return message.Text;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}