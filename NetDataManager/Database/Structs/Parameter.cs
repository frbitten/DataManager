using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Database.Structs
{
    public class Parameter
    {
        #region [ Constructor ]
        public Parameter()
        {
            Name = string.Empty;
            Value = null;
        }
        public Parameter(string name, object value)
        {
            this.Name = name;
            this.Value = value;
        }
        #endregion

        #region [ Properties ]
        public string Name
        {
            get;
            set;
        }

        public object Value
        {
            get;
            set;
        }
        #endregion
    }
}
