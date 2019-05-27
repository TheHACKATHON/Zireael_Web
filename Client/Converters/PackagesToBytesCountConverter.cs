using System;
using System.Globalization;
using System.Windows.Data;
using Client.Properties;
using Client.ServiceReference;

namespace Client.Converters
{
    public class PackagesToBytesCountConverter : IMultiValueConverter
    {
        private static readonly string[] SizeSuffixes = {"bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"};

        public object Convert(
            object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var packageSize = int.Parse(Resources.PackageSize);
            if (values[0] is int packagesCount && values[1] is MessageItem messageItem &&
                messageItem.Message is MessageFileWCF messageFile)
            {
                if (messageItem.FileDownloadState.Equals(FileDownloadState.Downloading) ||
                    messageItem.FileDownloadState.Equals(FileDownloadState.Uploadind))
                {
                    var currentSize = packagesCount * packageSize;
                    if (currentSize > messageFile.File.Lenght)
                    {
                        currentSize = messageFile.File.Lenght;
                    }

                    return $"{SizeSuffix(currentSize)}/{SizeSuffix(messageFile.File.Lenght)}";
                }

                return $"{SizeSuffix(messageFile.File.Lenght)}";
            }

            return null;
        }

        public object[] ConvertBack(
            object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private static string SizeSuffix(long value, int decimalPlaces = 1)
        {
            if (decimalPlaces < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(decimalPlaces));
            }

            if (value < 0)
            {
                return "-" + SizeSuffix(-value);
            }

            if (value.Equals(0))
            {
                return string.Format("{0:n" + decimalPlaces + "} bytes", 0);
            }

            var mag = (int) Math.Log(value, 1024);
            var adjustedSize = (decimal) value / (1L << (mag * 10));

            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag]);
        }
    }
}