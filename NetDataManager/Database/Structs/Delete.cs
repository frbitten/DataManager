using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Database.Types;
using Database.Connections;
using System.Reflection;

namespace Database.Structs
{
    public class Delete
    {
        private List<string> froms;
        private List<string> where;
        private string deleteTableName;
        //private string groupBy;

        public Delete(DatabaseType type)
        {
            froms = new List<string>();
            where = new List<string>();

            this.deleteTableName = type.TableName;
            AddFrom(type.TableName);
        }

        private void AddFrom(string table)
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

        private void AddWhere(string item)
        {
            where.Add(item);
        }

        public Dictionary<string, object> AddWhere(Where where, DataBaseConnection connection)
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
                                    DatabaseFieldInfo databaseInfo = null;
                                    if (!(where.Items[i - 1] is DatabaseFieldInfo))
                                    {
                                        throw new ArgumentException("Where is not valid. Contains syntax invalid");
                                    }
                                    if (!(where.Items[i + 1] is String) && !(where.Items[i + 1] is PropertyInfo))
                                    {
                                        throw new ArgumentException("Where is not valid. Contains syntax invalid");
                                    }
                                    string aux = connection.ConvertOperatorToString((Operator)item) + "('";
                                    // § // if (where.Items[i + 1] is PropertyInfo)
                                    if (where.Items[i + 1] is DatabaseFieldInfo)
                                    {
                                        databaseInfo = where.Items[i + 1] as DatabaseFieldInfo;

                                        // § //tempType = TypesManager.TypeOf((where.Items[i + 1] as PropertyInfo).ReflectedType);
                                        // § // databaseInfo = tempType.GetPropertyInfo((where.Items[i + 1] as PropertyInfo).Name) as DatabaseFieldInfo;

                                        AddFrom(databaseInfo.TableName);
                                        aux += databaseInfo.TableName + "." + databaseInfo.Attribute.Name;
                                    }
                                    else
                                    {
                                        aux += where.Items[i + 1].ToString();
                                    }
                                    aux += "',";

                                    //tempType = TypesManager.TypeOf((where.Items[i - 1] as PropertyInfo).ReflectedType);
                                    //databaseInfo = tempType.GetPropertyInfo((where.Items[i - 1] as PropertyInfo).Name) as DatabaseFieldInfo;

                                    databaseInfo = (where.Items[i - 1] as DatabaseFieldInfo);
                                    AddFrom(databaseInfo.TableName);
                                    aux += databaseInfo.TableName + "." + databaseInfo.Attribute.Name;
                                    aux += ")>0";
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

        public override string ToString()
        {
            StringBuilder select = new StringBuilder();
            select.Append("delete ");
            select.Append(deleteTableName);
            select.Append(".*");


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

            return select.ToString();
        }
    }
}
