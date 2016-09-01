package joo.databaseManager.type;

import java.lang.reflect.Field;
import java.lang.reflect.ParameterizedType;
import java.lang.reflect.Type;

import joo.databaseManager.Return;

@SuppressWarnings("rawtypes")
public class FileInfo {

	private String name;
	private Field field;
	
	private Class returnClazz;
	private Class ownerClazz;
	private boolean simpleField;
	private boolean required;
	private String extension;
	
	public FileInfo(Class primaryClass, Field field) {
		joo.databaseManager.annotation.File anotation=field.getAnnotation(joo.databaseManager.annotation.File.class);
		required=field.getAnnotation(joo.databaseManager.annotation.Required.class)!=null;
		name=anotation.Name().toLowerCase();
		extension=anotation.Extension();
		this.field=field;
		field.setAccessible(true);
		
		if(Return.class.isAssignableFrom(field.getType())){
			this.simpleField=false;
			Type genericType = field.getGenericType();
			if(genericType instanceof ParameterizedType)
	        {
				ParameterizedType type = (ParameterizedType) genericType;               
	            Type[] typeArguments = type.getActualTypeArguments();
	            if(typeArguments.length>0){
	            	this.returnClazz=(Class)typeArguments[0];
	            }
	        }
		}else{
			this.simpleField=true;
			this.returnClazz=field.getType();
		}		
		
		ownerClazz=primaryClass;
	}
	public String getExtension() {
		return extension;
	}
	public void setExtension(String extension) {
		this.extension = extension;
	}

	public String getName(){
		return name;
	}
	
	public Field getField(){
		return field;
	}
	public Class getType(){
		return returnClazz;
	}
	public Class getOwnerClazz() {
		return ownerClazz;
	}
    public boolean isSimpleField(){
    	return simpleField;
    }
    
    public boolean isRequired(){
    	return required;
    }
	
}
