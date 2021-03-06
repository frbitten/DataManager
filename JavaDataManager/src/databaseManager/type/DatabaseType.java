package databaseManager.type;

import java.lang.reflect.Field;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import databaseManager.annotation.Table;
import databaseManager.type.RelationshipInfo.TYPE;

@SuppressWarnings("rawtypes")
public class DatabaseType {
	
	private Map<String, FieldInfo> fields;
	private Map<String, FileInfo> files;
	private Map<String,RelationshipInfo> relationshipsOneToMany;
	private Map<String,RelationshipInfo> relationshipsManyToMany;
	private Map<String,RelationshipInfo> relationshipsOneToOne;
	private String tableName;
	private Class clazz;
	
	@SuppressWarnings("unchecked")
	public DatabaseType(Class clazz){
		this.clazz=clazz;
		fields=new HashMap<String, FieldInfo>();
		files=new HashMap<String, FileInfo>();
		relationshipsOneToMany=new HashMap<String, RelationshipInfo>();
		relationshipsOneToOne=new HashMap<String, RelationshipInfo>();
		relationshipsManyToMany=new HashMap<String, RelationshipInfo>();
		processAllFields(clazz,clazz);
		Table table=(Table)clazz.getAnnotation(Table.class);
		if(table != null)
		{
		     tableName=table.Name();
		}
	}
	
	public String getTableName(){
		return tableName;
	}
	
	public List<FieldInfo> getFields(){
		return new ArrayList<FieldInfo>(fields.values());
	}
	
	public List<FileInfo> getFiles(){
		return new ArrayList<FileInfo>(files.values());
	}
	public List<RelationshipInfo> getRelationshipsOneToMany(){
		return new ArrayList<RelationshipInfo>(relationshipsOneToMany.values());
	}
	public List<RelationshipInfo> getRelationshipsOneToOne(){
		return new ArrayList<RelationshipInfo>(relationshipsOneToOne.values());
	}
	public List<RelationshipInfo> getRelationshipsManyToMany(){
		return new ArrayList<RelationshipInfo>(relationshipsManyToMany.values());
	}
	
	public Class getModelClass(){
		return clazz;
	}
	public FieldInfo getFieldInfo(String name){
		return fields.get(name.toLowerCase());
	}
	
	public FileInfo getFileInfo(String name){
		return files.get(name.toLowerCase());
	}
	public RelationshipInfo getRelationshipInfo(String name){
		if(relationshipsOneToOne.containsKey(name.toLowerCase())){
			return relationshipsOneToOne.get(name.toLowerCase());
		}
		if(relationshipsOneToMany.containsKey(name.toLowerCase())){
			return relationshipsOneToMany.get(name.toLowerCase());
		}
		if(relationshipsManyToMany.containsKey(name.toLowerCase())){
			return relationshipsManyToMany.get(name.toLowerCase());
		}
		return null;
	}
	
	private void processAllFields(Class primaryClass,Class processClass) {
		for (Field field: processClass.getDeclaredFields()) {
			addField(primaryClass,field);
	    }

	    if (processClass.getSuperclass() != null) {
	    	processAllFields(primaryClass,processClass.getSuperclass());
	    }
	}
	
	private void addField(Class primaryClass,Field f){
		if(f.isAnnotationPresent(databaseManager.annotation.Field.class)){
			FieldInfo info=new FieldInfo(primaryClass,f);
			if(fields.containsKey(info.getName().toLowerCase())){
				throw new IllegalArgumentException("O campo "+info.getName()+" j� foi definido.");
			}
			if(files.containsKey(info.getName().toLowerCase())){
				throw new IllegalArgumentException("O campo "+info.getName()+" j� foi definido.");
			}
			fields.put(info.getName().toLowerCase(),info);
		}
		
		if(f.isAnnotationPresent(databaseManager.annotation.File.class)){
			FileInfo info=new FileInfo(primaryClass,f);
			if(files.containsKey(info.getName().toLowerCase())){
				throw new IllegalArgumentException("O campo "+info.getName()+" j� foi definido.");
			}
			if(fields.containsKey(info.getName().toLowerCase())){
				throw new IllegalArgumentException("O campo "+info.getName()+" j� foi definido.");
			}
			if(!String.class.isAssignableFrom(info.getType()) && !byte[].class.isAssignableFrom(info.getType())){
				throw new IllegalArgumentException("O campo "+info.getName()+" tem que ser do tipo byte[] ou String para poder ser um FILE.");
			}
			files.put(info.getName().toLowerCase(),info);
		}
		
		if(f.isAnnotationPresent(databaseManager.annotation.RelationshipOneToMany.class)){
			RelationshipInfo info=new RelationshipInfo(f, TYPE.ONE_TO_MANY);
			relationshipsOneToMany.put(info.getName().toLowerCase(), info);
		}
		if(f.isAnnotationPresent(databaseManager.annotation.RelationshipManyToMany.class)){
			RelationshipInfo info=new RelationshipInfo(f, TYPE.MANY_TO_MANY);
			relationshipsManyToMany.put(info.getName().toLowerCase(), info);
		}
		if(f.isAnnotationPresent(databaseManager.annotation.RelationshipOneToOne.class)){
			RelationshipInfo info=new RelationshipInfo(f, TYPE.ONE_TO_ONE);
			relationshipsOneToOne.put(info.getName().toLowerCase(), info);
		}
	}
}

