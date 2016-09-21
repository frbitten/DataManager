package utils.table;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import javax.swing.event.TableModelEvent;
import javax.swing.table.AbstractTableModel;

import databaseManager.BasicModel;

@SuppressWarnings("serial")
public class TableModel<T extends BasicModel> extends AbstractTableModel {

	private List<T> models;
	private Map<Integer,Object[]> columnsByIndex;
	private int columnCount;

	public TableModel() { 
		columnsByIndex = new HashMap<Integer,Object[]>();
		models = new ArrayList<T>();
		columnCount=0;
	}
	
	public boolean addColumn(int index,String header,String fieldName,Class<?> clazz,boolean isCellEditable) {
		if( !columnsByIndex.containsKey(index)){
			Object[] values=new Object[]{header,fieldName,clazz,isCellEditable};
			columnsByIndex.put(index,values);
			columnCount++;
			this.fireTableChanged(new TableModelEvent(this,TableModelEvent.HEADER_ROW));
			return true;
		}
		return false;
	}

	@Override
	public int getColumnCount() {
		return columnCount;
	}

	@Override
	public int getRowCount() {
		return models.size();
	}

	@Override
	public String getColumnName(int columnIndex) {
		if(columnsByIndex.containsKey(columnIndex)){
			return (String)columnsByIndex.get(columnIndex)[0];
		}else{
			return "";
		}
	}

	@Override
	public Class<?> getColumnClass(int columnIndex) {
		if(columnsByIndex.containsKey(columnIndex)){
			return (Class<?>)columnsByIndex.get(columnIndex)[2];
		}else{
			return String.class;
		}
	}

	@Override
	public Object getValueAt(int rowIndex, int columnIndex) {
		T model = models.get(rowIndex);

		if(columnsByIndex.containsKey(columnIndex)){
			String property=(String)columnsByIndex.get(columnIndex)[1];
			if(property==null){
				return null;
			}
			String[] fieldNames=property.split("\\.");
			BasicModel aux=model;
			Object ret=null;
			for (int i = 0; i < fieldNames.length; i++) {
				String fieldName=fieldNames[i];
				if(fieldName!=null && fieldName!=""){
					try {
						ret=aux.getValue(fieldName);
					} catch (Exception e) {
						e.printStackTrace();
						return null;
					}
					if(ret instanceof BasicModel){
						aux=(BasicModel)ret;
					}
				}
			}
//			if(getColumnClass(columnIndex).equals(String.class)){
//				if(Date.class.equals(ret.getClass())){
//					SimpleDateFormat format=new SimpleDateFormat("dd/MM/yyyy HH:mm");
//					ret=format.format((Date)ret);
//				}
//				if(Double.class.equals(ret.getClass())){
//					NumberFormat nf = NumberFormat.getNumberInstance();
//					ret=nf.format((Double)ret);
//				}
//			}
//			
			return ret;			
		}
		return null;
	}
	
	public T getModel(int rowIndex) {
		return models.get(rowIndex);
	}

	@Override
	public void setValueAt(Object aValue, int rowIndex, int columnIndex) {
		models.get(rowIndex);
		T model = models.get(rowIndex); 

		if(columnsByIndex.containsKey(columnIndex)){
			String fieldName=(String)columnsByIndex.get(columnIndex)[1];
			if(fieldName!=null && fieldName!=""){
				try {
					model.setValue(fieldName,aValue);
				} catch (Exception e) {
					e.printStackTrace();
					return;
				}
			}
		}
		fireTableCellUpdated(rowIndex, columnIndex);
	}

	@Override
	public boolean isCellEditable(int rowIndex, int columnIndex) {
		if(columnsByIndex.containsKey(columnIndex)){
			return (boolean)columnsByIndex.get(columnIndex)[3];
		}
		return true;
	}

	public void addModel(T model) {
		models.add(model);

		int lastIndex = getRowCount() - 1;

		fireTableRowsInserted(lastIndex, lastIndex);
	}

	public void removeModel(int indexLine) {
		models.remove(indexLine);

		fireTableRowsDeleted(indexLine, indexLine);
	}
	
	public void clear() {
		models.clear();
		fireTableDataChanged();
	}

	public boolean isEmpty() {
		return models.size()==0;
	}

	public void addRangeModel(List<T> models) {
		int tamanhoAntigo = getRowCount();

		this.models.addAll(models);

		fireTableRowsInserted(tamanhoAntigo, getRowCount() - 1);
	}

}
