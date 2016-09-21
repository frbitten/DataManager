using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Joo.Database.Connections;
using System.Data;
using Joo.Database.Types;
using Joo.Database.Exceptions;
using System.Reflection;
using Joo.Database.Attributes;

namespace Joo.Database
{
    public class DataBaseManager
    {
        #region [ Fields ]
        //private List<DataBaseConnection> connections;
        #endregion

        #region [ Constructors ]
        public DataBaseManager()
        {
            //connections = new List<DataBaseConnection>();
        }
        #endregion

        #region [ Static Methods ]
        private static DataBaseManager instance=null;
        public static DataBaseManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DataBaseManager();
                }
                return instance;
            }
        }
        #endregion

        #region [ Properties ]
        public DATABASE_TYPE DataBaseType
        {
            get;
            set;
        }

        public string ConnectionString
        {
            get;
            set;
        }
        public string DirectoryFiles
        {
            get;
            set;
        }
        #endregion

        #region [ Connection Methods ]

        public DataBaseConnection OpenConnection(DATABASE_TYPE type, string connectionString)
        {
            switch (type)
            {
                case DATABASE_TYPE.MYSQL:
                    {
                        MySqlServerConnection sql = new MySqlServerConnection(connectionString);
                        return sql;
                    }
                case DATABASE_TYPE.SQLITE:
                    {
                        SqliteConnection sql = new SqliteConnection(connectionString);
                        return sql;
                    }
                case DATABASE_TYPE.SQLSERVER:
                    {
                        SqlServerConnection sql = new SqlServerConnection(connectionString);
                        //connections.Add(sql);
                        return sql;
                    }
                case DATABASE_TYPE.SQLSERVERCE:
                    {
                        SqlServerCeConnection sql = new SqlServerCeConnection(connectionString);
                        //connections.Add(sql);
                        return sql;
                    }
            }
            return null;
        }
        public DataBaseConnection OpenConnection()
        {
            switch (DataBaseType)
            {
                case DATABASE_TYPE.MYSQL:
                    {
                        MySqlServerConnection sql = new MySqlServerConnection(ConnectionString);
                        return sql;
                    }
                case DATABASE_TYPE.SQLITE:
                    {
                        SqliteConnection sql = new SqliteConnection(ConnectionString);
                        return sql;
                    }
                case DATABASE_TYPE.SQLSERVER:
                    {
                        SqlServerConnection sql = new SqlServerConnection(ConnectionString);
                        return sql;
                    }
                case DATABASE_TYPE.SQLSERVERCE:
                    {
                        SqlServerCeConnection sql = new SqlServerCeConnection(ConnectionString);
                        return sql;
                    }
            }
            return null;
        }
        public void CloseConnection(DataBaseConnection connection)
        {
            
            if (connection is SqlServerConnection)
            {
                (connection as SqlServerConnection).Close();
            }
            if (connection is MySqlServerConnection)
            {
                (connection as MySqlServerConnection).Close();
            }
            if (connection is SqlServerCeConnection)
            {
                (connection as SqlServerCeConnection).Close();
            }
            if (connection is SqliteConnection)
            {
                (connection as SqliteConnection).Close();
            }
        }

        #endregion

        #region [ Private Methods ]

        #endregion
    }
}
