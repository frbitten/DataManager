package joo.controls;

import java.util.List;

import javax.swing.DefaultComboBoxModel;

@SuppressWarnings("serial")
public class CustomComboBoxModel extends DefaultComboBoxModel<ComboBoxItem> {

	public void addValue(String text,Object value){
		addElement(new ComboBoxItem(text, value));
	}
	
	public void addRange(List<ComboBoxItem> items){
		for(ComboBoxItem item:items){
			addElement(item);
		}
	}
	
	public void setSelectItemByValue(Object value){
		for(int i=0;i<this.getSize();i++){
			ComboBoxItem item=this.getElementAt(i);
			if(item.getValue()==value){
				this.setSelectedItem(item);
			}
		}
	}
	public void setSelectItemByIndex(int index){
		this.setSelectedItem(this.getElementAt(index));
	
	}
}
