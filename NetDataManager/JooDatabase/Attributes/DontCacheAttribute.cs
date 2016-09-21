using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Joo.Database.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class DontCacheAttribute:Attribute
    {
    }
}
