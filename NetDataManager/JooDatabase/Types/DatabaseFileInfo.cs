using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Joo.Database.Types
{
    public class DatabaseFileInfo : DatabasePropertyInfo
    {
        #region [ Properties ]
        public Joo.Database.Attributes.FileAttribute Attribute
        {
            get;
            set;
        }
        #endregion
    }
}
