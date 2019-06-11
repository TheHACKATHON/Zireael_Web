using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdditionsLibrary
{
    public static class DateTimeToOnlineStringConverter
    {

        public static string Convert(DateTime value)
        {
            if (value is DateTime dateTime)
            {
                var now = DateTime.Now;
                if (dateTime.Year.Equals(now.Year) && dateTime.DayOfYear.Equals(now.DayOfYear))
                {
                    var result = $"был онлайн сегодня в {dateTime.ToShortTimeString()}";

                    var tmpDate = now - dateTime;
                    if (tmpDate.Hours < 1)
                    {
                        if (tmpDate.Minutes > 1)
                        {
                            result = $"был онлайн {tmpDate.Minutes} {MinutesToStringConverter(tmpDate.Minutes)} назад";
                        }
                        else
                        {
                            result = $"был онлайн только что";
                        }

                    }
                    else
                    {
                        if (tmpDate.Hours.Equals(1))
                        {
                            result = $"был онлайн час назад";
                        }
                        else
                        {
                            result = $"был онлайн {tmpDate.Hours} {HoursToStringConverter(tmpDate.Hours)} назад";
                        }

                    }

                    return result;

                }

                if (dateTime.Year.Equals(now.Year) && dateTime.DayOfYear.Equals(now.DayOfYear - 1))
                {
                    return $"был онлайн вчера в {dateTime.ToShortTimeString()}";
                }

                return $"был онлайн {dateTime.ToShortDateString()} в {dateTime.ToShortTimeString()}";
            }

            return null;
        }

        private static string MinutesToStringConverter(int min)
        {
            var minString = "минут";

            if (min < 10)
            {
                switch (min)
                {
                    case 1:
                        {
                            minString += "у";
                        }
                        break;
                    case 2:
                    case 3:
                    case 4:
                        {
                            minString += "ы";
                        }
                        break;
                }
            }
            else if (min < 20)
            {
               
            }
            else
            {
                while (min > 10)
                {
                    min -= 10;
                }
                switch (min)
                {
                    case 1:
                        {
                            minString += "у";
                        }
                        break;
                    case 2:
                    case 3:
                    case 4:
                        {
                            minString += "ы";
                        }
                        break;
                }
            }

            return minString;
        }

        private static string HoursToStringConverter(int hour)
        {
            var minString = "час";

            if (hour < 10)
            {
                switch (hour)
                {
                    case 2:
                    case 3:
                    case 4:
                        {
                            minString += "а";
                        }
                        break;
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                        {
                            minString += "ов";
                        }
                        break;
                }
            }
            else if (hour <= 20)
            {
                minString += "ов";
            }
            else if (hour < 24)
            {
                while (hour > 10)
                {
                    hour -= 10;
                }
                switch (hour)
                {
                    case 2:
                    case 3:
                        {
                            minString += "а";
                        }
                        break;
                }
            }

            return minString;
        }

    }

}
