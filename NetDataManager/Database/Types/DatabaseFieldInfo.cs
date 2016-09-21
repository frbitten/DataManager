using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Database.Types
{
    [Serializable]
    public class DatabaseFieldInfo:DatabasePropertyInfo
    {
        #region [ Properties ]
        public Database.Attributes.FieldAttribute Attribute
        {
            get;
            set;
        }
        #endregion
    }
}
