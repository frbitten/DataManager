using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows;

namespace Joo.Utils.Converters
{
    [ValueConversion(typeof(string), typeof(string))]
    public class EmptyStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null || string.IsNullOrEmpty(parameter.ToString()))
            {
                throw new ArgumentException("Informar o texto a retornar quando a string for vazia.");
            }


            try
            {
                if (value == null)
                {
                    return parameter.ToString();
                }
                else
                {
                    var actualValue = value.ToString();

                    if (string.IsNullOrEmpty(actualValue))
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
            return value;
        }
    }
}
