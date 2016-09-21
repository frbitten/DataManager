using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Joo.Database.Attributes;

namespace Joo.Database.Types
{
    [Serializable]
    public abstract class DatabasePropertyInfo
    {
        #region [ Static ]
        protected static Dictionary<PropertyInfo, Func<object, object>> GetPropertyDelegateDictionary = new Dictionary<PropertyInfo, Func<object, object>>();
        protected static Dictionary<PropertyInfo, Action<object, object>> SetPropertyDelegateDictionary = new Dictionary<PropertyInfo, Action<object, object>>();
        protected static List<PropertyInfo> PrivateGetterMember = new List<PropertyInfo>();
        protected static List<PropertyInfo> PrivateSetterMember = new List<PropertyInfo>();
        #endregion

        #region [ Fields ]
        private PropertyInfo propertyInfo;
        private Type propertyType;
        #endregion

        #region [ Constructors ]
        public DatabasePropertyInfo()
        {
            IsOnDemandField = false;
            Property = null;
        }
        #endregion

        #region [ Properties ]
        public PropertyInfo Property
        {
            get
            {
                return this.propertyInfo;
            }
            set
            {
                if (value != null)
                {
                    if (value.PropertyType.IsArray)
                        this.propertyType = value.PropertyType.GetElementType();
                    else
                        this.propertyType = value.PropertyType;

                    var attributes = value.GetCustomAttributes(true);

                    var attrib = attributes.AsEnumerable().FirstOrDefault(obj => obj is FieldAttribute);
                    if (attrib == null)
                    {
                        this.IsFieldAttribute = false;
                    }
                    else
                    {
                        this.IsFieldAttribute = true;
                    }
            
                }

                this.propertyInfo = value;

            }
        }

        public Type ElementType
        {
            get
            {

                return this.propertyType;

            }
        }

        public string TableName
        {
            get;
            set;
        }

        public bool IsFieldAttribute 
        { 
            get;
            set; 
        }

        public bool IsOnDemandField
        {
            get;
            set;
        }
        #endregion

        #region [ Methods ]
        public object FastGetValue(object source)
        {
            if (GetPropertyDelegateDictionary.ContainsKey(propertyInfo))
            {
                var delegateToGet = GetPropertyDelegateDictionary[propertyInfo];
                return delegateToGet(source); // Executa o Método de Get
            }
            else
            {
                if (PrivateGetterMember.Contains(propertyInfo))
                {
                    return this.propertyInfo.GetValue(source, null);
                }

                var methodGet = propertyInfo.GetGetMethod();
                if (methodGet == null)   // Propriedade com Set Privado
                {
                    PrivateGetterMember.Add(propertyInfo);
                    return this.propertyInfo.GetValue(source, null);
                }

                var delegateGet = DatabaseType.BuildGetAccessor(methodGet);
                GetPropertyDelegateDictionary.Add(Property, delegateGet);
                return delegateGet(source); // Executa o Método de Get
            }

        }

        public void FastSetValue(object source, object anyValue)
        {
            if (SetPropertyDelegateDictionary.ContainsKey(propertyInfo))
            {
                var delegateToSet = SetPropertyDelegateDictionary[propertyInfo];
                delegateToSet(source, anyValue); // Executa o Método de Set
            }
            else
            {
                if (PrivateSetterMember.Contains(propertyInfo))
                {
                    this.propertyInfo.SetValue(source, anyValue, null);
                    return;
                }

                var methodSet = propertyInfo.GetSetMethod();

                if (methodSet == null) // Propriedade com Get Privado
                {
                    this.propertyInfo.SetValue(source, anyValue, null);
                    PrivateSetterMember.Add(propertyInfo);
                    return;
                }

                var delegateSet = DatabaseType.BuildSetAccessor(methodSet);
                SetPropertyDelegateDictionary.Add(propertyInfo, delegateSet);
                delegateSet(source, anyValue); // Executa o Método de Set
            }
        }
        #endregion

        #region [ Override ]
        public override string ToString()
        {
            return ElementType.ReflectedType.Name + "." + Property.Name;
        }
        #endregion

    }
}
