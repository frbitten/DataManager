package controls;

import java.awt.Component;
import java.awt.event.ActionListener;
import java.awt.event.ComponentEvent;
import java.awt.event.ComponentListener;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;

import javax.swing.Icon;
import javax.swing.JTable;
import javax.swing.event.TableModelEvent;
import javax.swing.table.TableCellEditor;
import javax.swing.table.TableCellRenderer;
import javax.swing.table.TableColumn;
import javax.swing.text.JTextComponent;

import utils.table.TableButton;
import utils.table.TableModel;
import databaseManager.BasicModel;

@SuppressWarnings("serial")
public class JooTable<T extends BasicModel> extends JTable implements ComponentListener {
	public enum RESIZE{
		EXPANDED,
		FIXED
	}
	private TableModel<T> model;
	
	private List<Integer> expandeds;
	private List<Integer> fixeds;
	private HashMap<Integer, Object[]> buttons;
	
	public JooTable(){
		super();
		expandeds=new ArrayList<Integer>();
		fixeds=new ArrayList<Integer>();
		this.buttons=new HashMap<Integer, Object[]>();
		this.addComponentListener(this);
		model=new TableModel<T>();
		this.setModel(model);
	}
	public boolean addColumn(int index,String header,String fieldName,Class<?> clazz,boolean isCellEditable,RESIZE resize) {
		boolean ret= model.addColumn(index, header, fieldName, clazz, isCellEditable);
		if(ret){
			model.fireTableStructureChanged();
			if(resize==RESIZE.EXPANDED){
				expandeds.add(index);
			}else{
				fixeds.add(index);
			}
		}
		return ret;
	}
	
	public boolean addButtonColumn(int index,Icon icon,int size,ActionListener listener){
//		if(!buttons.containsKey(index)){
//			buttons.put(index, new Object[2]);
//		}
//		Object[] values=buttons.get(index);
//		
//		//model.fireTableStructureChanged();
//		TableButton btn=new TableButton(size);
//		btn.setIcon(icon);
//		btn.addActionListener(listener);
//		values[0]=btn;
//		values[1]=size;
//		return model.addColumn(index, "", "", String.class, true);
		return addButtonColumn(index, icon, size, true, listener);
	}
	
	public boolean addButtonColumn(int index, Icon icon, int size, boolean permission, ActionListener listener){
		if(!buttons.containsKey(index)){
			buttons.put(index, new Object[2]);
		}
		Object[] values=buttons.get(index);
		
		//model.fireTableStructureChanged();
		TableButton btn=new TableButton(size);
		if(!permission) {
			btn.setEnable(false);
		}
		btn.setIcon(icon);
		btn.addActionListener(listener);
		values[0]=btn;
		values[1]=size;
		return model.addColumn(index, "", "", String.class, true);
	}
	
	@Override
	public void tableChanged(TableModelEvent e){
		super.tableChanged(e);
		if(buttons==null){
			return;
		}
		if(e.getFirstRow()==TableModelEvent.HEADER_ROW){
			for(Integer key:buttons.keySet()){
				Object[] values=buttons.get(key);
				TableColumn column=getColumnModel().getColumn(key);
				column.setPreferredWidth((int)values[1]);
				column.setWidth((int)values[1]);
				column.setCellEditor((TableButton)values[0]);
				column.setCellRenderer((TableButton)values[0]);
			}
			
		}
	}
	
	public void clear(){
		model.clear();
	}
	public T getModel(int row){
		return model.getModel(row);
	}
	public void addRangeModel(List<T> models){
		model.addRangeModel(models);
	}
	
	@Override
	public void componentHidden(ComponentEvent arg0) {
		
	}
	@Override
	public void componentMoved(ComponentEvent arg0) {
		
	}
	@Override
	public void componentResized(ComponentEvent arg0) {
		int width=0;
		for (int i = 0; i < getColumnCount(); i++)
		{
			if(buttons.containsKey(i)){
				Object[] values= buttons.get(i);
				updateTableColumn(i,(int)values[1]);
				width+=(int)values[1];
			}else{
				int columnHeaderWidth = getColumnHeaderWidth( i );
				int columnDataWidth   = getColumnDataWidth( i );
				int preferredWidth    = Math.max(columnHeaderWidth, columnDataWidth);
				preferredWidth+=4;
				updateTableColumn(i, preferredWidth);
				width+=preferredWidth;
			}
		}
		
		int maxWidth=getWidth();
		if(width<maxWidth){
			int delta=maxWidth-width;
			if(expandeds.size() > 0) {
				delta/=expandeds.size();
			}			
			for(Integer index:expandeds){
				TableColumn tableColumn = getColumnModel().getColumn(index);
				width=tableColumn.getWidth()+delta;
				updateTableColumn(index, width);
			}
		}
		
	}
	@Override
	public void componentShown(ComponentEvent arg0) {
		
	}
	
	/*
	 *  Update the TableColumn with the newly calculated width
	 */
	private void updateTableColumn(int column, int width)
	{
		TableColumn tableColumn = getColumnModel().getColumn(column);

		//if (! tableColumn.getResizable()) return;

		//width += 2;

		getTableHeader().setResizingColumn(tableColumn);
		tableColumn.setWidth(width);
	}
	
	/*
	 *  Calculated the width based on the column name
	 */
	private int getColumnHeaderWidth(int column)
	{
		TableColumn tableColumn = getColumnModel().getColumn(column);
		Object value = tableColumn.getHeaderValue();
		TableCellRenderer renderer = tableColumn.getHeaderRenderer();

		if (renderer == null)
		{
			renderer = getTableHeader().getDefaultRenderer();
		}

		Component c = renderer.getTableCellRendererComponent(this, value, false, false, -1, column);
		return c.getPreferredSize().width;
	}
	
	/*
	 *  Calculate the width based on the widest cell renderer for the
	 *  given column.
	 */
	private int getColumnDataWidth(int column)
	{
		int preferredWidth = 0;
		int maxWidth = getColumnModel().getColumn(column).getMaxWidth();

		for (int row = 0; row < getRowCount(); row++)
		{
    		preferredWidth = Math.max(preferredWidth, getCellDataWidth(row, column));

			//  We've exceeded the maximum width, no need to check other rows

			if (preferredWidth >= maxWidth)
			    break;
		}

		return preferredWidth;
	}
	
	/*
	 *  Get the preferred width for the specified cell
	 */
	private int getCellDataWidth(int row, int column)
	{
		TableCellRenderer cellRenderer = getCellRenderer(row, column);
		Component c = prepareRenderer(cellRenderer, row, column);
		int width = c.getPreferredSize().width + getIntercellSpacing().width;

		return width;
	}
	
	/*
	 *  Select text in a editable cell when clicked
	 */
	@Override
	public Component prepareEditor(TableCellEditor editor, int row, int column) {
		Component c = super.prepareEditor(editor, row, column);
	    if (c instanceof JTextComponent) {
	        ((JTextComponent) c).selectAll();
	    } 
	    return c;
	}
}
