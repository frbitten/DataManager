using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Database.Exceptions
{
    public class RelationshipException:Exception
    {
        public RelationshipException(string message):base(message)
        {
            
        }
    }
}
