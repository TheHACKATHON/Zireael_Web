using System;
using System.Globalization;
using System.Windows.Data;

namespace Client.Converters
{
    public class UserInContactsToMenuItemHeaderConvertrer : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isUserInContacts && isUserInContacts)
            {
                return "Удалить из контактов";
            }
            else
            {
                return "Добавить в контакты";
            }
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}