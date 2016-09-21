using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Joo.Database.Structs;
using System.Reflection;
using System.Linq.Expressions;

namespace Joo.Database.Attributes
{
    public class RelationshipAttribute : Attribute
    {
        public RelationshipAttribute(string fieldName)
        {
            this.FieldName = fieldName;
            this.Where = null;
            //this.FieldName2 = string.Empty;
            //this.AuxiliaryType = null;
        }

        public RelationshipAttribute(string fieldName, object[] where)
        {
            this.FieldName = fieldName;
            this.Where = new Where();
            for (int i = 0; i < where.Length; i++)
            {
                if (where[i] is Type)
                {
                    if (where[i + 1] is String)
                    {
                        this.Where.AddItem((Type)where[i], (string)where[i + 1]);
                        i++;
                        continue;
                    }
                    else
                    {
                        throw new ArgumentException("Um valor Type sempre tem que ser seguido do nome da propriedade em string no array de where");
                    }
                }
                if (where[i] is Joo.Database.Types.DatabasePropertyInfo)
                {
                    this.Where.AddItemInfo((Joo.Database.Types.DatabasePropertyInfo)where[i]);
                    continue;
                }

                if (where[i] is Operator)
                {
                    this.Where.AddOperator((Operator)where[i]);
                    continue;
                }
                this.Where.AddItem(where[i]);
            }

            //this.FieldName2 = string.Empty;
            //this.AuxiliaryType = null;
        }
        //public RelationshipAttribute(string fieldName, Type auxType, string fieldName2)
        //{
        //    this.FieldName = fieldName;
        //    this.FieldName2 = fieldName2;
        //    this.AuxiliaryType = auxType;
        //}
        public string FieldName
        {
            get;
            set;
        }
        //public string FieldName2
        //{
        //    get;
        //    set;
        //}
        //public Type AuxiliaryType
        //{
        //    get;
        //    set;
        //}
        public Where Where
        {
            get;
            set;
        }

        //public bool IsManyToMany
        //{
        //    get
        //    {
        //        return !string.IsNullOrEmpty(this.FieldName2) && AuxiliaryType != null;
        //    }
        //}

        //public bool IsOneToMany
        //{
        //    get
        //    {
        //        return string.IsNullOrEmpty(this.FieldName2) || AuxiliaryType == null;
        //    }
        //}
    }
}
