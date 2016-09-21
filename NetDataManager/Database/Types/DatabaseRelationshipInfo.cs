using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Database.Types
{
    public class DatabaseRelationshipInfo:DatabasePropertyInfo
    {
        #region [ Properties ]
        public Database.Attributes.RelationshipAttribute Attribute
        {
            get;
            set;
        }
        #endregion
    }
}
