package databaseManager.SQL;

import java.util.ArrayList;
import java.util.List;

import databaseManager.BasicModel;
import databaseManager.type.DatabaseType;
import databaseManager.type.FieldInfo;

public class Where {

	public enum Operator
    {
        EQUAL,
        MORE_EQUAL,
        LESS_EQUAL,
        DIFFERENT,
        MORE,
        MINOR,
        AND,
        OR,
        OPEN_PARENTHESIS,
        CLOSE_PARENTHESIS,
        CONTAINS
    }
	private List<Object> items;
	
	public Where()
	{
		items=new ArrayList<Object>();
	}
	
	public<T extends BasicModel> void addField(Class<T> clazz, String propertyName)
    {
        if (!BasicModel.class.isAssignableFrom(clazz))
        {
            throw new IllegalArgumentException("O Tipo " + clazz.getName() + " não herda BasicModel");
        }
        
        DatabaseType type=new DatabaseType(clazz);
        FieldInfo info=type.getFieldInfo(propertyName);
        items.add(info);
    }
	public void addValue(Object obj){
		items.add(obj);
	}
	public void addOperator(Operator operator){
		items.add(operator);
	}
	
	public List<Object> getItems(){
		return items;
	}

}
