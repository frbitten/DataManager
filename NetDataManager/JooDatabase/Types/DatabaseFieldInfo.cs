using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Joo.Database.Types
{
    [Serializable]
    public class DatabaseFieldInfo:DatabasePropertyInfo
    {
        #region [ Properties ]
        public Joo.Database.Attributes.FieldAttribute Attribute
        {
            get;
            set;
        }
        #endregion
    }
}
