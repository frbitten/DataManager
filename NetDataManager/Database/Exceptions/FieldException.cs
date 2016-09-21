using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Database.Exceptions
{
    public class FieldException:Exception
    {
        public FieldException(string message)
            : base(message)
        {
            
        }
    }
}
