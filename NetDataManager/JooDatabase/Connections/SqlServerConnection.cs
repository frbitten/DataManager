using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Reflection;
using System.Globalization;
using Joo.Database.Attributes;
using Joo.Database.Exceptions;
using Joo.Database.Structs;
using System.Data;
using System.Data.Common;
using System.IO;
using Joo.Database.Types;
using Joo.Utils.Helpers;

namespace Joo.Database.Connections
{
    internal class SqlServerConnection : DataBaseConnection
    {
        #region [ Atributes ]
        private SqlConnection connection;
        private SqlTransaction transaction;
        #endregion

        #region [ Public Methods ]
        public SqlServerConnection(string connectionString)
        {
            connection = new SqlConnection(connectionString);
            connection.Open();
            transaction = null;
        }

        public void Close()
        {
            connection.Close();
            cacheTransaction.Clear();
        }
        #endregion

        #region [ IDataBaseConnection Members ]

        public override int Duplicate<T>(int id)
        {
            throw new NotImplementedException("Implementar metodo");
        }
        public override void ExecuteScript(string script, string delimiter)
        {
            if (!string.IsNullOrEmpty(script))
            {
                SqlCommand command = new SqlCommand(script);
                command.Connection = connection;
                command.ExecuteNonQuery();
            }
        }

        public override DataTable ExecuteStoredProcedure(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                SqlCommand command = new SqlCommand(name);
                command.Connection = connection;
                command.CommandType = CommandType.StoredProcedure;
                SqlDataReader data = command.ExecuteReader();
                DataTable table = new DataTable();
                table.Load(data);
                table.TableName = name;
                return table;
            }
            throw new ArgumentException("name is empty");
        }
        public override DataTable ExecuteStoredProcedure(string name, IEnumerable<Parameter> parameters)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Name is empty");
            }
            if (parameters == null)
            {
                throw new ArgumentException("Parameters list is null");
            }

            SqlCommand command = new SqlCommand(name);
            command.Connection = connection;
            command.CommandType = CommandType.StoredProcedure;
            foreach (Parameter param in parameters)
            {
                if (string.IsNullOrEmpty(param.Name))
                {
                    throw new ArgumentException("Parameter invalid");
                }
                if (param.Value == null)
                {
                    throw new ArgumentException("Parameter invalid");
                }
                command.Parameters.Add(new SqlParameter(param.Name, param.Value));
            }
            SqlDataReader data = command.ExecuteReader();
            DataTable table = new DataTable();
            table.Load(data);
            table.TableName = name;
            return table;
        }

        protected override List<BasicModel> GetItemsProtected(Type type, Where where, OrderBy orderBy,int level,int start,int length)
        {
            Dictionary<int, BasicModel> ret = new Dictionary<int, BasicModel>();

            SqlCommand sql = new SqlCommand();
            sql.Connection = connection;
            DatabaseType databaseType = TypesManager.TypeOf(type);
            Select select = null;
            if (level == (int)LEVELS_OF_SEARCH.ON_DEMAND)
            {
                select = new Select(databaseType, true);
            }
            else
            {
                select = new Select(databaseType, false);
            }
            select.OrderBy = orderBy;
            if (where != null)
            {
                Dictionary<string, object> parameters = select.AddWhere(where, this);
                foreach (var item in parameters)
                {
                    SqlParameter parameter = new SqlParameter(item.Key, item.Value);
                    try
                    {
                        parameter.SqlDbType = ConvertToSqlDbType(item.Value.GetType());
                    }
                    catch (NotImplementedException err)
                    {
                        throw new ArgumentException("Where is not valid");
                    }
                    sql.Parameters.Add(parameter);
                }
            }
            try
            {
                sql.CommandText = select.ToString();

                SqlDataReader data = sql.ExecuteReader();
                if (data.HasRows)
                {
                    while (data.Read())
                    {
                        if (!ret.ContainsKey((int)data["ID"]))
                        {
                            BasicModel model = CreateModel(data, type, level <= (int)LEVELS_OF_SEARCH.ON_DEMAND);
                            ret.Add(model.ID, model);
                        }
                    }
                }
                data.Close();

                if (databaseType.DataBaseRelationship.Length > 0)
                {
                    ProcessRelationships(ret.Values, databaseType, level);
                }

                foreach (BasicModel item in ret.Values)
                {
                    item.Status = Status.Normal;
                    LoadFiles(item);
                    item.OnLoaded();
                }
            }
            catch (SqlException err)
            {
                throw err;
            }
            return new List<BasicModel>(ret.Values);
        }

        public override int GetLenghtItems(Type type, Where where)
        {
            int count = 0;
            SqlCommand sql = new SqlCommand();
            sql.Connection = connection;
            DatabaseType databaseType = TypesManager.TypeOf(type);
            Select select = new Select(databaseType, true);

            if (where != null)
            {
                Dictionary<string, object> parameters = select.AddWhere(where, this);
                foreach (var item in parameters)
                {
                    SqlParameter parameter = new SqlParameter(item.Key, item.Value);
                    try
                    {
                        parameter.SqlDbType = ConvertToSqlDbType(item.Value.GetType());
                    }
                    catch (NotImplementedException err)
                    {
                        throw new ArgumentException("Where is not valid");
                    }
                    sql.Parameters.Add(parameter);
                }
            }
            try
            {
                sql.CommandText = select.ToString();

                SqlDataReader data = sql.ExecuteReader();
                if (data.HasRows)
                {
                    while (data.Read())
                    {
                        count++;
                    }
                }
                data.Close();
            }
            catch (SqlException err)
            {
                throw err;
            }
            return count;
        }

        public override int GetLenghtItems<T>(Where where)
        {
            Type type = typeof(T);
            int count = 0;
            SqlCommand sql = new SqlCommand();
            sql.Connection = connection;
            DatabaseType databaseType = TypesManager.TypeOf(type);
            Select select = new Select(databaseType, true);

            if (where != null)
            {
                Dictionary<string, object> parameters = select.AddWhere(where, this);
                foreach (var item in parameters)
                {
                    SqlParameter parameter = new SqlParameter(item.Key, item.Value);
                    try
                    {
                        parameter.SqlDbType = ConvertToSqlDbType(item.Value.GetType());
                    }
                    catch (NotImplementedException err)
                    {
                        throw new ArgumentException("Where is not valid");
                    }
                    sql.Parameters.Add(parameter);
                }
            }
            try
            {
                sql.CommandText = select.ToString();

                SqlDataReader data = sql.ExecuteReader();
                if (data.HasRows)
                {
                    while (data.Read())
                    {
                        count++;
                    }
                }
                data.Close();
            }
            catch (SqlException err)
            {
                throw err;
            }
            return count;
        }

        public override void BeginTransaction()
        {
            transaction = connection.BeginTransaction();
        }
        public override void ComitTransaction()
        {
            transaction.Commit();
        }
        public override void RoolbackTransaction()
        {
            transaction.Rollback();
        }

        public override bool DeleteAllItems(DatabaseType objectType)
        {
            SqlCommand sql = new SqlCommand();
            sql.Connection = connection;
            sql.CommandText += "delete  from " + objectType.TableName;
            sql.CommandText += ";";

            int ret = sql.ExecuteNonQuery();
            if (ret <= 0)
            {
                return false;
            }
            return true;
        }

        public override bool DeleteAllItems<T>()
        {
            DatabaseType objectType = TypesManager.TypeOf(typeof(T));
            SqlCommand sql = new SqlCommand();
            sql.Connection = connection;
            sql.CommandText += "delete  from " + objectType.TableName;
            sql.CommandText += ";";

            int ret = sql.ExecuteNonQuery();
            if (ret <= 0)
            {
                return false;
            }
            return true;
        }

        [Obsolete("Use o metodo com generics")]
        public override bool CreateTable(DatabaseType objectType)
        {
            try
            {
                SqlCommand sql = new SqlCommand();
                sql.Connection = connection;
                sql.CommandText = "CREATE TABLE " + objectType.TableName + " ( ";

                foreach (var field in objectType.DataBaseProperties)
                {
                    sql.CommandText += field.Attribute.Name + " ";

                    sql.CommandText += " " + ConvertToSqlDbTypeString(field.Property.PropertyType, field.Attribute.Size) + " ";

                    if (field.Attribute.IsIdentity)
                    {
                        sql.CommandText += "IDENTITY(1,1) ";
                    }
                    if (field.Attribute.IsPrimaryKey)
                    {
                        sql.CommandText += "PRIMARY KEY CLUSTERED ";
                    }
                    else
                    {
                        if (field.Attribute.IsNull)
                        {
                            sql.CommandText += "NULL ";
                        }
                        else
                        {
                            sql.CommandText += "NOT NULL ";
                        }
                    }
                    sql.CommandText += ", ";
                }
                sql.CommandText = sql.CommandText.Remove(sql.CommandText.Length - 2, 2);
                sql.CommandText += " ) ";

                sql.ExecuteNonQuery();
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
#endif
                return false;
            }
            return true;
        }
        public override bool CreateTable<T>()
        {
            try
            {
                DatabaseType objectType = TypesManager.TypeOf(typeof(T));
                SqlCommand sql = new SqlCommand();
                sql.Connection = connection;
                sql.CommandText = "CREATE TABLE " + objectType.TableName + " ( ";

                foreach (var field in objectType.DataBaseProperties)
                {
                    sql.CommandText += field.Attribute.Name + " ";

                    sql.CommandText += " " + ConvertToSqlDbTypeString(field.Property.PropertyType, field.Attribute.Size) + " ";

                    if (field.Attribute.IsIdentity)
                    {
                        sql.CommandText += "IDENTITY(1,1) ";
                    }
                    if (field.Attribute.IsPrimaryKey)
                    {
                        sql.CommandText += "PRIMARY KEY CLUSTERED ";
                    }
                    else
                    {
                        if (field.Attribute.IsNull)
                        {
                            sql.CommandText += "NULL ";
                        }
                        else
                        {
                            sql.CommandText += "NOT NULL ";
                        }
                    }
                    sql.CommandText += ", ";
                }
                sql.CommandText = sql.CommandText.Remove(sql.CommandText.Length - 2, 2);
                sql.CommandText += " ) ";

                sql.ExecuteNonQuery();
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
#endif
                return false;
            }
            return true;
        }

        public override bool DropTable<T>()
        {
            Type objectType = typeof(T);
            if (objectType.IsSubclassOf(typeof(BasicModel)) == false)
            {
                throw new ArgumentException("Type invalid. Not extend class BasicModel");
            }
            try
            {
                string tableName = "";
                object[] attributes = objectType.GetCustomAttributes(false);
                foreach (object item in attributes)
                {
                    if (item is TableAttribute)
                    {
                        tableName = (item as TableAttribute).Name;
                        break;
                    }
                }

                SqlCommand sql = new SqlCommand();
                sql.Connection = connection;
                sql.CommandText = "DROP TABLE " + tableName + ";";
                sql.ExecuteNonQuery();
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
#endif
                return false;
            }
            return true;
        }

        public override DataTable ExecuteCustomSQL(string sql)
        {
            SqlCommand command = new SqlCommand(sql);
            command.Connection = connection;
            SqlDataReader data = command.ExecuteReader();
            DataTable table = new DataTable();
            table.Load(data);
            return table;
        }

        public override int ExecuteCustomSQLNotQuery(string sql)
        {
            SqlCommand command = new SqlCommand(sql);
            command.Connection = connection;
            int ret = command.ExecuteNonQuery();
            return ret;
        }
        public override DataTable ExecuteCustomSQL(DbCommand sql)
        {
            if (sql is SqlCommand)
            {
                SqlCommand command = sql as SqlCommand;
                command.Connection = connection;
                SqlDataReader data = command.ExecuteReader();
                DataTable table = new DataTable();
                table.Load(data);
                return table;
            }
            throw new ArgumentException("DbCommand invalid. Not is instance of SqlCommand.");
        }
        public override string ConvertOperatorToString(Operator op)
        {
            switch (op)
            {
                case Operator.AND:
                    return "and";
                case Operator.OR:
                    return "or";
                case Operator.OPEN_PARENTHESIS:
                    return "(";
                case Operator.CLOSE_PARENTHESIS:
                    return ")";
                case Operator.DIFFERENT:
                    return "<>";
                case Operator.EQUAL:
                    return "=";
                case Operator.LESS_EQUAL:
                    return "<=";
                case Operator.MINOR:
                    return "<";
                case Operator.MORE:
                    return ">";
                case Operator.MORE_EQUAL:
                    return ">=";
                case Operator.CONTAINS:
                    return "CHARINDEX";
                default:
                    throw new NotImplementedException("Operator not implemented");
            }
        }
        #endregion

        #region [ Private Methods ]

        protected override bool DeleteItem(BasicModel model)
        {
            DatabaseType type = model.GetDatabaseType();

            SqlCommand sql = new SqlCommand();
            if (transaction != null)
            {
                sql.Transaction = transaction;
            }
            sql.Connection = connection;
            sql.CommandText = "delete  from " + type.TableName + " where ";
            string where = "";
            foreach (var field in type.DataBaseProperties)
            {
                if (field.Attribute.IsPrimaryKey)
                {
                    where += field.Attribute.Name + "=@" + field.Attribute.Name + " and ";
                    //§ // SqlParameter parameter = new SqlParameter("@" + field.Attribute.Name, field.Property.GetValue(model, null));
                    SqlParameter parameter = new SqlParameter("@" + field.Attribute.Name, field.FastGetValue(model));
                    parameter.SqlDbType = ConvertToSqlDbType(field.Property.PropertyType);
                    sql.Parameters.Add(parameter);
                }
            }

            where = where.Remove(where.Length - 4, 4);
            sql.CommandText += where;
            sql.CommandText += ";";

            int ret = sql.ExecuteNonQuery();
            if (ret <= 0)
            {
                return false;
            }

            model.Status = Status.Invalid;
            DeleteFiles(model);
            bool otherRet = DeleteOneToManyRelationships(model);
            return otherRet;
        }

        protected override bool DeleteOneToManyRelationships(BasicModel model)
        {
            DatabaseType type = model.GetDatabaseType();
            var oneToMany = type.DataBaseRelationship.Where(obj => obj.Property.PropertyType.IsArray);

            bool ret = true;
            foreach (var item in oneToMany)
            {
                var objectsRelationShip = item.FastGetValue(model) as BasicModel[];


                if (objectsRelationShip.HasElements()) // Objeto já carregado
                {
                    foreach (var toRemove in objectsRelationShip)
                    {
                        if (toRemove.Status == Status.New || toRemove.Status == Status.Invalid)
                            continue;

                        ret = DeleteOneToManyRelationships(toRemove); // Antes de deletar remover recursivamente 

                        if (ret == false)
                            return false;

                    }

                    ret = DeleteItemsFromTheRelation(item, model.ID);

                    if (ret)
                        objectsRelationShip.Each(obj => Cache.CacheManager.Instance.Remove(item.ElementType, obj.ID));
                    else
                        return false;

                    Array newElements = Array.CreateInstance(item.ElementType, 0); // Se a relacao com 0 itens
                    item.FastSetValue(model, newElements);

                }
                else // Objeto não carregado
                {
                    int[] oneToManyResult;
                    var itemsRelationExists = ExistItemsInRelation(item, model.ID, out oneToManyResult);

                    if (itemsRelationExists == false) // Nenhum item para deletar
                        continue;


                    foreach (var toRemove in oneToManyResult) // Existe Items para deletar
                    { // Antes de deletar chamar recursivamente 

                        DeleteOneToManyRelationships(TypesManager.TypeOf(item.ElementType), toRemove);

                        if (ret == false)
                            return false;

                    }


                    ret = DeleteItemsFromTheRelation(item, model.ID);

                    if (ret)
                        oneToManyResult.Each(obj => Cache.CacheManager.Instance.Remove(item.ElementType, obj));
                    else
                        return false;
                }
            }

            return ret;
        }

        protected override bool DeleteOneToManyRelationships(DatabaseType type, int argsId)
        {
            var oneToMany = type.DataBaseRelationship.Where(obj => obj.Property.PropertyType.IsArray);

            bool ret = true;
            foreach (var item in oneToMany)
            {

                int[] oneToManyResult;
                var itemsRelationExists = ExistItemsInRelation(item, argsId, out oneToManyResult);

                if (itemsRelationExists == false) // Nenhum item para deletar
                    continue;

                foreach (var toRemove in oneToManyResult) // Existe items para deletar
                { // Antes de deletar chamar recursivamente 

                    DeleteOneToManyRelationships(TypesManager.TypeOf(item.ElementType), toRemove);

                    if (ret == false)
                        return false;


                }

                ret = DeleteItemsFromTheRelation(item, argsId);

                if (ret)
                    oneToManyResult.Each(obj => Cache.CacheManager.Instance.Remove(item.ElementType, obj));
                else
                    return false;
            }


            return ret;
        }

        protected override bool DeleteItemsFromTheRelation(DatabaseRelationshipInfo info, int argsId)
        {
            if (argsId <= 0)
                return true;

            var relationType = TypesManager.TypeOf(info.ElementType);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = connection;
            var fieldInClass = relationType.DataBaseProperties.First(obj => obj.Property.Name == info.Attribute.FieldName);

            if (info.Attribute.Where != null)
            {
                var myWhere = info.Attribute.Where.Clone();
                myWhere.AddItem(Operator.AND);

                myWhere.AddItemInfo(fieldInClass);
                myWhere.AddOperator(Operator.EQUAL);
                myWhere.AddItem(argsId);

                var delete = new Delete(relationType); ;
                var result = delete.AddWhere(myWhere, this);


                foreach (var item in result)
                {
                    SqlParameter param = new SqlParameter(item.Key, item.Value);
                    param.SqlDbType = ConvertToSqlDbType(item.Value.GetType());
                    cmd.Parameters.Add(param);

                }
                cmd.CommandText = delete.ToString();
            }
            else
            {
                cmd.CommandText += "DELETE FROM " + relationType.TableName + " WHERE " + fieldInClass.Attribute.Name + " = " + argsId + ";";
            }
            int ret = cmd.ExecuteNonQuery();
            if (ret <= 0)
            {
                return false;
            }
            return true;
        }


        protected override bool ExistItemsInRelation(DatabaseRelationshipInfo relation, int argsId, out int[] relationships)
        {
            if (argsId <= 0)
            {
                relationships = null;
                return false;
            }

            var relationType = TypesManager.TypeOf(relation.ElementType);
            DataTable tableRet = null;
            var fieldInClass = relationType.DataBaseProperties.First(obj => obj.Property.Name == relation.Attribute.FieldName);

            if (relation.Attribute.Where == null)
            {
                tableRet = this.ExecuteCustomSQL("SELECT " + relationType.TableName + ".id " + " FROM " + relationType.TableName + " WHERE " + fieldInClass.Attribute.Name + " = " + argsId);
            }
            else
            {
                var myWhere = relation.Attribute.Where.Clone();
                myWhere.AddItem(Operator.AND);

                myWhere.AddItemInfo(fieldInClass);
                myWhere.AddOperator(Operator.EQUAL);
                myWhere.AddItem(argsId);

                var select = new Select();
                select.AddField(relationType.TableName + ".id");
                var result = select.AddWhere(myWhere, this);

                SqlCommand cmd = new SqlCommand();
                cmd.Connection = this.connection;

                foreach (var item in result)
                {
                    SqlParameter param = new SqlParameter(item.Key, item.Value);
                    param.SqlDbType = ConvertToSqlDbType(item.Value.GetType());
                    cmd.Parameters.Add(param);

                }
                cmd.CommandText = select.ToString();
                tableRet = new System.Data.DataTable();
                SqlDataReader data = cmd.ExecuteReader();
                tableRet.Load(data);
                data.Close();
            }


            if (tableRet.Rows.HasElements())
            {
                relationships = tableRet.AsEnumerable().Select(obj => (int)obj[0]).ToArray();
                return true;
            }

            relationships = null;
            return false;
        }


        protected override bool UpdateItem(BasicModel model)
        {
            DatabaseType type = model.GetDatabaseType();

            SqlCommand sql = new SqlCommand();
            sql.Connection = connection;
            sql.CommandText = "update " + type.TableName + " set ";

            string where = "";
            foreach (var field in type.DataBaseProperties)
            {
                if (field.IsOnDemandField && !model.IsFull)
                {
                    continue;
                }
                if (field.Property.PropertyType == typeof(String) && field.Attribute.Size > 0)
                {
                    // § // string value = field.Property.GetValue(model, null) as string;
                    string value = field.FastGetValue(model) as string;
                    if (value.Length > field.Attribute.Size)
                    {
                        // § // field.Property.SetValue(model, value.Substring(0, (int)field.Attribute.Size), null);
                        field.FastSetValue(model, value.Substring(0, (int)field.Attribute.Size));

                    }
                }
                if (!String.IsNullOrEmpty(field.Attribute.Name) && !field.Attribute.IsIdentity && !field.Attribute.IsPrimaryKey)
                {
                    sql.CommandText += "`" + field.Attribute.Name + "`" + "=@" + field.Attribute.Name + ", ";
                    // § //SqlParameter parameter = new SqlParameter("@" + field.Attribute.Name, field.Property.GetValue(model, null));

                    SqlParameter parameter = new SqlParameter("@" + field.Attribute.Name, field.FastGetValue(model));
                    parameter.SqlDbType = ConvertToSqlDbType(field.Property.PropertyType);
                    sql.Parameters.Add(parameter);
                }
                if (field.Attribute.IsPrimaryKey)
                {
                    where += "`" + field.Attribute.Name + "`" + "=@" + field.Attribute.Name + " and ";

                    // § // SqlParameter parameter = new SqlParameter("@" + field.Attribute.Name, field.Property.GetValue(model, null));
                    SqlParameter parameter = new SqlParameter("@" + field.Attribute.Name, field.FastGetValue(model));

                    parameter.SqlDbType = ConvertToSqlDbType(field.Property.PropertyType);
                    sql.Parameters.Add(parameter);
                }
            }
            if (String.IsNullOrEmpty(where))
            {
                throw new FieldException("Not found primary key value");
            }
            sql.CommandText = sql.CommandText.Remove(sql.CommandText.Length - 2, 2);
            where = where.Remove(where.Length - 4, 4);
            sql.CommandText += " where " + where;
            sql.CommandText += ";";

            int ret = sql.ExecuteNonQuery();
            if (ret <= 0)
            {
                return false;
            }

            model.Status = Status.Normal;
            SaveFiles(model);
            return true;
        }

        public override bool InsertIgnoringIdentify(BasicModel model)
        {
            DatabaseType type = model.GetDatabaseType();

            Dictionary<string, object> values = new Dictionary<string, object>();
            Dictionary<string, Type> types = new Dictionary<string, Type>();
            DatabaseFieldInfo identity = null;
            foreach (var field in type.DataBaseProperties)
            {
                if (field.Property.PropertyType == typeof(String) && field.Attribute.Size > 0)
                {
                    // § // string value = field.Property.GetValue(model, null) as string;
                    string value = field.FastGetValue(model) as string;
                    if (value.Length > field.Attribute.Size)
                    {
                        // § // field.Property.SetValue(model, value.Substring(0, (int)field.Attribute.Size), null);
                        field.FastSetValue(model, value.Substring(0, (int)field.Attribute.Size));
                    }
                }
                if (!String.IsNullOrEmpty(field.Attribute.Name) /*&& !field.Attribute.IsIdentity*/)
                {
                    var realValue = field.FastGetValue(model);
                    if (realValue != null)
                    {
                        values.Add(field.Attribute.Name, realValue);
                        types.Add(field.Attribute.Name, field.Property.PropertyType);
                    }
                    else
                    {
                        throw new ArgumentException("Value is null in Object: " + field.Property.DeclaringType + " - PropertyName: " + field.Property.Name + " - FieldType: " + field.ElementType);
                    }
                }
                if (field.Attribute.IsIdentity)
                {
                    identity = field;
                }
            }
            if (values.Count == 0)
            {
                throw new TableException("Table invalid format. Requires at least one field beyond the ID");
            }

            SqlCommand sql = new SqlCommand();
            sql.Connection = connection;

            string fields = "";
            string tagValues = "";
            foreach (var item in values)
            {
                fields += item.Key + ", ";
                tagValues += "@" + item.Key + ", ";
                SqlParameter parameter = new SqlParameter("@" + item.Key, item.Value);
                parameter.SqlDbType = ConvertToSqlDbType(types[item.Key]);
                sql.Parameters.Add(parameter);
            }
            fields = fields.Remove(fields.Length - 2, 2);
            tagValues = tagValues.Remove(tagValues.Length - 2, 2);

            sql.CommandText = "insert into  " + type.TableName + " (" + fields + ") values (" + tagValues + ");";

            int ret = sql.ExecuteNonQuery();
            if (ret <= 0)
            {
                return false;
            }
            model.Status = Status.Normal;
            return true;
        }

        protected override bool InsertItem(BasicModel model)
        {
            DatabaseType type = model.GetDatabaseType();

            Dictionary<string, object> values = new Dictionary<string, object>();
            Dictionary<string, Type> types = new Dictionary<string, Type>();
            DatabaseFieldInfo identity = null;
            foreach (var field in type.DataBaseProperties)
            {
                if (field.Property.PropertyType == typeof(String) && field.Attribute.Size > 0)
                {
                    // § // string value = field.Property.GetValue(model, null) as string;
                    string value = field.FastGetValue(model) as string;
                    if (value.Length > field.Attribute.Size)
                    {
                        // § // field.Property.SetValue(model, value.Substring(0, (int)field.Attribute.Size), null);
                        field.FastSetValue(model, value.Substring(0, (int)field.Attribute.Size));
                    }
                }
                if (!String.IsNullOrEmpty(field.Attribute.Name) && !field.Attribute.IsIdentity)
                {
                    var realValue = field.FastGetValue(model);
                    if (realValue != null)
                    {
                        values.Add(field.Attribute.Name, realValue);
                        types.Add(field.Attribute.Name, field.Property.PropertyType);
                    }
                    else
                    {
                        throw new ArgumentException("Value is null in Object: " + field.Property.DeclaringType + " - PropertyName: " + field.Property.Name + " - FieldType: " + field.ElementType);
                    }
                }
                if (field.Attribute.IsIdentity)
                {
                    identity = field;
                }
            }
            if (values.Count == 0)
            {
                throw new TableException("Table invalid format. Requires at least one field beyond the ID");
            }

            SqlCommand sql = new SqlCommand();
            sql.Connection = connection;

            string fields = "";
            string tagValues = "";
            foreach (var item in values)
            {
                fields += item.Key + ", ";
                tagValues += "@" + item.Key + ", ";
                SqlParameter parameter = new SqlParameter("@" + item.Key, item.Value);
                parameter.SqlDbType = ConvertToSqlDbType(types[item.Key]);
                sql.Parameters.Add(parameter);
            }
            fields = fields.Remove(fields.Length - 2, 2);
            tagValues = tagValues.Remove(tagValues.Length - 2, 2);

            sql.CommandText = "insert into  " + type.TableName + " (" + fields + ") values (" + tagValues + ");";

            int ret = sql.ExecuteNonQuery();
            if (ret <= 0)
            {
                return false;
            }
            if (identity != null)
            {
                sql = new SqlCommand("select @@identity as id from " + type.TableName + ";");
                sql.Connection = connection;
                SqlDataReader data = sql.ExecuteReader();
                if (data.Read())
                {
                    identity.FastSetValue(model, Convert.ToInt32(data["id"]));
                    data.Close();
                }
                else
                {
                    data.Close();
                    return false;
                }
            }
            model.Status = Status.Normal;
            SaveFiles(model);
            return true;
        }

        private System.Data.SqlDbType ConvertToSqlDbType(Type type)
        {
            if (type == typeof(int))
            {
                return SqlDbType.Int;
            }
            if (type == typeof(Int16))
            {
                return SqlDbType.SmallInt;
            }
            if (type == typeof(Int64))
            {
                return SqlDbType.BigInt;
            }

            if (type == typeof(decimal))
            {
                return SqlDbType.Decimal;
            }

            if (type == typeof(double))
            {
                return SqlDbType.Float;
            }

            if (type == typeof(string))
            {
                return SqlDbType.VarChar;
            }
            if (type == typeof(DateTime))
            {
                return SqlDbType.DateTime;
            }
            if (type == typeof(byte[]))
            {
                return SqlDbType.Binary;
            }
            if (type == typeof(byte))
            {
                return System.Data.SqlDbType.TinyInt;
            }
            if (type == typeof(bool))
            {
                return System.Data.SqlDbType.Bit;
            }
            if (type == typeof(double))
            {
                return System.Data.SqlDbType.Float;
            }

            throw new NotImplementedException();
        }

        private string ConvertToSqlDbTypeString(Type type, double size)
        {
            System.Data.SqlDbType dbType = ConvertToSqlDbType(type);

            switch (dbType)
            {
                case SqlDbType.Int:
                    return "int";
                case SqlDbType.SmallInt:
                    return "smallint";
                case SqlDbType.BigInt:
                    return "bigInt";
                case SqlDbType.VarChar:
                    if (size <= 0)
                    {
                        return "text";
                    }
                    else
                    {
                        return "varchar(" + size + ")";
                    }
                case SqlDbType.Decimal:
                    {
                        NumberFormatInfo numberInfo = new NumberFormatInfo();
                        numberInfo.NumberDecimalSeparator = ",";
                        string aux = "10,0";
                        if (size > 0)
                        {
                            aux = size.ToString("N1", numberInfo);
                        }
                        return "decimal(" + aux + ")";
                    }
                case SqlDbType.Float:
                    {
                        NumberFormatInfo numberInfo = new NumberFormatInfo();
                        numberInfo.NumberDecimalSeparator = ",";
                        string aux = "10,2";
                        if (size > 0)
                        {
                            aux = size.ToString("N1", numberInfo);
                        }
                        return "float(" + aux + ")";
                    }
                case SqlDbType.DateTime:
                    return "DATETIME";
                case SqlDbType.Binary:
                    return "BINARY";
                case SqlDbType.TinyInt:
                    return "tinyint";
                case SqlDbType.Bit:
                    return "bit";
            }
            throw new NotImplementedException();
        }
        #endregion
    }
}
