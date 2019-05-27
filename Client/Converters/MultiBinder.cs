using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Client
{
    public class MultiBinder : MultiBinding
    {
        private void CheckConverter()
        {
            if (Converter == null && DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                Converter = new DummyConverter();
            }
        }

        public MultiBinder(BindingBase b1, BindingBase b2, object converter)
        {
            Bindings.Add(b1);
            Bindings.Add(b2);
            Converter = converter as IMultiValueConverter;
            CheckConverter();
        }

        public MultiBinder(BindingBase b1, BindingBase b2, BindingBase b3, object converter)
        {
            Bindings.Add(b1);
            Bindings.Add(b2);
            Bindings.Add(b3);
            Converter = converter as IMultiValueConverter;
            CheckConverter();
        }

        private class DummyConverter : IMultiValueConverter
        {
            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }
    }
}