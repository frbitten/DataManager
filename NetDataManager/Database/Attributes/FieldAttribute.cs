using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Database.Exceptions;

namespace Database.Attributes
{
    public enum FieldType
    {
        /* binary values
         * PRIMARY  101
         * IDENTITY 011 
         * NOT NULL 001
         * NULL     000
         * */

        PRIMARY_KEY=5,
        IDENTITY=3,
        NOT_NULL=1,
        NULL=0
        
    }

    [AttributeUsage(AttributeTargets.Property)]
    [Serializable]
    public class FieldAttribute:Attribute
    {
        private FieldType fieldType;

        public FieldAttribute(String name, double size)
        {
            Name = name;
            Size = size;
            fieldType = FieldType.NOT_NULL;
        }
        
        public FieldAttribute(String name, double size, FieldType fieldType)
        {
            Name = name;
            Size = size;
            this.fieldType = fieldType;
        }
        public FieldAttribute(String name)
        {
            Name = name;
            Size = 0;
            fieldType = FieldType.NOT_NULL;
        }
        
        public FieldAttribute(String name, FieldType fieldType)
        {
            Name = name;
            Size = -1;
            this.fieldType = fieldType;
        }
        
        public string Name
        {
            get;
            set;
        }
       
        public double Size
        {
            get;
            set;
        }

        public bool IsIdentity
        {
            get
            {
                return (fieldType & FieldType.IDENTITY) == FieldType.IDENTITY;
            }
        }
        public bool IsPrimaryKey
        {
            get
            {
                return (fieldType & FieldType.PRIMARY_KEY) == FieldType.PRIMARY_KEY;
            }
        }
        public bool IsNull
        {
            get
            {
                return (fieldType | FieldType.NULL) == FieldType.NULL;
            }
        }
    }
}
