package joo.controls;

public class ComboBoxItem {
	private String text;
	private Object value;
	
	public ComboBoxItem(String text,Object value){
		this.text=text;
		this.value=value;
	}
	
	public String toString(){
		return text;
	}

	public String getText() {
		return text;
	}

	public Object getValue() {
		return value;
	}
}
