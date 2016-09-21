package controls;

import javax.swing.DefaultComboBoxModel;

import databaseManager.BasicModel;

@SuppressWarnings("serial")
public class JooComboBoxModel<T extends BasicModel> extends DefaultComboBoxModel<T> {

	public JooComboBoxModel(T[] values){
		super(values);
	}
	
	public void setSelectItemById(int id){
		if(id==0){
			setSelectedItem(null);
			return;
		}
		int count=this.getSize();
		for (int i = 0; i < count; i++) {
			T item=getElementAt(i);
			if(item!=null){
				if(item.getID()==id){
					setSelectedItem(item);
					return;
				}
			}
		}
		setSelectedItem(null);
	}
}
