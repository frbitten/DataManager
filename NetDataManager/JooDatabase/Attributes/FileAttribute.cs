using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Joo.Database.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    [Serializable]
    public class FileAttribute:Attribute
    {
        private FieldType fieldType;

        public FileAttribute(String name, String extension)
        {
            Name = name;
            Extension = extension;
            fieldType = FieldType.NOT_NULL;
        }
        
        public string Name
        {
            get;
            set;
        }

        public string Extension
        {
            get;
            set;
        }
    }
}
