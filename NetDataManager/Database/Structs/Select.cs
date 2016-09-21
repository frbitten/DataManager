using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Database.Connections;
using System.Reflection;
using Database.Types;

namespace Database.Structs
{
    public class Select
    {
        private List<string> fields;
        private List<string> froms;
        private List<string> where;
        //private string groupBy;

        public Select()
        {
            fields = new List<string>();
            froms = new List<string>();
            where = new List<string>();
            //groupBy = string.Empty;
        }
        public Select(DatabaseType type,bool onDemand)
        {
            fields = new List<string>();
            froms = new List<string>();
            where = new List<string>();
            AddFrom(type.TableName);
            //groupBy = " group by " + type.TableName + ".id";
            OrderBy = new OrderBy();
            OrderBy.AddItem(type.Type, "ID");

            foreach (var field in type.DataBaseProperties)
            {
                if (onDemand)
                {
                    if (!field.IsOnDemandField)
                    {
                        AddField(type.TableName + "." + field.Attribute.Name);
                    }
                }
                else
                {
                    AddField(type.TableName + "." + field.Attribute.Name);
                }
            }
        }

        public override string ToString()
        {
            StringBuilder select = new StringBuilder();
            select.Append("select ");
            foreach (string item in fields)
            {
                select.Append(item);
                if (fields.IndexOf(item) != fields.Count - 1)
                {
                    select.Append(", ");
                }
            }

            select.Append(" from ");

            foreach (string item in froms)
            {
                select.Append(item);
                if (froms.IndexOf(item) != froms.Count - 1)
                {
                    select.Append(", ");
                }
            }

            if (where.Count > 0)
            {
                select.Append(" where ");
                foreach (string item in where)
                {
                    select.Append(item + " ");
                }
            }

            if (OrderBy!=null)
            {
                select.Append(" order by ");
                bool more = false;

                if (OrderBy.Items.Count > 0)
                {
                    DatabaseType type = TypesManager.TypeOf(OrderBy.Items[0][0] as Type);
                    foreach (Object[] item in OrderBy.Items)
                    {
                        if (more)
                        {
                            select.Append(", ");
                        }
                     
                        foreach (var property in type.DataBaseProperties)
                        {
                            if (property.Property.Name == item[1] as string)
                            {
                                select.Append(type.TableName + "." + property.Attribute.Name);
                                more = true;
                                break;
                            }
                        }
                    }
                }
            }
            
            if (LimitStart > 0 && LimitLength > 0)
            {
                select.Append(" LIMIT " + LimitStart + " , " + LimitLength + "");
            }
            return select.ToString();
        }

        public void AddField(string name)
        {
            foreach (string item in fields)
            {
                if (name == item)
                {
                    return;
                }
            }
            fields.Add(name);
        }

        private void AddWhere(string item)
        {
            where.Add(item);
        }

        public Dictionary<string,object> AddWhere(Where where,DataBaseConnection connection)
        {
            Dictionary<string, object> ret = new Dictionary<string, object>();
            int parameterCount = 1;
            for (int i = 0; i < where.Items.Count; i++)
            {
                object item = where.Items[i];
                // § //if (item is PropertyInfo)
                if (item is DatabasePropertyInfo)
                {
                    if (where.Items.Count > i + 1 && where.Items[i + 1] is Operator && ((Operator)where.Items[i + 1]) == Operator.CONTAINS)
                    {
                        continue;
                    }
                    // § // DatabaseType tempType = TypesManager.TypeOf((item as PropertyInfo).ReflectedType);
                    // § // DatabaseFieldInfo databaseInfo = tempType.GetPropertyInfo((item as PropertyInfo).Name) as DatabaseFieldInfo;

                    var dtInfo = (item as DatabaseFieldInfo);
                    AddFrom(dtInfo.TableName);
                    AddWhere(dtInfo.TableName + "." + dtInfo.Attribute.Name);
                }
                else
                {
                    if (item is Operator)
                    {
                        switch ((Operator)item)
                        {
                            case Operator.AND:
                            case Operator.OR:
                            case Operator.OPEN_PARENTHESIS:
                            case Operator.CLOSE_PARENTHESIS:
                            case Operator.DIFFERENT:
                            case Operator.EQUAL:
                            case Operator.LESS_EQUAL:
                            case Operator.MINOR:
                            case Operator.MORE:
                            case Operator.MORE_EQUAL:
                                AddWhere(connection.ConvertOperatorToString((Operator)item));
                                break;
                            case Operator.CONTAINS:
                                {
                                    if (!(where.Items[i-1] is DatabaseFieldInfo ))
                                    {
                                         throw new ArgumentException("Where is not valid. Contains syntax invalid");
                                    }

                                    if (!(where.Items[i + 1] is String) && !(where.Items[i + 1] is DatabaseFieldInfo))
                                    {
                                        throw new ArgumentException("Where is not valid. Contains syntax invalid");
                                    }
                                    String aux="";
                                    DatabaseFieldInfo info = (DatabaseFieldInfo)where.Items[i - 1];
                                    AddFrom(info.TableName);
                                    aux += info.TableName + "." + info.Attribute.Name;
                                
                                
                                    aux += " " + connection.ConvertOperatorToString((Operator)item) + " ";

                                    if (where.Items[i + 1] is DatabaseFieldInfo)
                                    {
                                        info = (DatabaseFieldInfo)where.Items[i + 1];
                                        AddFrom(info.TableName);
                                        aux += info.TableName + "." + info.Attribute.Name;
                                    }
                                    else
                                    {
                                        aux += "'%"+where.Items[i+1]+"%'";
                                    }                                
                                    AddWhere(aux);
                                    i++;
                                }
                                break;
                        }
                    }
                    else
                    {
                        AddWhere("@Parameter" + parameterCount);
                        ret.Add("@Parameter" + parameterCount, item);
                        parameterCount++;
                    }
                }
            }
            return ret;
        }

        public void AddFrom(string table)
        {
            foreach (string item in froms)
            {
                if (table == item)
                {
                    return;
                }
            }
            froms.Add(table);
        }

        public bool HasFrom()
        {
            return froms.Count > 0;
        }

        public bool hasField()
        {
            return fields.Count > 0;
        }

        public bool hasWhere()
        {
            return where.Count > 0;
        }

        #region [ Properties ]
        public OrderBy OrderBy
        {
            get;
            set;
        }
        public int LimitStart
        {
            get;
            set;
        }
        public int LimitLength
        {
            get;
            set;
        }
        #endregion
    }
}
