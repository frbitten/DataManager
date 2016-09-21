using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace Joo.Utils.Converters
{
    public class StringLengthConverter : IValueConverter
    {        

        public object Convert(object value, Type targetType,object parameter, CultureInfo culture)
        {
            int delimiter=0;

            try
            {
                delimiter = int.Parse(parameter.ToString());
            }
            catch (InvalidCastException e)
            {
                
            }

            if ((value as string).Length > delimiter)
            {
                return (value as string).Substring(0,delimiter);
            }
            else
            {
                return (value as string);
            }
            
        }

        public object ConvertBack(object value, Type targetType,object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
