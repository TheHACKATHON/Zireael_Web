using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Client.Converters
{
    public class UsersCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                var usersString = $"{count} пользовател";

                if (count < 10 || count > 20)
                {
                    while (count > 10)
                    {
                        count -= 10;
                    }

                    switch (count)
                    {
                        case 1:
                        {
                            usersString += "ь";
                        }
                            break;
                        case 2:
                        case 3:
                        case 4:
                        {
                            usersString += "я";
                        }
                            break;
                        default:
                        {
                            usersString += "ей";
                        }
                            break;
                    }
                }
                else if (count < 20)
                {
                    usersString += "ей";
                }
               
                return usersString;
            }

            return null;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}