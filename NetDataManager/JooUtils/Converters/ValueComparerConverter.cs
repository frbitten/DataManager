using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace Joo.Utils.Converters
{
    public class ValueComparerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (ValueToCompare == null)
                throw new ArgumentException();

            if (Operator == OPERATORS_TO_COMPARE.INVALID)
                throw new ArgumentException();

            if (value == null)
                throw new ArgumentException();

            var realValue = System.Convert.ToDouble(value);

            switch (Operator)
            {
                case OPERATORS_TO_COMPARE.GREATER:
                    if (realValue > ValueToCompare)
                        return true;
                    break;
                case OPERATORS_TO_COMPARE.GREATER_OR_EQUAL:
                    if (realValue >= ValueToCompare)
                        return true;
                    break;
                case OPERATORS_TO_COMPARE.EQUAL:
                    if (realValue == ValueToCompare)
                        return true;
                    break;
                case OPERATORS_TO_COMPARE.LESS_OR_EQUAL:
                    if (realValue <= ValueToCompare)
                        return true;
                    break;
                case OPERATORS_TO_COMPARE.LESS:
                    if (realValue < ValueToCompare)
                        return true;
                    break;
                case OPERATORS_TO_COMPARE.DIFFERENT:
                    if (realValue != ValueToCompare)
                        return true;
                    break;
                default:
                    throw new ArgumentException();
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public double? ValueToCompare { get; set; }

        public OPERATORS_TO_COMPARE Operator { get; set; }
    }

    public enum OPERATORS_TO_COMPARE
    {
        INVALID,
        GREATER,
        GREATER_OR_EQUAL,
        EQUAL,
        LESS_OR_EQUAL,
        LESS,
        DIFFERENT
    }


}
