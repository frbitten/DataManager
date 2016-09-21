using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Joo.Database.Types
{
    public class DatabaseRelationshipInfo:DatabasePropertyInfo
    {
        #region [ Properties ]
        public Joo.Database.Attributes.RelationshipAttribute Attribute
        {
            get;
            set;
        }
        #endregion
    }
}
