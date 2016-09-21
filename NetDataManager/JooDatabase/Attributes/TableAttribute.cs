using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Joo.Database.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class TableAttribute : Attribute
    {
        private string name;
        public TableAttribute(string name)
        {
            this.name = name;
        }
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
    }
}
