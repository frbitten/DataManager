package databaseManager.type;

import java.lang.reflect.Field;
import java.lang.reflect.ParameterizedType;
import java.lang.reflect.Type;

import databaseManager.Return;

@SuppressWarnings("rawtypes")
public class FieldInfo {

	private String name;
	private Field field;
	private double size;
	
	private Class returnClazz;
	private Class ownerClazz;
	private int type;
	private boolean simpleField;
	private boolean required;
	
	public FieldInfo(Class primaryClass,Field field){
		databaseManager.annotation.Field anotation=field.getAnnotation(databaseManager.annotation.Field.class);
		required=field.getAnnotation(databaseManager.annotation.Required.class)!=null;
		name=anotation.Name().toLowerCase();
		size=anotation.Size();
		this.type=anotation.Type();
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
	public String getName(){
		return name;
	}
	public double getSize(){
		return size;
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
	
	public boolean isIdentity()
    {
        return (type & databaseManager.annotation.Field.IDENTITY) == databaseManager.annotation.Field.IDENTITY;
    }
    public boolean isPrimaryKey()
    {
        return (type & databaseManager.annotation.Field.PRIMARY_KEY) == databaseManager.annotation.Field.PRIMARY_KEY;
    }
    public boolean isNull()
    {
        return (type | databaseManager.annotation.Field.NULL) == databaseManager.annotation.Field.NULL;
    }
    public boolean isSimpleField(){
    	return simpleField;
    }
    
    public boolean isRequired(){
    	return required;
    }
}
