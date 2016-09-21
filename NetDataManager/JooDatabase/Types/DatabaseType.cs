using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.ObjectModel;
using Joo.Utils;
using Joo.Database.Attributes;
using Joo.Database.Exceptions;
using System.Linq.Expressions;

namespace Joo.Database.Types
{
    public class DatabaseType
    {
        #region [ Fields ]
        private Dictionary<string, DatabaseFieldInfo> databaseProperties;
        private Dictionary<string, DatabaseRelationshipInfo> databaseRelationship;
        private Dictionary<string, DatabaseFileInfo> databaseFiles;
        #endregion

        #region [ Contructors ]
        /// <summary>
        /// Não chamar o constructor diretamente, usar o TypesManager por causa do Cache de Types
        /// </summary>
        /// <param name="typeModel"></param>
        internal DatabaseType(Type typeModel)
        {
            if (!typeModel.IsSubclassOf(typeof(BasicModel)))
            {
                throw new ArgumentException("Type " + typeModel.Name + " not extend class BasicModel");
            }

            this.Type = typeModel;
            databaseProperties = new Dictionary<string, DatabaseFieldInfo>();
            databaseRelationship = new Dictionary<string, DatabaseRelationshipInfo>();
            databaseFiles = new Dictionary<string, DatabaseFileInfo>();
            object[] attributes = this.Type.GetCustomAttributes(true);

            SaveInCache = attributes.FirstOrDefault(obj => obj is DontCacheAttribute) == null;
            TableAttribute attribute=attributes.FirstOrDefault(obj => obj is TableAttribute) as TableAttribute;
            if(attribute==null)
            {
                throw new TableException("Table name not found in " + this.Name + ".");
            }
            TableName = attribute.Name;
            if (string.IsNullOrEmpty(this.TableName))
            {
                throw new TableException("Table name not found in " + this.Name + ".");
            }

            PropertyInfo[] infos = this.Type.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            bool isOnDemandField = false;
            foreach (PropertyInfo info in infos)
            {
                isOnDemandField = false;
                attributes = info.GetCustomAttributes(true);
                DatabasePropertyInfo propertyInfo = null;
                foreach (object item in attributes)
                {
                    if (item is FieldAttribute)
                    {
                        DatabaseFieldInfo databaseInfo = new DatabaseFieldInfo();
                        databaseInfo.TableName = this.TableName;
                        databaseInfo.Attribute = item as FieldAttribute;
                        databaseInfo.Property = info;
                        propertyInfo = databaseInfo;
                        propertyInfo.IsOnDemandField = isOnDemandField;
                        databaseProperties.Add(databaseInfo.Attribute.Name.ToLower(), databaseInfo);
                    }

                    if (item is FileAttribute)
                    {
                        DatabaseFileInfo databaseInfo = new DatabaseFileInfo();
                        databaseInfo.TableName = this.TableName;
                        databaseInfo.Attribute = item as FileAttribute;
                        databaseInfo.Property = info;
                        propertyInfo = databaseInfo;
                        propertyInfo.IsOnDemandField = isOnDemandField;
                        databaseFiles.Add(databaseInfo.Attribute.Name.ToLower(), databaseInfo);
                    }
                    if (item is RelationshipAttribute)
                    {
                        Type element;
                        if (info.PropertyType.IsArray)
                        {
                            element = info.PropertyType.GetElementType();
                        }
                        else
                        {
                            element = info.PropertyType;
                        }
                        if (element.IsSubclassOf(typeof(BasicModel)))
                        {
                            DatabaseRelationshipInfo databaseInfo = new DatabaseRelationshipInfo();
                            databaseInfo.TableName = this.TableName;
                            databaseInfo.Attribute = item as RelationshipAttribute;
                            databaseInfo.Property = info;
                            propertyInfo = databaseInfo;
                            propertyInfo.IsOnDemandField = isOnDemandField;
                            databaseRelationship.Add(info.Name.ToLower(), databaseInfo);
                        }
                        else
                        {
                            throw new RelationshipException("Invalid use of RelationshipAttribute.Property has to be the daughter of BasicModel.");
                        }
                    }
                    if (item is OnDemandFieldAttribute)
                    {
                        isOnDemandField = true;
                        if (propertyInfo != null)
                        {
                            propertyInfo.IsOnDemandField = true;
                        }
                    }
                }
            }
            infos = this.Type.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo info in infos)
            {
                isOnDemandField = false;
                attributes = info.GetCustomAttributes(true);
                DatabasePropertyInfo propertyInfo = null;
                foreach (object item in attributes)
                {
                    if (item is FieldAttribute)
                    {
                        if (databaseProperties.ContainsKey((item as FieldAttribute).Name.ToLower()))
                        {
                            break;
                        }
                        DatabaseFieldInfo databaseInfo = new DatabaseFieldInfo();
                        databaseInfo.TableName = this.TableName;
                        databaseInfo.Attribute = item as FieldAttribute;
                        databaseInfo.Property = info;
                        propertyInfo = databaseInfo;
                        propertyInfo.IsOnDemandField = isOnDemandField;
                        databaseProperties.Add(databaseInfo.Attribute.Name.ToLower(), databaseInfo);
                        break;
                    }

                    if (item is FileAttribute)
                    {
                        if (databaseProperties.ContainsKey((item as FileAttribute).Name.ToLower()))
                        {
                            break;
                        }
                        DatabaseFileInfo databaseInfo = new DatabaseFileInfo();
                        databaseInfo.TableName = this.TableName;
                        databaseInfo.Attribute = item as FileAttribute;
                        databaseInfo.Property = info;
                        propertyInfo = databaseInfo;
                        propertyInfo.IsOnDemandField = isOnDemandField;
                        databaseFiles.Add(databaseInfo.Attribute.Name.ToLower(), databaseInfo);
                    }

                    if (item is RelationshipAttribute)
                    {
                        if (databaseRelationship.ContainsKey(info.Name.ToLower()))
                        {
                            break;
                        }
                        Type element;
                        if (info.PropertyType.IsArray)
                        {
                            element = info.PropertyType.GetElementType();
                        }
                        else
                        {
                            element = info.PropertyType;
                        }
                        if (element.IsSubclassOf(typeof(BasicModel)))
                        {
                            DatabaseRelationshipInfo databaseInfo = new DatabaseRelationshipInfo();
                            databaseInfo.TableName = this.TableName;
                            databaseInfo.Attribute = item as RelationshipAttribute;
                            databaseInfo.Property = info;
                            propertyInfo = databaseInfo;
                            propertyInfo.IsOnDemandField = isOnDemandField;
                            databaseRelationship.Add(info.Name.ToLower(), databaseInfo);
                        }
                        else
                        {
                            throw new RelationshipException("Invalid use of RelationshipAttribute.Property has to be the daughter of BasicModel.");
                        }
                        break;
                    }
                    if (item is OnDemandFieldAttribute)
                    {
                        isOnDemandField = true;
                        if (propertyInfo != null)
                        {
                            propertyInfo.IsOnDemandField = true;
                        }
                    }
                }
            }
            this.DataBaseProperties = databaseProperties.Values.ToArray();
            this.DataBaseRelationship = databaseRelationship.Values.ToArray();
            this.DataBaseFiles = databaseFiles.Values.ToArray();
            this.DataBaseStringProperties = this.DataBaseProperties.Where(obj => obj.Property.PropertyType == typeof(string)).ToArray();
        }
        #endregion

        #region [ Properties ]
        public DatabaseFieldInfo[] DataBaseProperties
        {
            get;
            protected set;
        }

        public DatabaseFileInfo[] DataBaseFiles
        {
            get;
            protected set;
        }

        public DatabaseRelationshipInfo[] DataBaseRelationship
        {
            get;
            protected set;
        }

        public DatabaseFieldInfo[] DataBaseStringProperties
        {
            get;
            protected set;
        }

        public String TableName
        {
            get;
            protected set;
        }
        public Type Type
        {
            get;
            protected set;
        }
        public string Name
        {
            get
            {
                return Type.Name;
            }
        }
        public bool SaveInCache
        {
            get;
            protected set;
        }
        #endregion

        #region [ Public methods]
        public DatabasePropertyInfo GetPropertyInfo(string name)
        {
            name = name.ToLower();
            if (databaseProperties.ContainsKey(name))
            {
                return databaseProperties[name];
            }
            if (databaseRelationship.ContainsKey(name))
            {
                return databaseRelationship[name];
            }
            return null;
        }

        public bool ContainsProperty(string name)
        {
            if (databaseProperties.ContainsKey(name.ToLower()))
            {
                return true;
            }
            if (databaseRelationship.ContainsKey(name.ToLower()))
            {
                return true;
            }
            return false;
        }
        #endregion

        #region [ Static Helper Methods ]
        public static Func<object, object> BuildGetAccessor(MethodInfo method)
        {
            var obj = Expression.Parameter(typeof(object), "o");

            Expression<Func<object, object>> expr =
                Expression.Lambda<Func<object, object>>(
                    Expression.Convert(
                        Expression.Call(
                            Expression.Convert(obj, method.DeclaringType),
                            method),
                        typeof(object)),
                    obj);

            return expr.Compile();
        }

        public static Action<object, object> BuildSetAccessor(MethodInfo method)
        {
            var obj = Expression.Parameter(typeof(object), "o");
            var value = Expression.Parameter(typeof(object));

            Expression<Action<object, object>> expr =
                Expression.Lambda<Action<object, object>>(
                    Expression.Call(
                        Expression.Convert(obj, method.DeclaringType),
                        method,
                        Expression.Convert(value, method.GetParameters()[0].ParameterType)),
                    obj,
                    value);

            return expr.Compile();
        }
        #endregion

    }
}
