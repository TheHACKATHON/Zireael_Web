using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Client.ServiceReference;

namespace Client.Converters
{
    public class FileMessageToMessageGridVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            {
                if (value is MessageItem messageItem && messageItem.Message is MessageFileWCF messageFile)
                {
                    if (messageFile.Type.Equals(FileType.File))
                    {
                        return Visibility.Visible;
                    }

                    if (messageFile.Type.Equals(FileType.Image))
                    {
                        return Visibility.Visible;
                    }
                }
            }
            {
                if (value is MessageItem messageItem && messageItem.Message is MessageWCF message)
                {
                    return Visibility.Collapsed;
                }
            }


            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}