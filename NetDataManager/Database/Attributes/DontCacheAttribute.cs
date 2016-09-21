using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Database.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class DontCacheAttribute:Attribute
    {
    }
}
