using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Database.Exceptions
{
    internal class TableException:Exception
    {
        public TableException(string message)
            : base(message)
        {
            
        }
    }
}
