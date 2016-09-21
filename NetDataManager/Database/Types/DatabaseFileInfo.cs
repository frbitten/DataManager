using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Database.Types
{
    public class DatabaseFileInfo : DatabasePropertyInfo
    {
        #region [ Properties ]
        public Database.Attributes.FileAttribute Attribute
        {
            get;
            set;
        }
        #endregion
    }
}
