package databaseManager.connection;

import java.sql.ResultSet;
import java.text.SimpleDateFormat;
import java.util.Date;

import databaseManager.FIELD_TYPES;
import databaseManager.SQL.Where.Operator;
import databaseManager.type.DatabaseType;
import databaseManager.type.FieldInfo;


public class SqliteConnection extends DataBaseConnection {

	public SqliteConnection(DATABASE_TYPE type, String server, String database,	String user, String password,String filesDirectory) {
		super(type, server, database, user, password,filesDirectory);
		try {
			Class.forName("org.sqlite.JDBC");
		} catch (ClassNotFoundException e) {
			e.printStackTrace();
		}
	}

	@SuppressWarnings("rawtypes")
	@Override
	public String converTypeToString(Class type, double size) {
		switch (FIELD_TYPES.getFieldType(type))
		{
			case FIELD_TYPES.INT:
				return "INTEGER";
		
			case FIELD_TYPES.DOUBLE:
			case FIELD_TYPES.FLOAT:
			case FIELD_TYPES.BIG_DECIMAL:
				return "REAL";
			case FIELD_TYPES.STRING:
			case FIELD_TYPES.DATE:
				return "TEXT";
			case FIELD_TYPES.BYTE_ARRAY:
				return "BLOB";
			case FIELD_TYPES.SHORT:
				return "INTEGER";
			case FIELD_TYPES.LONG:
				return "INTEGER";
		}
		throw new IllegalArgumentException("Tipo "+type.getName()+" n�o suportado");
	}

	@Override
	public String convertOperatorToString(Operator operador) {
		switch (operador)
        {
            case AND:
                return "AND";
            case OR:
                return "OR";
            case OPEN_PARENTHESIS:
                return "(";
            case CLOSE_PARENTHESIS:
                return ")";
            case DIFFERENT:
                return "<>";
            case EQUAL:
                return "=";
            case LESS_EQUAL:
                return "<=";
            case MINOR:
                return "<";
            case MORE:
                return ">";
            case MORE_EQUAL:
                return ">=";
            case CONTAINS:
                return "LIKE";
            default:
                throw new IllegalArgumentException("Operator not implemented");
        }
	}

	@SuppressWarnings("rawtypes")
	@Override
	public String getCreateTable(Class clazz) 
	{
		DatabaseType type = new DatabaseType(clazz);
		String sql = "CREATE TABLE IF NOT EXISTS '" + type.getTableName() + "' (";
		
		String aux = "";
        for (FieldInfo field: type.getFields())
        {
            sql += "'" + field.getName() + "' ";

            sql += converTypeToString(field.getType(), field.getSize());

            if (field.isNull())
            {
                sql += " NULL";
            }
            else
            {
                sql += " NOT NULL";
            }

            if (field.isIdentity())
            {
                sql += " PRIMARY KEY AUTOINCREMENT";
            }
            
//            if (field.isPrimaryKey())
//            {
//            	// CREATE TABLE t(x INTEGER, y, z, PRIMARY KEY(x ASC));
//                aux += " PRIMARY KEY(" + field.getName() + " ASC)";
//            }
            
            sql += ", ";
        }
        
        if (!aux.equals(""))
        {
            sql += aux;
        }
        else
        {
        	sql = sql.substring(0,sql.length() - 2);
        }
        
        sql += ");";
        return sql;
	}

	@Override
	protected Object convertToDbType(Object object) {
		if(object instanceof Date) {
			object = new SimpleDateFormat("yyyy/MM/dd HH:mm:ss").format(object);
		}
		return object;
	}

	@Override
	protected Object convertToJavaType(ResultSet data, String fieldName,int type) {		
		try{
			switch (type)
			{
				case FIELD_TYPES.INT:
					return data.getInt(fieldName);
			
				case FIELD_TYPES.DOUBLE:
					return data.getDouble(fieldName);
				case FIELD_TYPES.FLOAT:
					return data.getFloat(fieldName);
				case FIELD_TYPES.BIG_DECIMAL:
					return data.getBigDecimal(fieldName);
				case FIELD_TYPES.STRING:
					return data.getString(fieldName);
				case FIELD_TYPES.DATE:
					return  new SimpleDateFormat("yyyy/MM/dd HH:mm:ss").parse((String) data.getString(fieldName));
				case FIELD_TYPES.BYTE_ARRAY:
					return data.getBytes(fieldName);
				case FIELD_TYPES.SHORT:
					return data.getShort(fieldName);
				case FIELD_TYPES.LONG:
					return data.getLong(fieldName);
			}
		}catch(Exception e){
			e.printStackTrace();
			return null;
		}
		
		return null;
	}

}
