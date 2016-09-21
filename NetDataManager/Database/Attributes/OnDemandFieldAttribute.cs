using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Database.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class OnDemandFieldAttribute : Attribute
    {
    }
}
