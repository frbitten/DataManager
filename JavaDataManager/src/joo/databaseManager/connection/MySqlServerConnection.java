package joo.databaseManager.connection;

import java.sql.ResultSet;
import java.util.Date;

import joo.databaseManager.FIELD_TYPES;
import joo.databaseManager.SQL.Where.Operator;
import joo.databaseManager.type.DatabaseType;
import joo.databaseManager.type.FieldInfo;



public class MySqlServerConnection extends DataBaseConnection {

	
	public MySqlServerConnection(DATABASE_TYPE type, String server,String database,String user,String password,String filesDirectory) {
		super(type, server,database,user,password,filesDirectory);
	}

	@Override
	public String convertOperatorToString(Operator operador) {
		switch (operador)
        {
            case AND:
                return "and";
            case OR:
                return "or";
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
	public String converTypeToString(Class type, double size) {
		switch (FIELD_TYPES.getFieldType(type)) {
		case FIELD_TYPES.STRING:
			if(size<=0){
				return "TEXT";
			}
			return "VARCHAR("+String.format("%.0f", size)+")";
		case FIELD_TYPES.INT:
			return "INT";
		case FIELD_TYPES.DOUBLE:
		case FIELD_TYPES.FLOAT:{
			if(size<=0){
				throw new IllegalArgumentException("Tamanho do campo não foi informado.");
			}
			String ret="DOUBLE("+String.format("%.1f", size)+")";
			return ret.replace('.', ',');
		}
		case FIELD_TYPES.DATE:
			return "DATETIME";
		case FIELD_TYPES.BIG_DECIMAL:{
			String ret="DECIMAL("+String.format("%.1f", size)+")";
			return ret.replace('.', ',');
		}
		case FIELD_TYPES.BYTE_ARRAY:
			return "BLOB";
		case FIELD_TYPES.SHORT:
			return "SMALLINT";
		case FIELD_TYPES.LONG:
			return "BIGINT";
		}
			
		throw new IllegalArgumentException("Tipo "+type.getName()+" não suportado");
	}

	@SuppressWarnings("rawtypes")
	@Override
	public String getCreateTable(Class clazz) {
		DatabaseType type = new DatabaseType(clazz);
        String sql="CREATE TABLE IF NOT EXISTS `" + type.getTableName() + "` ( ";

        String aux = "";
        for (FieldInfo field: type.getFields())
        {
            sql += " `" + field.getName() + "` ";

            sql += " " + converTypeToString(field.getType(), field.getSize()) + " ";

            if (field.isNull())
            {
                sql += " NULL ";
            }
            else
            {
                sql += " NOT NULL ";
            }

            if (field.isIdentity())
            {
                sql += "AUTO_INCREMENT ";
            }
            if (field.isPrimaryKey())
            {
                aux += " PRIMARY KEY(" + field.getName() + ")";
            }
            sql += ", ";

        }
        if (aux!="")
        {
            sql += aux;
        }else{
        	sql = sql.substring(0,sql.length() - 2);
        }
        sql += " ) ENGINE=InnoDB DEFAULT CHARSET=utf8 AUTO_INCREMENT=1 ";
        return sql;
	}

	@Override
	protected Object convertToDbType(Object object) {
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
					return  new Date(data.getTimestamp(fieldName).getTime());
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
