package joo.databaseManager;

public class Return<T> {
	private T value;
	private boolean hasValue;
	public Return(T value) {
		this.value=value;
		hasValue=true;
	}
	public Return() {
		hasValue=false;
		value=null;
	}
	
	public T getValue(){
		return value;
	}
	public boolean hasValue(){
		return hasValue;
	}
	public void setValue(T value){
		this.value=value;
		hasValue=true;
	}
	public void removeValue() {
		this.value=null;
		hasValue=false;
	}
}
