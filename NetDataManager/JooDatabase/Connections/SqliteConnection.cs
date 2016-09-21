using System;
using System.Collections.Generic;
using Joo.Database.Attributes;
using System.Globalization;
using MySql.Data.MySqlClient;
using Joo.Database.Structs;
using Joo.Database.Exceptions;
using System.Data.Common;
using System.Data;
using Joo.Database.Types;
using System.Linq;
using Joo.Utils.Helpers;
using System.Data.SQLite;

namespace Joo.Database.Connections
{
    internal class SqliteConnection : DataBaseConnection
    {
        #region [ Atributes ]

        private SQLiteConnection connection;

        private SQLiteTransaction transaction;

        #endregion

        #region [ Public Methods ]

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteConnection"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public SqliteConnection(string connectionString)
        {
            connection = new SQLiteConnection(connectionString);
            connection.Open();
            transaction = null;
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public void Close()
        {
            connection.Close();
            cacheTransaction.Clear();
        }

        #endregion

        #region [ IDataBaseConnection Members ]
        protected override List<BasicModel> GetItemsProtected(Type type, Where where, OrderBy orderby, int level, int start, int length)
        {
            Dictionary<int, BasicModel> ret = new Dictionary<int, BasicModel>();

            SQLiteCommand sql = new SQLiteCommand();

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
            select.LimitStart = start;
            select.LimitLength = length;
            select.OrderBy = orderby;
            if (where != null)
            {
                Dictionary<string, object> parameters = select.AddWhere(where, this);
                foreach (var item in parameters)
                {
                    SQLiteParameter parameter = new SQLiteParameter(item.Key, convertToDbValue(item.Value));
                    try
                    {
                        parameter.DbType = ConvertToSqlDbType(item.Value.GetType());
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

                SQLiteDataReader data = sql.ExecuteReader();
                if (data.HasRows)
                {
                    while (data.Read())
                    {
                        if (!ret.ContainsKey(Convert.ToInt32((Int64)data["ID"])))
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
            catch (MySqlException err)
            {
                throw err;
            }
            return new List<BasicModel>(ret.Values);
        }

        public override int GetLenghtItems<T>(Where where)
        {
            Type type = typeof(T);
            int count = 0;
            SQLiteCommand sql = new SQLiteCommand();
            sql.Connection = connection;
            DatabaseType databaseType = TypesManager.TypeOf(type);
            Select select = select = new Select(databaseType, true);
            if (where != null)
            {
                Dictionary<string, object> parameters = select.AddWhere(where, this);
                foreach (var item in parameters)
                {
                    SQLiteParameter parameter = new SQLiteParameter(item.Key, convertToDbValue(item.Value));
                    try
                    {
                        parameter.DbType = ConvertToSqlDbType(item.Value.GetType());
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
                SQLiteDataReader data = sql.ExecuteReader();
                if (data.HasRows)
                {
                    while (data.Read())
                    {
                        count++;
                    }
                }
                data.Close();

            }
            catch (MySqlException err)
            {
                throw err;
            }
            return count;
        }

        [Obsolete("Usar metodo com generics")]
        public override int GetLenghtItems(Type type, Where where)
        {
            int count = 0;
            SQLiteCommand sql = new SQLiteCommand();
            sql.Connection = connection;
            DatabaseType databaseType = TypesManager.TypeOf(type);
            Select select = select = new Select(databaseType, true);
            if (where != null)
            {
                Dictionary<string, object> parameters = select.AddWhere(where, this);
                foreach (var item in parameters)
                {
                    SQLiteParameter parameter = new SQLiteParameter(item.Key, convertToDbValue(item.Value));
                    try
                    {
                        parameter.DbType = ConvertToSqlDbType(item.Value.GetType());
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
                SQLiteDataReader data = sql.ExecuteReader();
                if (data.HasRows)
                {
                    while (data.Read())
                    {
                        count++;
                    }
                }
                data.Close();

            }
            catch (MySqlException err)
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
            if (transaction != null)
            {
                transaction.Commit();
            }
        }

        public override void RoolbackTransaction()
        {
            if (transaction != null)
            {
                transaction.Rollback();
            }
        }

        [Obsolete("Use o metodo com generics")]
        public override bool DeleteAllItems(DatabaseType objectType)
        {
            SQLiteCommand sql = new SQLiteCommand();
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
            SQLiteCommand sql = new SQLiteCommand();
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
                SQLiteCommand sql = new SQLiteCommand();
                sql.Connection = connection;
                sql.CommandText = "CREATE TABLE `" + objectType.TableName + "` ( ";

                string aux = "";
                foreach (var field in objectType.DataBaseProperties)
                {
                    sql.CommandText += " `" + field.Attribute.Name + "` ";

                    sql.CommandText += " " + ConvertToSqlDbTypeString(field.Property.PropertyType, field.Attribute.Size) + " ";

                    if (field.Attribute.IsNull)
                    {
                        sql.CommandText += " NULL ";
                    }
                    else
                    {
                        sql.CommandText += " NOT NULL ";
                    }

                    if (field.Attribute.IsIdentity)
                    {
                        sql.CommandText += "AUTO_INCREMENT ";
                    }
                    if (field.Attribute.IsPrimaryKey)
                    {
                        aux += " PRIMARY KEY(" + field.Attribute.Name + ")";
                    }
                    sql.CommandText += ", ";

                }
                if (string.IsNullOrEmpty(aux))
                {
                    sql.CommandText = sql.CommandText.Remove(sql.CommandText.Length - 2, 2);
                }
                else
                {
                    sql.CommandText += aux;
                }
                sql.CommandText += " ) ENGINE=InnoDB DEFAULT CHARSET=utf8 AUTO_INCREMENT=1 ";

                sql.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        public override bool CreateTable<T>()
        {
            try
            {
                DatabaseType objectType = TypesManager.TypeOf(typeof(T));
                SQLiteCommand sql = new SQLiteCommand();
                sql.Connection = connection;
                sql.CommandText = "CREATE TABLE `" + objectType.TableName + "` ( ";

                foreach (var field in objectType.DataBaseProperties)
                {
                    sql.CommandText += " `" + field.Attribute.Name + "` ";

                    sql.CommandText += " " + ConvertToSqlDbTypeString(field.Property.PropertyType, field.Attribute.Size) + " ";

                    if (field.Attribute.IsNull)
                    {
                        sql.CommandText += " NULL ";
                    }
                    else
                    {
                        sql.CommandText += " NOT NULL ";
                    }

                    if (field.Attribute.IsPrimaryKey)
                    {
                        sql.CommandText += " PRIMARY KEY ";
                    }

                    if (field.Attribute.IsIdentity)
                    {
                        sql.CommandText += "AUTOINCREMENT ";
                    }
                    
                    sql.CommandText += ", ";

                }
                sql.CommandText = sql.CommandText.Remove(sql.CommandText.Length - 2, 2);
                sql.CommandText += " )";

                sql.ExecuteNonQuery();
            }
            catch (Exception e)
            {
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

                SQLiteCommand sql = new SQLiteCommand();
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
            SQLiteCommand command = new SQLiteCommand(sql);
            command.Connection = connection;
            SQLiteDataReader data = command.ExecuteReader();
            DataTable table = new DataTable();
            table.Load(data);
            return table;
        }

        public override int ExecuteCustomSQLNotQuery(string sql)
        {
            SQLiteCommand command = new SQLiteCommand(sql);
            command.Connection = connection;
            int ret = command.ExecuteNonQuery();
            return ret;
        }

        public void ExecuteCustomSQLDontReturn(string sql)
        {
            SQLiteCommand command = new SQLiteCommand(sql);
            command.Connection = connection;
            command.ExecuteNonQuery();
        }

        public override DataTable ExecuteCustomSQL(DbCommand sql)
        {
            if (sql is SQLiteCommand)
            {
                SQLiteCommand command = sql as SQLiteCommand;
                command.Connection = connection;
                SQLiteDataReader data = command.ExecuteReader();
                DataTable table = new DataTable();
                table.Load(data);
                return table;
            }
            throw new ArgumentException("DbCommand invalid. Not is instance of SQLiteCommand.");
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
                    return "LIKE";
                default:
                    throw new NotImplementedException("Operator not implemented");
            }
        }

        public override void ExecuteScript(string script, string delimiter)
        {
            throw new NotImplementedException("Implementar metodo");
        }

        public override DataTable ExecuteStoredProcedure(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                SQLiteCommand command = new SQLiteCommand(name);
                command.Connection = connection;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 0;
                SQLiteDataReader data = command.ExecuteReader();
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

            SQLiteCommand command = new SQLiteCommand(name);
            command.Connection = connection;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandTimeout = 0;
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
                command.Parameters.Add(new SQLiteParameter(param.Name, param.Value));
            }
            SQLiteDataReader data = command.ExecuteReader();
            DataTable table = new DataTable();
            table.Load(data);
            table.TableName = name;
            return table;
        }
        #endregion

        #region [ Protected Methods ]

        protected Object convertToDbValue(Object value)
        {
            if (value is DateTime)
            {
                value = ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss");
            }
            return value;
        }

        protected override T ConvertValue<T>(Object value)
        {
            if (typeof(T) == typeof(int) && value is Int64)
            {
                value = Convert.ToInt32((Int64)value);
            }
            if(typeof(T) == typeof(DateTime) && value is String) {
                value = DateTime.Parse((String)value);
		    }
            return (T)value;
        }

        protected override object ConvertValue(Object value, Type type)
        {
            if (type == typeof(int) && value is Int64)
            {
                value = Convert.ToInt32((Int64)value);
            }
            if (type == typeof(DateTime) && value is String)
            {
                value = DateTime.Parse((String)value);
            }
            return Convert.ChangeType(value, type);
        }

        protected override bool DeleteItem(BasicModel model)
        {
            //System.Diagnostics.Debugger.Launch();

            DatabaseType type = model.GetDatabaseType();

            SQLiteCommand sql = new SQLiteCommand();
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
                    // § // SQLiteParameter parameter = new SQLiteParameter("@" + field.Attribute.Name, field.Property.GetValue(model, null));
                    SQLiteParameter parameter = new SQLiteParameter("@" + field.Attribute.Name, convertToDbValue(field.FastGetValue(model)));
                    parameter.DbType = ConvertToSqlDbType(field.Property.PropertyType);
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

        protected override bool DeleteOneToManyRelationships(DatabaseType originalType, int argsId)
        {
            var oneToMany = originalType.DataBaseRelationship.Where(obj => obj.Property.PropertyType.IsArray);

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

        protected override bool DeleteItemsFromTheRelation(DatabaseRelationshipInfo relation, int argsId)
        {
            if (argsId <= 0)
                return true;

            var relationType = TypesManager.TypeOf(relation.ElementType);
            SQLiteCommand cmd = new SQLiteCommand();
            cmd.Connection = connection;
            var fieldInClass = relationType.DataBaseProperties.First(obj => obj.Property.Name == relation.Attribute.FieldName);

            if (relation.Attribute.Where != null)
            {
                var myWhere = relation.Attribute.Where.Clone();
                myWhere.AddItem(Operator.AND);

                myWhere.AddItemInfo(fieldInClass);
                myWhere.AddOperator(Operator.EQUAL);
                myWhere.AddItem(argsId);

                var delete = new Delete(relationType); ;
                var result = delete.AddWhere(myWhere, this);


                foreach (var item in result)
                {
                    SQLiteParameter param = new SQLiteParameter(item.Key, convertToDbValue(item.Value));
                    param.DbType = ConvertToSqlDbType(item.Value.GetType());
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

        protected override bool ExistItemsInRelation(DatabaseRelationshipInfo relation, int argsId, out int[] idItems)
        {
            if (argsId <= 0)
            {
                idItems = null;
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

                SQLiteCommand cmd = new SQLiteCommand();
                cmd.Connection = this.connection;

                foreach (var item in result)
                {
                    SQLiteParameter param = new SQLiteParameter(item.Key, convertToDbValue(item.Value));
                    param.DbType = ConvertToSqlDbType(item.Value.GetType());
                    cmd.Parameters.Add(param);

                }
                cmd.CommandText = select.ToString();
                tableRet = new System.Data.DataTable();
                SQLiteDataReader data = cmd.ExecuteReader();
                tableRet.Load(data);
                data.Close();
            }


            if (tableRet.Rows.HasElements())
            {
                idItems = tableRet.AsEnumerable().Select(obj => (int)obj[0]).ToArray();
                return true;
            }

            idItems = null;
            return false;
        }

        protected override bool UpdateItem(BasicModel model)
        {
            DatabaseType type = model.GetDatabaseType();

            SQLiteCommand sql = new SQLiteCommand();
            if (transaction != null)
            {
                sql.Transaction = transaction;
            }
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
                    SQLiteParameter parameter = new SQLiteParameter("@" + field.Attribute.Name, convertToDbValue(field.FastGetValue(model)));
                    parameter.DbType = ConvertToSqlDbType(field.Property.PropertyType);
                    sql.Parameters.Add(parameter);
                }
                if (field.Attribute.IsPrimaryKey)
                {
                    where += "`" + field.Attribute.Name + "`" + "=@" + field.Attribute.Name + " and ";
                    // § //SQLiteParameter parameter = new SQLiteParameter("@" + field.Attribute.Name, field.Property.GetValue(model, null));
                    SQLiteParameter parameter = new SQLiteParameter("@" + field.Attribute.Name, convertToDbValue(field.FastGetValue(model)));
                    parameter.DbType = ConvertToSqlDbType(field.Property.PropertyType);
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
            DeleteFiles(model);
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
                    if (value != null && value.Length > field.Attribute.Size)
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

            SQLiteCommand sql = new SQLiteCommand();
            if (transaction != null)
            {
                sql.Transaction = transaction;
            }
            sql.Connection = connection;

            string fields = "";
            string tagValues = "";
            foreach (var item in values)
            {
                fields += item.Key + ", ";
                tagValues += "@" + item.Key + ", ";
                SQLiteParameter parameter = new SQLiteParameter("@" + item.Key, convertToDbValue(item.Value));
                parameter.DbType = ConvertToSqlDbType(types[item.Key]);
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

            DatabaseFieldInfo identity=null;

            foreach (var field in type.DataBaseProperties)
            {
                if (field.Property.PropertyType == typeof(String) && field.Attribute.Size > 0)
                {
                    // § // string value = field.Property.GetValue(model, null) as string;
                    string value = field.FastGetValue(model) as string;
                    if (value != null && value.Length > field.Attribute.Size)
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

            SQLiteCommand sql = new SQLiteCommand();
            if (transaction != null)
            {
                sql.Transaction = transaction;
            }
            sql.Connection = connection;

            string fields = "";
            string tagValues = "";
            foreach (var item in values)
            {
                fields += item.Key + ", ";
                tagValues += "@" + item.Key + ", ";
                SQLiteParameter parameter = new SQLiteParameter("@" + item.Key, convertToDbValue(item.Value));
                parameter.DbType = ConvertToSqlDbType(types[item.Key]);
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

            if (identity!=null)
            {
                sql = new SQLiteCommand("select @@identity as id from " + type.TableName + ";");
                sql.Connection = connection;
                SQLiteDataReader data = sql.ExecuteReader();
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

        public override int Duplicate<T>(int id)
        {
            throw new NotImplementedException("Implementar metodo");

        }
        #endregion

        #region [ Private Methods ]

        private DbType ConvertToSqlDbType(Type type)
        {
            if (type == typeof(int))
            {
                return DbType.Int32;
            }
            if (type == typeof(Int16))
            {
                return DbType.Int16;
            }
            if (type == typeof(Int64))
            {
                return DbType.Int64;
            }

            if (type == typeof(decimal))
            {
                return DbType.Decimal;
            }

            if (type == typeof(string))
            {
                return DbType.AnsiStringFixedLength;
            }
            if (type == typeof(DateTime))
            {
                return DbType.StringFixedLength;
            }
            if (type == typeof(byte[]))
            {
                return DbType.Binary;
            }
            if (type == typeof(double))
            {
                return DbType.Double;
            }
            if (type == typeof(byte))
            {
                return DbType.Byte;
            }
            if (type == typeof(bool))
            {
                return DbType.Boolean;
            }
            throw new NotImplementedException();
        }

        private string ConvertToSqlDbTypeString(Type type, double size)
        {
            DbType dbType = ConvertToSqlDbType(type);

            switch (dbType)
            {
                case DbType.Int32:
                case DbType.Int16:
                case DbType.Int64:
                    return "INTEGER";
                case DbType.AnsiStringFixedLength:
                    return "TEXT";
                case DbType.Decimal:
                    {
                        return "real";
                    }
                case DbType.DateTime:
                    return "TEXT";
                case DbType.Binary:
                    return "BLOB";
                case DbType.Double:
                    {
                        return "real";
                    }
                case DbType.Byte:
                    return "INTEGER";
                case DbType.Boolean:
                    return "INTEGER";
            }
            throw new NotImplementedException();
        }
        #endregion
    }
}
