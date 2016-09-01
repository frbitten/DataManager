package joo.databaseManager.SQL;

import java.util.ArrayList;
import java.util.List;

import joo.databaseManager.BasicModel;
import joo.databaseManager.type.DatabaseType;



public class OrderBy
{
	public enum ORDER{
		ASC,
		DESC
	}
	private  List<Object[]> items;
	private ORDER order;
	public OrderBy(ORDER order)
	{
		items=new ArrayList<Object[]>();
		this.order=order;
	}

	public <T extends BasicModel> void addItem(Class<T> clazz,String propertyName) {
		Object[] item = new Object[2];
		item[0] = new DatabaseType(clazz);
		item[1] = propertyName;
		items.add(item);
	}

	public List<Object[]> getItems() {
		return items;
	}

	public void setItems(List<Object[]> items) {
		this.items = items;
	}
	
	public String getOrder(){
		if(order==ORDER.ASC){
			return "ASC";
		}else{
			return "DESC";
		}
	}
		 		     
}
