using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace Joo.Utils
{
    [ValueConversion(typeof(DateTime), typeof(String))]
    public class JooStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(string))
            {
                throw new NotImplementedException("JooStringConverter only supports converting to string");
            }
            if (value.GetType() == typeof(DateTime))
            {
                return ConvertDateTime((DateTime)value, parameter as string, culture);
            }
            if (value.GetType() == typeof(decimal))
            {

            }
            throw new NotImplementedException("Target type " + targetType.Name + " is not implemented");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                if (targetType == typeof(DateTime))
                {
                    return ConvertBackDateTime(value as string,parameter as string,culture);
                }
                if (targetType == typeof(decimal))
                {
                }
                throw new NotImplementedException("Target type " + targetType.Name + " is not implemented");
            }
            throw new ArgumentException("Value is not of type string");
        }

        private DateTime ConvertBackDateTime(string value, string parameter, CultureInfo culture)
        {
            DateTime date;

            if (culture != null)
            {
                if (parameter is string && !string.IsNullOrEmpty(parameter as string))
                {
                    if (DateTime.TryParseExact(value, parameter as string, culture.DateTimeFormat, DateTimeStyles.None, out date))
                    {
                        return date;
                    }
                    else
                    {
                        throw new FormatException("Formato da data esta incorreto. Favor usar o formato " + parameter + ".");
                    }
                }
                else
                {
                    if (DateTime.TryParse(value, culture.DateTimeFormat, DateTimeStyles.None, out date))
                    {
                        return date;
                    }
                    else
                    {
                        throw new FormatException("Formato da data esta incorreto. Favor usar " + culture.DateTimeFormat.FullDateTimePattern + ".");
                    }
                }
            }
            else
            {
                if (parameter is string && !string.IsNullOrEmpty(parameter as string))
                {
                    if (DateTime.TryParseExact(value, parameter as string, CultureInfo.CurrentCulture.DateTimeFormat, DateTimeStyles.None, out date))
                    {
                        return date;
                    }
                    else
                    {
                        throw new FormatException("Formato da data esta incorreto. Favor usar o formato " + parameter + ".");
                    }
                }
                else
                {
                    if (DateTime.TryParse(value, out date))
                    {
                        return date;
                    }
                    else
                    {
                        throw new FormatException("Formato da data esta incorreto.favor usar o formato " + CultureInfo.CurrentCulture.DateTimeFormat.FullDateTimePattern + ".");
                    }
                }
            }
        }

        private string ConvertDateTime(DateTime value, string parameter, CultureInfo culture)
        {
            if (culture != null)
            {
                if (parameter is string && !string.IsNullOrEmpty(parameter as string))
                {
                    return value.ToString(parameter as string, culture.DateTimeFormat);
                }
                else
                {
                    return value.ToString(culture.DateTimeFormat);
                }
            }
            else
            {
                if (parameter is string && !string.IsNullOrEmpty(parameter as string))
                {
                    return value.ToString(parameter as string);
                }
                else
                {
                    return value.ToString();
                }
            }
        }
    }

}
