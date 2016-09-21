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
using System.Data;

namespace Joo.Database.Connections
{
    internal class MySqlServerConnection : DataBaseConnection
    {
        #region [ Atributes ]

        private MySqlConnection connection;

        private MySqlTransaction transaction;

        #endregion

        #region [ Public Methods ]

        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlServerConnection"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public MySqlServerConnection(string connectionString)
        {
            connection = new MySqlConnection(connectionString);
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

            MySqlCommand sql = new MySqlCommand();

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
                    MySqlParameter parameter = new MySqlParameter(item.Key, item.Value);
                    try
                    {
                        parameter.MySqlDbType = ConvertToSqlDbType(item.Value.GetType());
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

                MySqlDataReader data = sql.ExecuteReader();
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
            MySqlCommand sql = new MySqlCommand();
            sql.Connection = connection;
            DatabaseType databaseType = TypesManager.TypeOf(type);
            Select select = select = new Select(databaseType, true);
            if (where != null)
            {
                Dictionary<string, object> parameters = select.AddWhere(where, this);
                foreach (var item in parameters)
                {
                    MySqlParameter parameter = new MySqlParameter(item.Key, item.Value);
                    try
                    {
                        parameter.MySqlDbType = ConvertToSqlDbType(item.Value.GetType());
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
                MySqlDataReader data = sql.ExecuteReader();
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
            MySqlCommand sql = new MySqlCommand();
            sql.Connection = connection;
            DatabaseType databaseType = TypesManager.TypeOf(type);
            Select select = select = new Select(databaseType, true);
            if (where != null)
            {
                Dictionary<string, object> parameters = select.AddWhere(where, this);
                foreach (var item in parameters)
                {
                    MySqlParameter parameter = new MySqlParameter(item.Key, item.Value);
                    try
                    {
                        parameter.MySqlDbType = ConvertToSqlDbType(item.Value.GetType());
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
                MySqlDataReader data = sql.ExecuteReader();
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
            MySqlCommand sql = new MySqlCommand();
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
            MySqlCommand sql = new MySqlCommand();
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
                MySqlCommand sql = new MySqlCommand();
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
                MySqlCommand sql = new MySqlCommand();
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

                MySqlCommand sql = new MySqlCommand();
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
            MySqlCommand command = new MySqlCommand(sql);
            command.Connection = connection;
            MySqlDataReader data = command.ExecuteReader();
            DataTable table = new DataTable();
            table.Load(data);
            return table;
        }

        public override int ExecuteCustomSQLNotQuery(string sql)
        {
            MySqlCommand command = new MySqlCommand(sql);
            command.Connection = connection;
            int ret = command.ExecuteNonQuery();
            return ret;
        }

        public void ExecuteCustomSQLDontReturn(string sql)
        {
            MySqlCommand command = new MySqlCommand(sql);
            command.Connection = connection;
            command.ExecuteNonQuery();
        }

        public override DataTable ExecuteCustomSQL(DbCommand sql)
        {
            if (sql is MySqlCommand)
            {
                MySqlCommand command = sql as MySqlCommand;
                command.Connection = connection;
                MySqlDataReader data = command.ExecuteReader();
                DataTable table = new DataTable();
                table.Load(data);
                return table;
            }
            throw new ArgumentException("DbCommand invalid. Not is instance of MySqlCommand.");
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
                    return "LOCATE";
                default:
                    throw new NotImplementedException("Operator not implemented");
            }
        }

        public override void ExecuteScript(string script, string delimiter)
        {
            if (!string.IsNullOrEmpty(script))
            {
                MySqlScript sql = new MySqlScript();
                sql.Connection = connection;
                sql.Delimiter = delimiter;
                sql.Query = script;
                sql.Execute();
                return;
            }
            throw new ArgumentException("script is empty");
        }

        public override DataTable ExecuteStoredProcedure(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                MySqlCommand command = new MySqlCommand(name);
                command.Connection = connection;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 0;
                MySqlDataReader data = command.ExecuteReader();
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

            MySqlCommand command = new MySqlCommand(name);
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
                command.Parameters.Add(new MySqlParameter(param.Name, param.Value));
            }
            MySqlDataReader data = command.ExecuteReader();
            DataTable table = new DataTable();
            table.Load(data);
            table.TableName = name;
            return table;
        }
        #endregion

        #region [ Protected Methods ]

        protected override bool DeleteItem(BasicModel model)
        {
            //System.Diagnostics.Debugger.Launch();

            DatabaseType type = model.GetDatabaseType();

            MySqlCommand sql = new MySqlCommand();
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
                    // § // MySqlParameter parameter = new MySqlParameter("@" + field.Attribute.Name, field.Property.GetValue(model, null));
                    MySqlParameter parameter = new MySqlParameter("@" + field.Attribute.Name, field.FastGetValue(model));
                    parameter.MySqlDbType = ConvertToSqlDbType(field.Property.PropertyType);
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
            MySqlCommand cmd = new MySqlCommand();
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
                    MySqlParameter param = new MySqlParameter(item.Key, item.Value);
                    param.MySqlDbType = ConvertToSqlDbType(item.Value.GetType());
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

                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = this.connection;

                foreach (var item in result)
                {
                    MySqlParameter param = new MySqlParameter(item.Key, item.Value);
                    param.MySqlDbType = ConvertToSqlDbType(item.Value.GetType());
                    cmd.Parameters.Add(param);

                }
                cmd.CommandText = select.ToString();
                tableRet = new System.Data.DataTable();
                MySqlDataReader data = cmd.ExecuteReader();
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

            MySqlCommand sql = new MySqlCommand();
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
                    // § // MySqlParameter parameter = new MySqlParameter("@" + field.Attribute.Name, field.Property.GetValue(model, null));
                    MySqlParameter parameter = new MySqlParameter("@" + field.Attribute.Name, field.FastGetValue(model));
                    parameter.MySqlDbType = ConvertToSqlDbType(field.Property.PropertyType);
                    sql.Parameters.Add(parameter);
                }
                if (field.Attribute.IsPrimaryKey)
                {
                    where += "`" + field.Attribute.Name + "`" + "=@" + field.Attribute.Name + " and ";
                    // § //MySqlParameter parameter = new MySqlParameter("@" + field.Attribute.Name, field.Property.GetValue(model, null));
                    MySqlParameter parameter = new MySqlParameter("@" + field.Attribute.Name, field.FastGetValue(model));
                    parameter.MySqlDbType = ConvertToSqlDbType(field.Property.PropertyType);
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

            MySqlCommand sql = new MySqlCommand();
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
                MySqlParameter parameter = new MySqlParameter("@" + item.Key, item.Value);
                parameter.MySqlDbType = ConvertToSqlDbType(types[item.Key]);
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

            MySqlCommand sql = new MySqlCommand();
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
                MySqlParameter parameter = new MySqlParameter("@" + item.Key, item.Value);
                parameter.MySqlDbType = ConvertToSqlDbType(types[item.Key]);
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
                if (sql.LastInsertedId > 0)
                {
                    identity.FastSetValue(model, Convert.ToInt32(sql.LastInsertedId));
                }
                else
                {
                    return false;
                }
            }
            model.Status = Status.Normal;
            SaveFiles(model);
            return true;

        }

        public override int Duplicate<T>(int id)
        {
            MySqlCommand sql = new MySqlCommand();
            if (transaction != null)
            {
                sql.Transaction = transaction;
            }
            sql.Connection = connection;
            

            DatabaseType type = TypesManager.TypeOf(typeof(T));

            String query = "INSERT INTO "+type.TableName+" ";

            //duplicar evento
            String fields = "";
            foreach (DatabaseFieldInfo field in type.DataBaseProperties)
            {
                if (!field.Attribute.IsIdentity)
                {
                    if (!String.IsNullOrEmpty(fields))
                    {
                        fields += ",";
                    }
                    fields += field.Attribute.Name;
                }
            }

            query += "(" + fields + ") SELECT " + fields + " FROM "+type.TableName+" WHERE "+type.TableName+".id=" + id;

            sql.CommandText = query;
            sql.ExecuteNonQuery();
            int ret = (int)sql.LastInsertedId;

            //duplicar relações one to many
            foreach (DatabaseRelationshipInfo info in type.DataBaseRelationship)
            {
                if (info.Property.PropertyType.IsArray)
                {
                    DuplicateRelationships(info.ElementType, info.Attribute.FieldName, id,ret);
                }
            }

            return ret;

        }
        #endregion

        #region [ Private Methods ]

        private void DuplicateRelationships(Type child,String fieldName,int oldParentId,int newParentId)
        {
            DatabaseType childType = TypesManager.TypeOf(child);
            String fields = "";
            foreach (DatabaseFieldInfo field in childType.DataBaseProperties)
            {
                if (!field.Attribute.IsIdentity && field.Attribute.Name.ToLower()!=fieldName.ToLower())
                {
                    if (!String.IsNullOrEmpty(fields))
                    {
                        fields += ",";
                    }
                    fields += field.Attribute.Name;
                }
            }
            String query = "INSERT INTO " + childType.TableName + " ( " + fields + ","+fieldName+" ) SELECT " + fields + ","+newParentId+" as "+fieldName+"  from " + childType.TableName + " WHERE " + childType.TableName + "." + fieldName + "=" + oldParentId;

            MySqlCommand sql = new MySqlCommand();
            if (transaction != null)
            {
                sql.Transaction = transaction;
            }
            sql.Connection = connection;

            sql.CommandText = query;
            sql.ExecuteNonQuery();
            int newId = (int)sql.LastInsertedId;

            //duplicar relações one to many
            foreach (DatabaseRelationshipInfo info in childType.DataBaseRelationship)
            {
                if (info.Property.PropertyType.IsArray)
                {
                    sql= new MySqlCommand("select id from "+childType.TableName+" where "+childType.TableName+"."+fieldName+"="+oldParentId);
                    if (transaction != null)
                    {
                        sql.Transaction = transaction;
                    }
                    sql.Connection = connection;
                    MySqlDataReader result=sql.ExecuteReader();
                    List<int> ids = new List<int>();
                    if (result.HasRows)
                    {
                        while (result.Read())
                        {
                            int id=(int)result["id"];
                            ids.Add(id);
                        }
                    }
                    result.Close();
                    foreach (int id in ids)
                    {
                        DuplicateRelationships(info.ElementType, info.Attribute.FieldName, id, newId);
                    }
                }
            }
        }

        private MySqlDbType ConvertToSqlDbType(Type type)
        {
            if (type == typeof(int))
            {
                return MySqlDbType.Int32;
            }
            if (type == typeof(Int16))
            {
                return MySqlDbType.Int16;
            }
            if (type == typeof(Int64))
            {
                return MySqlDbType.Int64;
            }

            if (type == typeof(decimal))
            {
                return MySqlDbType.Decimal;
            }

            if (type == typeof(string))
            {
                return MySqlDbType.VarChar;
            }
            if (type == typeof(DateTime))
            {
                return MySqlDbType.DateTime;
            }
            if (type == typeof(byte[]))
            {
                return MySqlDbType.LongBlob;
            }
            if (type == typeof(double))
            {
                return MySqlDbType.Double;
            }
            if (type == typeof(byte))
            {
                return MySqlDbType.UByte;
            }
            if (type == typeof(bool))
            {
                return MySqlDbType.Bit;
            }
            throw new NotImplementedException();
        }

        private string ConvertToSqlDbTypeString(Type type, double size)
        {
            MySqlDbType dbType = ConvertToSqlDbType(type);

            switch (dbType)
            {
                case MySqlDbType.Int32:
                    return "int";
                case MySqlDbType.Int16:
                    return "smallint";
                case MySqlDbType.Int64:
                    return "bigint";
                case MySqlDbType.VarChar:
                    if (size <= 0)
                    {
                        return "text";
                    }
                    else
                    {
                        return "varchar(" + size + ")";
                    }
                case MySqlDbType.Decimal:
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
                case MySqlDbType.DateTime:
                    return "DATETIME";
                case MySqlDbType.LongBlob:
                    return "LONGBLOB";
                case MySqlDbType.Double:
                    {
                        NumberFormatInfo numberInfo = new NumberFormatInfo();
                        numberInfo.NumberDecimalSeparator = ",";
                        string aux = "10,2";
                        if (size > 0)
                        {
                            aux = size.ToString("N1", numberInfo);
                        }
                        return "double(" + aux + ")";
                    }
                case MySqlDbType.UByte:
                    return "UByte";
                case MySqlDbType.Bit:
                    return "bit";
            }
            throw new NotImplementedException();
        }
        #endregion
    }
}
