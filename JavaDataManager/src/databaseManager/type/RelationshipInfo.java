package databaseManager.type;

import java.lang.reflect.Field;
import java.lang.reflect.ParameterizedType;
import java.lang.reflect.Type;

import databaseManager.Return;
import databaseManager.annotation.RelationshipOneToMany;
import databaseManager.annotation.RelationshipOneToOne;

public class RelationshipInfo {

	public enum TYPE{
		ONE_TO_ONE,
		ONE_TO_MANY,
		MANY_TO_MANY
	}
	private String fieldName;
	private Field field;
	private String name;
	@SuppressWarnings({"rawtypes" })
	private Class returnClazz;
	@SuppressWarnings("rawtypes")
	private Class genericClazz;
	@SuppressWarnings({ "rawtypes" })
	private Class ownerClazz;
	private TYPE type;
		
	@SuppressWarnings("rawtypes")
	public RelationshipInfo(Field field, TYPE type){
		this.type=type;
		field.setAccessible(true);
		Type[] typeArguments=null;
		if(Return.class.isAssignableFrom(field.getType())){
			Type genericType = field.getGenericType();
			if(genericType instanceof ParameterizedType)
	        {
				ParameterizedType paramType= (ParameterizedType) genericType;               
	            typeArguments = paramType.getActualTypeArguments();
	        }
		}else{
			throw new IllegalArgumentException("Field de relação precisa ter campo de Return. Use Return<"+field.getType().getSimpleName()+">");
		}
		
		switch (type) {
		case MANY_TO_MANY:			
			break;
		case ONE_TO_MANY:{
				RelationshipOneToMany anotation=field.getAnnotation(RelationshipOneToMany.class);
				fieldName=anotation.FieldName().toLowerCase();
				name=anotation.Name().toLowerCase();
				if(typeArguments!=null && typeArguments.length>0){
					if(typeArguments[0] instanceof ParameterizedType)
			        {
						ParameterizedType paramType= (ParameterizedType) typeArguments[0];
						this.returnClazz=(Class)paramType.getRawType();
			            typeArguments = paramType.getActualTypeArguments();
			            if(typeArguments.length>0){
			            	this.genericClazz=(Class)typeArguments[0];
			            }
			        }
	            }
			}
			break;
		case ONE_TO_ONE:{
				RelationshipOneToOne anotation=field.getAnnotation(RelationshipOneToOne.class);
				fieldName=anotation.FieldName().toLowerCase();
				name=anotation.Name().toLowerCase();
				if(typeArguments!=null && typeArguments.length>0){
	            	this.returnClazz=(Class)typeArguments[0];
	            }
			}
			break;
		default:
			break;
		}		
		this.field=field;
		ownerClazz=field.getDeclaringClass();
	}
	public String getFieldName(){
		return fieldName;
	}
	public String getName(){
		return name;
	}
	public Field getField(){
		return field;
	}
	@SuppressWarnings("rawtypes")
	public Class getType(){
		return returnClazz;
	}
	@SuppressWarnings("rawtypes")
	public Class getOwnerClass() {
		return ownerClazz;
	}
	@SuppressWarnings("rawtypes")
	public Class getGenericClass(){
		return genericClazz;
	}
	public TYPE getRelationshipType(){
		return type;
	}
	public boolean isOneToMany(){
		return type==TYPE.ONE_TO_MANY;
	}
	public boolean isOneToOne(){
		return type==TYPE.ONE_TO_ONE;
	}
	public boolean isManyToMany(){
		return type==TYPE.MANY_TO_MANY;
	}
}
