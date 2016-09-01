package joo.utils.table;

import java.awt.Component;
import java.awt.Dimension;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.util.Vector;

import javax.swing.DefaultCellEditor;
import javax.swing.Icon;
import javax.swing.JButton;
import javax.swing.JCheckBox;
import javax.swing.JTable;
import javax.swing.table.TableCellRenderer;

@SuppressWarnings("serial")
public class TableButton extends DefaultCellEditor implements TableCellRenderer {
	protected JButton editButton;
	protected JButton renderButton;
	protected Object value;
	protected int row;
	protected int column;
	protected Vector<ActionListener> listeners;

	public TableButton(int size) {
		super(new JCheckBox());
		listeners = new Vector<ActionListener>();
		editButton = new JButton();
		editButton.setOpaque(true);
		editButton.setSize(size, size);
		editButton.setMaximumSize(new Dimension(size,size));
		editButton.addActionListener(new ActionListener() {
			public void actionPerformed(ActionEvent e) {
				fireEditingStopped();
				ActionEvent event=new ActionEvent(TableButton.this, 0, "");
				for(ActionListener l : listeners) { 
			          l.actionPerformed(event);
				}
			}
		});
		renderButton = new JButton();
		renderButton.setOpaque(true);
		renderButton.setSize(size, size);
		renderButton.setMaximumSize(new Dimension(size,size));
	}

	public void addActionListener(ActionListener l) {
		listeners.add(l);
	}

	public void removeActionListener(ActionListener l) {
		listeners.remove(l);
	}

	public void setText(String text) {
		editButton.setText(text);
		renderButton.setText(text);
	}

	public void setIcon(Icon icon) {
		editButton.setIcon(icon);
		renderButton.setIcon(icon);
	}

	public Component getTableCellEditorComponent(JTable table, Object value,boolean isSelected, int row, int column) {
		if (isSelected) {
			editButton.setForeground(table.getSelectionForeground());
			editButton.setBackground(table.getSelectionBackground());
		} else {
			editButton.setForeground(table.getForeground());
			editButton.setBackground(table.getBackground());
		}
		this.value = value;
		this.row = row;
		this.column = column;
		return editButton;
	}

	public Object getCellEditorValue() {
		return value;
	}

	public boolean stopCellEditing() {
		return super.stopCellEditing();
	}

	@Override
	public Component getTableCellRendererComponent(JTable arg0, Object arg1,
			boolean arg2, boolean arg3, int arg4, int arg5) {
		return renderButton;
	}
	public int getRow(){
		return row;
	}
	public int getColumn(){
		return column;
	}
	
	public void setEnable(boolean enable) {
		this.editButton.setEnabled(enable);
		this.renderButton.setEnabled(enable);
	}
}
