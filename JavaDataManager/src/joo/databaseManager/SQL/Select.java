package joo.databaseManager.SQL;

import java.util.ArrayList;
import java.util.List;

import joo.databaseManager.SQL.OrderBy.ORDER;
import joo.databaseManager.SQL.Where.Operator;
import joo.databaseManager.connection.DataBaseConnection;
import joo.databaseManager.type.DatabaseType;
import joo.databaseManager.type.FieldInfo;

public class Select {

	private List<String> fields;
    private List<String> froms;
    private List<String> where;
    private int limitStart;
    private int limitLength;
    private OrderBy orderBy;
    private List<Object> whereParameters;
    private String groupBy;
    
	@SuppressWarnings("unchecked")
	public Select(DatabaseType type) {
		groupBy=" group by "+type.getTableName()+".id ";
		fields = new ArrayList<String>();
        froms = new ArrayList<String>();
        where = new ArrayList<String>();
        whereParameters=new ArrayList<Object>();
        
        addFrom(type.getTableName());
        //groupBy = " group by " + type.TableName + ".id";
        OrderBy orderBy = new OrderBy(ORDER.ASC);
        orderBy.addItem(type.getModelClass(), "ID");

        for (FieldInfo field: type.getFields())
        {
            addField(type.getTableName() + "." + field.getName());
        }
	}
	
	public void addField(String name)
    {
        for (String item : fields) {
			if(item==name){
				return;
			}
		}
        fields.add(name);
    }

    private void addWhere(String item)
    {
        where.add(item);
    }
    
    public void addFrom(String table)
    {
        for (String item:froms) {
			if(item==table){
				return;
			}
		}
        froms.add(table);
    }

    public boolean hasFrom()
    {
        return froms.size() > 0;
    }

    public boolean hasField()
    {
        return fields.size() > 0;
    }

    public boolean hasWhere()
    {
        return where.size() > 0;
    }

	public int getLimitStart() {
		return limitStart;
	}

	public void setLimitStart(int limitStart) {
		this.limitStart = limitStart;
	}

	public int getLimitLength() {
		return limitLength;
	}

	public void setLimitLength(int limitLength) {
		this.limitLength = limitLength;
	}

	public OrderBy getOrderBy() {
		return orderBy;
	}

	public void setOrderBy(OrderBy orderBy) {
		this.orderBy = orderBy;
	}
	public List<Object> getParameters(){
		return whereParameters;
	}

	public void addWhere(Where where, DataBaseConnection connection) {
        if(where==null){
        	return;
        }
		List<Object> items=where.getItems();
        for (int i = 0; i < items.size(); i++)
        {
            Object item = items.get(i);
            if (item instanceof FieldInfo)
            {
                if (items.size() > i + 1 && items.get(i + 1) instanceof Operator && ((Operator)items.get(i + 1)) == Operator.CONTAINS)
                {
                    continue;
                }
                FieldInfo info=(FieldInfo)item;
                DatabaseType type=new DatabaseType(info.getOwnerClazz());
                String tableName=type.getTableName();
                if(tableName!=null && tableName!=""){
                	addFrom(tableName);
                }
                addWhere(type.getTableName() + "." + info.getName());
            }
            else
            {
                if (item instanceof Operator)
                {
                    switch ((Operator)item)
                    {
                        case AND:
                        case OR:
                        case OPEN_PARENTHESIS:
                        case CLOSE_PARENTHESIS:
                        case DIFFERENT:
                        case EQUAL:
                        case LESS_EQUAL:
                        case MINOR:
                        case MORE:
                        case MORE_EQUAL:
                            addWhere(connection.convertOperatorToString((Operator)item));
                            break;
                        case CONTAINS:
                            {
                                if (!(items.get(i-1) instanceof FieldInfo ))
                                {
                                    throw new IllegalAccessError("Where is not valid. Contains syntax invalid");
                                }
                                
                                if (!(items.get(i+1) instanceof String ) && !(items.get(i+1) instanceof FieldInfo ))
                                {
                                    throw new IllegalAccessError("Where is not valid. Contains syntax invalid");
                                }
                                String aux="";
                                FieldInfo info=(FieldInfo)items.get(i-1);
                                DatabaseType type=new DatabaseType(info.getOwnerClazz());
                                addFrom(type.getTableName());
                                aux += type.getTableName() + "." + info.getName();
                                
                                
                                aux += " " + connection.convertOperatorToString((Operator)item) + " ";

                                if (items.get(i+1) instanceof FieldInfo)
                                {
                                    info=(FieldInfo)items.get(i+1);
                                    type=new DatabaseType(info.getOwnerClazz());
                                    addFrom(type.getTableName());
                                    aux += type.getTableName() + "." + info.getName();
                                }
                                else
                                {
                                    aux += "'%"+items.get(i+1).toString()+"%'";
                                }
                                
                                addWhere(aux);
                                i++;
                            }
                            break;
                    }
                }
                else
                {
                    addWhere("?");
                    whereParameters.add(item);
                }
            }
        }
	}
	
	@Override
	public String toString(){
		String ret="select ";
		for (int i = 0; i < fields.size(); i++) {
			ret+=fields.get(i);
			if(i<fields.size()-1){
				ret+=", ";
			}
		}
		ret+=" from ";
		for (int i = 0; i < froms.size(); i++) {
			ret+="`"+froms.get(i)+"`";
			if(i<froms.size()-1){
				ret+=", ";
			}
		}
		
		if(where.size()>0){
			ret+=" where ";
			for (int i = 0; i < where.size(); i++) {
				ret+=where.get(i)+" ";
			}
		}
		
		ret+=groupBy;
		
		if(orderBy!=null && orderBy.getItems().size()>0){
			ret+="order by ";
			List<Object[]> items=orderBy.getItems();
			for (int i = 0; i < items.size(); i++) {
				DatabaseType type=(DatabaseType)items.get(i)[0];
				String field=(String)items.get(i)[1];
				ret+=type.getTableName()+"."+field;
				if(i<items.size()-1){
					ret+=", ";
				}
			}
			ret+=" "+orderBy.getOrder();
		}
		if (limitStart >= 0 && limitLength > 0)
        {
            ret+=" LIMIT " + limitStart + " , " + limitLength;
        }
		
        return ret;
	}

}
