using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Joo.Utils.Helpers
{
    public abstract class MonetaryFormatHelper
    {
        public static bool IsValueReset(String val)
        {
            if (val.Equals("0,00"))
            {
                return true;
            }
            return false;
        }
        public static bool CanDeleteChar(String val, int charPosition)
        {
            if (val.IndexOf(',') == charPosition)
            {
                return false;
            }
            return true;
        }
        public static bool ExceedsStandardSize(int size, int decimalHolders, int actualChar)
        {
            if (size >= (decimalHolders+2) && actualChar==0) // plceholders+(1)separador +quantidade mínima de unidade(=1)
            {
                return true;
            }
            return false;
        }
    }
}
