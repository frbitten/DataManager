using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows;

namespace Utils.Converters
{
    [ValueConversion(typeof(int), typeof(String))]
    public class ZeroConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var actualValue = System.Convert.ToInt32(value);

                if (actualValue == 0)
                {
                    if (parameter == null)
                        return string.Empty;
                    else
                    {
                        return parameter.ToString();
                    }
                }
            }
            catch
            {
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return 0;

            string text = value.ToString();

            if (text == string.Empty)
                return 0;

            foreach (var c in text)
            {
                if (char.IsNumber(c) == false)
                    return DependencyProperty.UnsetValue;
            }

            try
            {
                return System.Convert.ToInt32(text);
            }
            catch
            {
                return DependencyProperty.UnsetValue;
            }

        }
    }
}
