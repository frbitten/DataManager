package databaseManager;

import java.math.BigDecimal;
import java.text.DecimalFormat;
import java.text.DecimalFormatSymbols;
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;
import java.util.Locale;

import javax.xml.bind.DatatypeConverter;

import databaseManager.annotation.Field;
import databaseManager.type.DatabaseType;
import databaseManager.type.FieldInfo;
import databaseManager.type.RelationshipInfo;

public abstract class BasicModel {
	
	public enum Status
    {
        /// <remarks/>
        New,
        /// <remarks/>
        Update,
        /// <remarks/>
        Delete,
        /// <remarks/>
        Normal,
        /// <remarks/>
        Invalid
    }
	
	private List<ModelListener> modelListeners = null;
	
	public void addModelListener(ModelListener listener) {
		if(modelListeners == null) {
			modelListeners = new ArrayList<ModelListener>();
		}
		modelListeners.add(listener);
	}
	
	public void removeModelListener(int index) {
		if(modelListeners != null) {
			modelListeners.remove(index);
		}
	}
	
	public void removeModelListener(ModelListener listener) {
		if(modelListeners != null) {
			modelListeners.remove(listener);
		}
	}
	
	@SuppressWarnings({ "rawtypes" })
	public BasicModel()  {
		status=Status.New;
		DatabaseType type=new DatabaseType(this.getClass());
		try{
			List<FieldInfo> fields=type.getFields();
			for(FieldInfo field:fields){
				if(!field.isSimpleField()){
					field.getField().set(this,field.getField().getType().newInstance());
				}else{
					if(!field.isNull()){
						int typeId=FIELD_TYPES.getFieldType(field.getType());
						switch (typeId) {
						case FIELD_TYPES.DATE:
							field.getField().set(this,new Date());
							break;
						case FIELD_TYPES.DOUBLE:
							field.getField().set(this,0.0);
							break;
						case FIELD_TYPES.FLOAT:
							field.getField().set(this,0.0F);
							break;
						case FIELD_TYPES.INT:
							field.getField().set(this,0);
							break;
						case FIELD_TYPES.STRING:
							field.getField().set(this,"");
							break;
						case FIELD_TYPES.BIG_DECIMAL:
							field.getField().set(this,new BigDecimal(0));
							break;
							
						case FIELD_TYPES.BYTE_ARRAY:
							field.getField().set(this,new byte[]{});
							break;
						case FIELD_TYPES.SHORT:
							field.getField().set(this,(short)0);
							break;
						case FIELD_TYPES.LONG:
							field.getField().set(this,(long)0);
							break;
						default:
							break;
						}
					}
				}
			}
			
			List<RelationshipInfo> oneToMany=type.getRelationshipsOneToMany();
			for(RelationshipInfo field:oneToMany){
				Return ret=(Return)field.getField().getType().newInstance();
				field.getField().set(this,ret);
			}
			
			List<RelationshipInfo> manyToMany=type.getRelationshipsManyToMany();
			for(RelationshipInfo field:manyToMany){
				Return ret=(Return)field.getField().getType().newInstance();
				field.getField().set(this,ret);
			}
			
			List<RelationshipInfo> oneToOne=type.getRelationshipsOneToOne();
			for(RelationshipInfo field:oneToOne){
				field.getField().set(this,field.getField().getType().newInstance());
			}
		}catch(Exception err){
			err.printStackTrace();
		}
	}

	@Field(Name="id",Size=10,Type=Field.IDENTITY | Field.PRIMARY_KEY)
	private int ID;
	public int getID(){
		return ID;
	}
	/*public void setID(int value){
		this.ID=value;
	}*/
	
	private Status status;
	public Status getStatus() {
		return status;
	}
	
	public void setStatus(Status status) {
		Status oldStatus = this.status;
		this.status = status;
		if(modelListeners != null) {
			for(ModelListener listener : modelListeners) {
				listener.statusChange(this, oldStatus);
			}
		}		
	}
	
	@SuppressWarnings({ "unchecked", "rawtypes" })
	public void setValue(String fieldName,Object value) throws IllegalArgumentException, IllegalAccessException{
		DatabaseType type=new DatabaseType(this.getClass());
		FieldInfo info=type.getFieldInfo(fieldName);
		if(info!=null){
			if(info.isSimpleField()){
				info.getField().set(this, value);
			}else{
				Return ret=(Return)info.getField().get(this);
				ret.setValue(value);
				info.getField().set(this, ret);
			}
		}else{
			RelationshipInfo relInfo=type.getRelationshipInfo(fieldName);
			if(relInfo!=null){
				Return obj=(Return)relInfo.getField().get(this);
				obj.setValue(value);
			}else{
				throw new IllegalArgumentException("Campo "+fieldName+" n�o pertence a classe "+this.getClass().getName()+".");
			}
		}
		onChangeField(fieldName);
	}
	
	public Object getValue(String fieldName) throws IllegalArgumentException, IllegalAccessException{
		DatabaseType type=new DatabaseType(this.getClass());
		FieldInfo info=type.getFieldInfo(fieldName);
		if(info!=null){
			java.lang.reflect.Field field=info.getField();			
			if(field!=null){
				Object obj=field.get(this);
				if(obj instanceof Return<?>){
					return ((Return<?>)obj).getValue();
				}else{
					return obj;
				}
			}
		}else{
			RelationshipInfo relInfo=type.getRelationshipInfo(fieldName);
			if(relInfo!=null){
				java.lang.reflect.Field field=relInfo.getField();			
				if(field!=null){
					Object obj=field.get(this);
					if(obj instanceof Return<?>){
						return ((Return<?>)obj).getValue();
					}else{
						return obj;
					}
				}
			}
		}
			
		
		return null;
	}
	
	public String getValueByString(String fieldName) throws IllegalArgumentException, IllegalAccessException{
		Object value=getValue(fieldName);
		if(value==null){
			return null;
		}
		switch (FIELD_TYPES.getFieldType(value.getClass())) {
		case FIELD_TYPES.STRING:
			return (String)value;
		case FIELD_TYPES.INT:
			return value.toString();
		case FIELD_TYPES.BIG_DECIMAL:
		case FIELD_TYPES.FLOAT:
		case FIELD_TYPES.DOUBLE:{
			DecimalFormat df = new DecimalFormat("##.######",DecimalFormatSymbols.getInstance( Locale.ENGLISH ));
			return df.format(value);
		}
		case FIELD_TYPES.DATE:
			SimpleDateFormat dt = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss");
	        return dt.format((Date)value);
		case FIELD_TYPES.BYTE_ARRAY:
			return "<![CDATA[" + DatatypeConverter.printBase64Binary((byte[]) value) + "]]>";
		case FIELD_TYPES.SHORT:
			return value.toString();
		case FIELD_TYPES.LONG:
			return value.toString();
		}
		return null;
	}
	
	public void setValueByString(String fieldName,String value) throws IllegalArgumentException, IllegalAccessException{
		if(value==null){
			return;
		}
		DatabaseType type=new DatabaseType(this.getClass());
		FieldInfo info=type.getFieldInfo(fieldName);
		if(info==null){
			throw new IllegalArgumentException("Campo "+fieldName+" n�o pertence a classe "+this.getClass().getName()+".");
		}
		Class<?> clazz=info.getType();
		switch (FIELD_TYPES.getFieldType(clazz)) {
		case FIELD_TYPES.STRING:
			setValue(fieldName, value);
			break;
		case FIELD_TYPES.INT:
			setValue(fieldName, Integer.parseInt(value));
			break;
		case FIELD_TYPES.DOUBLE:{
			DecimalFormat df = new DecimalFormat("##.######",DecimalFormatSymbols.getInstance( Locale.ENGLISH ));
			try {
				setValue(fieldName, df.parse(value).doubleValue());
			} catch (ParseException e) {
				e.printStackTrace();
			}
		}
			break;
		case FIELD_TYPES.FLOAT:{
			DecimalFormat df = new DecimalFormat("##.######",DecimalFormatSymbols.getInstance( Locale.ENGLISH ));
			try {
				setValue(fieldName, df.parse(value).floatValue());
			} catch (ParseException e) {
				e.printStackTrace();
			}
		}
			break;
		case FIELD_TYPES.DATE:
			SimpleDateFormat dt = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss");
			try{
			setValue(fieldName, dt.parse(value));
			}catch (Exception e) {
				e.printStackTrace();
				setValue(fieldName, null);
			}
	        break;
		case FIELD_TYPES.BIG_DECIMAL:{
			DecimalFormat df = new DecimalFormat("#,##0.0#",DecimalFormatSymbols.getInstance( Locale.ENGLISH ));
			df.setParseBigDecimal(true);

			// parse the string
			BigDecimal bigDecimal=null;
			try {
				bigDecimal = (BigDecimal) df.parse(value);
			} catch (ParseException e) {
				e.printStackTrace();
			}
			setValue(fieldName, bigDecimal);
		}
			break;
		
		case FIELD_TYPES.BYTE_ARRAY:
			if(value.length() > 12) {
				value = value.substring(9, value.length() - 3);
				setValue(fieldName, DatatypeConverter.parseBase64Binary(value));
			}else{
				setValue(fieldName, null);
			}
			
			break;
		case FIELD_TYPES.SHORT:
			setValue(fieldName, Short.parseShort(value));
			break;
		case FIELD_TYPES.LONG:
			setValue(fieldName, Long.parseLong(value));
			break;
		}
	}
	public void delete(){
		setStatus(Status.Delete);
	}
	
	public void onChangeField(String fieldName){
		if(status==Status.Normal){
			setStatus(Status.Update);
		}
		if(modelListeners != null) {
			for(ModelListener listener : modelListeners) {
				listener.propertyChange(this, fieldName);
			}
		}
	}
	
	public abstract void onLoaded();
	public abstract void onStartLoad();
	public abstract void onStartSaving();
	public abstract void onFinishSaving();
	public void onStartDeserialize(){
		
	}
	public abstract void onFinishDeserialize();
	
	@Override
	public abstract String toString();
	
	@Override
	public boolean equals(Object obj){
		if(obj==null){
			return false;
		}
		if(this.getClass().isAssignableFrom(obj.getClass())){
			BasicModel comp=(BasicModel)obj;
			if(this.getID()==0 && comp.getID()==0){
				return this.hashCode()==comp.hashCode();
			}
			
			return this.getID()==((BasicModel)obj).getID();
		}
		return false;
	}
	
	public DatabaseType getType(){
		return new DatabaseType(getClass());
	}
	
	@SuppressWarnings("unchecked")
	public String[] isValid(){
		DatabaseType type=new DatabaseType(this.getClass());
		ArrayList<String> errors=new ArrayList<String>();
		List<FieldInfo> fields=type.getFields();
		for(FieldInfo field:fields){
			if(!field.isRequired()){
				if(!field.isSimpleField()){
					try{
						if(!((Return<?>)field.getField().get(this)).hasValue()){
							errors.add(field.getName());
						}else{
							int typeId=FIELD_TYPES.getFieldType(field.getType());
							switch (typeId) {
							case FIELD_TYPES.STRING:
								try{
									Return<String> ret=(Return<String>)field.getField().get(this);
									if(ret.getValue().equals("")){
										errors.add(field.getName());
									}
								}catch(Exception err){
									continue;
								}
								break;							
							default:
								break;
							}
						}
					}catch( Exception err){
						continue;
					}
				}else{
					if(field.isNull()){
						errors.add(field.getName());
					}else{
						int typeId=FIELD_TYPES.getFieldType(field.getType());
						switch (typeId) {
						case FIELD_TYPES.STRING:
							try{
								if(field.getField().get(this).equals("")){
									errors.add(field.getName());
								}
							}catch(Exception err){
								continue;
							}
							break;							
						default:
							break;
						}
					}
				}
			}
		}
		return errors.toArray(new String[errors.size()]);
	}
}
