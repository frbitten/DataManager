package databaseManager.connection;

import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.IOException;
import java.io.StringWriter;
import java.io.UnsupportedEncodingException;
import java.math.BigDecimal;
import java.nio.charset.Charset;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.sql.CallableStatement;
import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.sql.Statement;
import java.sql.Timestamp;
import java.util.ArrayList;
import java.util.Date;
import java.util.HashMap;
import java.util.List;
import java.util.zip.GZIPOutputStream;

import org.kxml2.io.KXmlSerializer;
import org.xmlpull.v1.XmlSerializer;

import databaseManager.BasicModel;
import databaseManager.FIELD_TYPES;
import databaseManager.Return;
import databaseManager.BasicModel.Status;
import databaseManager.SQL.BasicScript;
import databaseManager.SQL.DontRelationsScript;
import databaseManager.SQL.LoadConfig;
import databaseManager.SQL.LoadScript;
import databaseManager.SQL.OrderBy;
import databaseManager.SQL.Select;
import databaseManager.SQL.Where;
import databaseManager.SQL.Where.Operator;
import databaseManager.helpers.ConvertHelper;
import databaseManager.helpers.SerializerHelper;
import databaseManager.type.DatabaseType;
import databaseManager.type.FieldInfo;
import databaseManager.type.FileInfo;
import databaseManager.type.RelationshipInfo;

public abstract class DataBaseConnection implements AutoCloseable {

	public enum DATABASE_TYPE {
		MYSQL, POSTGREE, SQLITE
	}

	protected DATABASE_TYPE type;
	protected String server;
	protected String user;
	protected String password;
	protected String database;
	protected String filesDirectory;
	protected Connection connection;
	protected HashMap<Class<?>,HashMap<Integer, BasicModel>> caches;
	protected HashMap<Class<?>,HashMap<Integer,StringWriter>> serializedCaches;

	public DataBaseConnection(DATABASE_TYPE type, String server,String database, String user, String password,String filesDirectory) {
		this.type = type;
		this.server = server;
		this.user = user;
		this.password = password;
		this.database = database;
		this.connection = null;
		this.filesDirectory=filesDirectory;
		caches=new HashMap<Class<?>,HashMap<Integer, BasicModel>>();
		serializedCaches=new HashMap<Class<?>,HashMap<Integer,StringWriter>>();
	}

	public DATABASE_TYPE getType() {
		return type;
	}

	public String getServer() {
		return server;
	}

	public String getUser() {
		return user;
	}

	public String getPassword() {
		return password;
	}

	public String getDatabase() {
		return database;
	}

	
	public void open() throws SQLException {
		switch (getType()) {
		case MYSQL:
			connection = DriverManager.getConnection("jdbc:mysql://"
					+ getServer() + "/" + getDatabase(), getUser(),
					getPassword());
			break;
			
		case POSTGREE:
			connection = DriverManager.getConnection("jdbc:postgresql://"
					+ getServer() + "/" + getDatabase(), getUser(),
					getPassword());
			break;
			
		case SQLITE:
			connection = DriverManager.getConnection("jdbc:sqlite:" + getDatabase() + ".db");
			break;
			
		default:
			break;
		}
		connection.setAutoCommit(false);
	}

	public boolean isConnected() {
		if (connection == null) {
			return false;
		}
		try {
			return !connection.isClosed();
		} catch (SQLException e) {
			return false;
		}
	}
	
	public void commit() throws SQLException{
		if(this.connection!=null){
			this.connection.commit();
		}
	}
	public void Rollback() throws SQLException{
		if(this.connection!=null){
			this.connection.rollback();
		}
	}

	@Override
	public void close() throws SQLException {
		connection.close();
	}

	public <T extends BasicModel> List<T> getItems(Class<T> clazz)
			throws SQLException {
		return getItems(clazz, null, null,null);
	}
	public <T extends BasicModel> List<T> getItems(Class<T> clazz,LoadConfig config)
			throws SQLException {
		return getItems(clazz, null, null,config);
	}
	public <T extends BasicModel> List<T> getItems(Class<T> clazz,Where where)
			throws SQLException {
		return getItems(clazz, where, null,null);
	}
	public <T extends BasicModel> List<T> getItems(Class<T> clazz,Where where,LoadConfig config)
			throws SQLException {
		return getItems(clazz, where, null,config);
	}

	public <T extends BasicModel> List<byte[]> getItemsSerialized(Class<T> clazz, Where where,OrderBy orderBy,LoadConfig config) throws SQLException, IOException {
		List<String> list=getItemsSerializedToXML(clazz,where,orderBy,config);
		List<byte[]> ret=new ArrayList<byte[]>();
		for(String xml:list){
			ByteArrayOutputStream outputStream = new ByteArrayOutputStream();
			GZIPOutputStream gzipOutputStream = new GZIPOutputStream(outputStream);
            gzipOutputStream.write(xml.getBytes("UTF-8"));
            gzipOutputStream.close();
            ret.add(outputStream.toByteArray());
		}
		return ret;
	}
	
	public <T extends BasicModel> List<String> getItemsSerializedToXML(Class<T> clazz, Where where,OrderBy orderBy,LoadConfig config) throws SQLException {
		DatabaseType type = new DatabaseType(clazz);
		Select select = null;
		select = new Select(type);
		select.setOrderBy(orderBy);
		select.addWhere(where, this);
		String sql = select.toString();
		
		List<Object> paramenters = select.getParameters();
		PreparedStatement stm = this.connection.prepareStatement(sql);
		if (paramenters.size() > 0) {
			for (int i = 0; i < paramenters.size(); i++) {
				Object item = paramenters.get(i);
				addParameter(stm, i+1, convertToDbType(item));
			}
		} 
		LoadScript script=null;
		if(config!=null && config.getType()==LoadConfig.TYPE_SCRIPT){
			script=new LoadScript(new String(config.getData()));
		}
		ResultSet result= stm.executeQuery();
		
		List<String> ret = new ArrayList<String>();
		while (result.next())
        {
        	try{
    		    StringBuilder xml=new StringBuilder("<?xml version='1.0' encoding='UTF-8' standalone='yes' ?>");
    		    xml.append("<serializer>");
    			StringWriter modelXmlSerializer=serializeModel(result, clazz, script);
    		    String model=modelXmlSerializer.toString();
    		    model=model.substring(model.indexOf("?>")+2);
    			xml.append(model);
    			xml.append("<"+SerializerHelper.TAG_CHILDREN+">");
    	    	for (HashMap<Integer,StringWriter> map:serializedCaches.values()) {
					for(StringWriter child: map.values()){
						if(child==modelXmlSerializer){
							continue;
						}
						model=child.toString();
						model=model.substring(model.indexOf("?>")+2);
						xml.append(model);
					}
				}
    	    	xml.append("</"+SerializerHelper.TAG_CHILDREN+">");
    	    	xml.append("</serializer>");
    		    ret.add(xml.toString());
    		    serializedCaches.clear();
    		}catch(Exception err){
    			err.printStackTrace();
    		}
        }
		result.close();
		this.connection.commit();
		return ret;
	}
	
	@SuppressWarnings("unchecked")
	protected <T extends BasicModel> StringWriter serializeModel(ResultSet data, Class<T> clazz,LoadScript script)
    {
		try{
			int id=(int)convertToJavaType(data,"id",FIELD_TYPES.INT);
			DatabaseType type = new DatabaseType(clazz);
			XmlSerializer xmlSerializer = new KXmlSerializer();
			StringWriter writer = new StringWriter();
			addCache(clazz, id, writer);
		    xmlSerializer.setOutput(writer);
		    xmlSerializer.startDocument("UTF-8", true);
			xmlSerializer.startTag("", SerializerHelper.TAG_MODEL);
		    xmlSerializer.startTag("", SerializerHelper.TAG_CLASS);
		    xmlSerializer.text(clazz.getName());
		    xmlSerializer.endTag("", SerializerHelper.TAG_CLASS);
		    xmlSerializer.startTag("", SerializerHelper.TAG_ID);
		    xmlSerializer.text(String.valueOf(writer.hashCode()));
		    xmlSerializer.endTag("", SerializerHelper.TAG_ID);
		    xmlSerializer.startTag("", SerializerHelper.TAG_FIELDS);
		    for(FieldInfo info:type.getFields()){
		    	Object value = convertToJavaType(data,info.getName(),FIELD_TYPES.getFieldType(info.getType()));
	            if (value != null)
	            {
	            	String valueText=ConvertHelper.toString(value);
	            	if(info.isSimpleField()){
	            		xmlSerializer.startTag("", info.getName());
				    	xmlSerializer.text(valueText);
				    	xmlSerializer.endTag("", info.getName());
	            	}else{
	            		if(script==null || script.isLoadField(clazz, info.getName())){
	            			xmlSerializer.startTag("", info.getName());
					    	xmlSerializer.text(valueText);
					    	xmlSerializer.endTag("", info.getName());
	            		}
	            	}
	            }
		    }
		    xmlSerializer.startTag("", SerializerHelper.TAG_STATUS);	    	
			xmlSerializer.text("normal");
	    	xmlSerializer.endTag("", SerializerHelper.TAG_STATUS);
	    	xmlSerializer.endTag("", SerializerHelper.TAG_FIELDS);
	    	xmlSerializer.startTag("", SerializerHelper.TAG_RELATIONSHIP_ONE_TO_ONE);
	    	for(RelationshipInfo info:type.getRelationshipsOneToOne()){
	    		if(script==null || script.isLoadField(clazz, info.getName())){
	    			int relaId=(int)convertToJavaType(data,info.getFieldName(),FIELD_TYPES.INT);
	    			StringWriter xml=getSerializedCache(info.getType(), relaId);
    				if(xml==null){
    					DatabaseType childType=new DatabaseType(info.getType());
    					Where where=new Where();
    					where.addField(info.getType(), "id");
    					where.addOperator(Operator.EQUAL);
    					where.addValue(relaId);
    	    			Select select = null;
    	    			select = new Select(childType);
    	    			select.addWhere(where, this);
    	    			String sql = select.toString();
    	    			
    	    			List<Object> paramenters = select.getParameters();
    	    			PreparedStatement stm = this.connection.prepareStatement(sql);
    	    			if (paramenters.size() > 0) {
    	    				for (int i = 0; i < paramenters.size(); i++) {
    	    					Object item = paramenters.get(i);
    	    					addParameter(stm, i+1, convertToDbType(item));
    	    				}
    	    			}
    	    			ResultSet result= stm.executeQuery();
    	    			if(!result.next()){
    	    				continue;
    	    			}
    	    			xml=serializeModel(result, info.getType(), script);
    	    			result.close();
    				}
    				xmlSerializer.startTag("", info.getName());
			    	xmlSerializer.startTag("", SerializerHelper.TAG_CLASS);
			    	xmlSerializer.text(info.getType().getName());
			    	xmlSerializer.endTag("", SerializerHelper.TAG_CLASS);
			    	xmlSerializer.startTag("", SerializerHelper.TAG_FIELD_NAME);
			    	xmlSerializer.text(info.getFieldName());
			    	xmlSerializer.endTag("", SerializerHelper.TAG_FIELD_NAME);
			    	xmlSerializer.startTag("", SerializerHelper.TAG_ID);	    	
			    	xmlSerializer.text(String.valueOf(xml.hashCode()));
			    	xmlSerializer.endTag("", SerializerHelper.TAG_ID);
			    	xmlSerializer.endTag("", info.getName());
	    		}
	    	}
	    	xmlSerializer.endTag("", SerializerHelper.TAG_RELATIONSHIP_ONE_TO_ONE);
	    	xmlSerializer.startTag("", SerializerHelper.TAG_RELATIONSHIP_ONE_TO_MANY);
	    	for(RelationshipInfo info:type.getRelationshipsOneToMany()){
	    		if(script==null || script.isLoadField(clazz, info.getName())){
	    			DatabaseType childType=new DatabaseType(info.getGenericClass());
	    			Where where=new Where();
					where.addField(info.getGenericClass(), info.getFieldName());
					where.addOperator(Operator.EQUAL);
					where.addValue(id);
	    			Select select = null;
	    			select = new Select(childType);
	    			select.addWhere(where, this);
	    			String sql = select.toString();
	    			
	    			List<Object> paramenters = select.getParameters();
	    			PreparedStatement stm = this.connection.prepareStatement(sql);
	    			if (paramenters.size() > 0) {
	    				for (int i = 0; i < paramenters.size(); i++) {
	    					Object item = paramenters.get(i);
	    					addParameter(stm, i+1, item);
	    				}
	    			} 
	    			
	    			ResultSet result= stm.executeQuery();
	    			xmlSerializer.startTag("", info.getName());
	    			while (result.next())
	    	        {
	    				int childId=(int)convertToJavaType(data,"id",FIELD_TYPES.INT);
	    				StringWriter xml=getSerializedCache(info.getGenericClass(), childId);
	    				if(xml==null){
	    					xml=serializeModel(result, info.getGenericClass(), script);
	    				}
	    				xmlSerializer.startTag("", SerializerHelper.TAG_RELATIONSHIP_ITEM);
    			    	xmlSerializer.startTag("", SerializerHelper.TAG_CLASS);
    			    	xmlSerializer.text(info.getGenericClass().getName());
    			    	xmlSerializer.endTag("", SerializerHelper.TAG_CLASS);
    			    	xmlSerializer.startTag("", SerializerHelper.TAG_FIELD_NAME);
    			    	xmlSerializer.text(info.getFieldName());
    			    	xmlSerializer.endTag("", SerializerHelper.TAG_FIELD_NAME);
    			    	xmlSerializer.startTag("", SerializerHelper.TAG_ID);	    	
    			    	xmlSerializer.text(String.valueOf(xml.hashCode()));
    			    	xmlSerializer.endTag("", SerializerHelper.TAG_ID);
    			    	xmlSerializer.endTag("", SerializerHelper.TAG_RELATIONSHIP_ITEM);
	    	        }
	    			xmlSerializer.endTag("", info.getName());
	    			result.close();
	    		}
	    	}
	    	xmlSerializer.endTag("", SerializerHelper.TAG_RELATIONSHIP_ONE_TO_MANY);
	    	xmlSerializer.endTag("", SerializerHelper.TAG_MODEL);
	    	return writer;
		}catch(Exception err){
			err.printStackTrace();
			return null;
		}
    }

	public <T extends BasicModel> List<T> getItems(Class<T> clazz, Where where,OrderBy orderBy,LoadConfig config) throws SQLException {
		BasicScript script=null;
		if(config!=null && config.getType()==LoadConfig.TYPE_SCRIPT){
			script=new LoadScript(new String(config.getData()));
		}
		if(config!=null && config.getType()==LoadConfig.TYPE_DONT_RELATIONS){
			script=new DontRelationsScript();
		}
		List<T> ret=getItemsInternal(clazz,where,orderBy,0,0,script);
		caches.clear();
		return ret;
	}
	
	public <T extends BasicModel> List<T> getItems(Class<T> clazz, Where where,OrderBy orderBy,int limitStart,int limitLength,LoadConfig config) throws SQLException {
		BasicScript script=null;
		if(config!=null && config.getType()==LoadConfig.TYPE_SCRIPT){
			script=new LoadScript(new String(config.getData()));
		}
		if(config!=null && config.getType()==LoadConfig.TYPE_DONT_RELATIONS){
			script=new DontRelationsScript();
		}
		
		List<T> ret=getItemsInternal(clazz,where,orderBy,limitStart,limitLength,script);
		caches.clear();
		return ret;
	}
	
	@SuppressWarnings("unchecked")
	protected <T extends BasicModel> List<T> getItemsInternal(Class<T> clazz, Where where,OrderBy orderBy,int limitStart,int limitLength,BasicScript script) throws SQLException {
		DatabaseType type = new DatabaseType(clazz);
		Select select = null;
		select = new Select(type);
		select.setOrderBy(orderBy);
		select.addWhere(where, this);
		if (limitStart >= 0 && limitLength > 0)
        {
			select.setLimitStart(limitStart);
			select.setLimitLength(limitLength);
		}
		
		String sql = select.toString();
		
		List<Object> paramenters = select.getParameters();
		PreparedStatement stm = this.connection.prepareStatement(sql);
		if (paramenters.size() > 0) {
			for (int i = 0; i < paramenters.size(); i++) {
				Object item = paramenters.get(i);
				addParameter(stm, i+1, convertToDbType(item));
			}
		} 
		ResultSet result= stm.executeQuery();
		
		List<T> ret = new ArrayList<T>();
		while (result.next())
        {
        	BasicModel model=getCacheModel(clazz,result.getInt("ID"));
            if (model==null)
            {
                model = createModel(result, clazz,script);
                addCache(model);
                
                //Carregar arquivos do objeto
                for (FileInfo field: type.getFiles()){
                	try{
	                	String namefile=filesDirectory+"/"+field.getOwnerClazz().getName()+"/"+model.getID()+"/";
	            		File file=new File(namefile);
	            		if(!file.exists()){
	            			continue;
	            		}
	            		namefile+="/"+field.getName()+"."+field.getExtension();
	            		file=new File(namefile);
	            		if(!file.exists()){
	            			continue;
	            		}
	            		byte[] buff=null;
	            		try {
	    					buff=Files.readAllBytes(Paths.get(namefile));
	    				} catch (IOException e) {
	    					e.printStackTrace();
	    					throw new IllegalArgumentException("N�o foi possivel ler o arquivo "+namefile);
	    				}
	            		if(buff==null){
	            			continue;
	            		}
	        			
	        			if(field.isSimpleField()){
	                		if(String.class.isAssignableFrom(field.getType())){
	                    		String value=new String(buff, Charset.forName("UTF8"));
	                    		field.getField().set(model,value);
	                    	}else{
	                    		field.getField().set(model,buff);
	                    	}
	                	}else{
	        				Return<Object> value=(Return<Object>)field.getField().get(model);
	                        if(value!=null ){
	                        	if(String.class.isAssignableFrom(field.getType())){
	                        		String aux=new String(buff, Charset.forName("UTF8"));
	                        		value.setValue(aux);
	                        	}else{
	                        		value.setValue(buff);
	                        	}
	                        }
	                        field.getField().set(model,value);
	                	}
                	}catch(Exception err){
            			err.printStackTrace();
            			continue;
            		}
        		}                
            }
            ret.add((T)model);
        }
		result.close();
		this.connection.commit();
		if(script==null || script instanceof LoadScript){

	        if (type.getRelationshipsOneToMany().size() > 0 || type.getRelationshipsOneToOne().size()>0 || type.getRelationshipsManyToMany().size()>0)
	        {
	            processRelationships(ret, type,script);
	        }
		}

        for (BasicModel item: ret)
        {
            item.setStatus(Status.Normal);
            item.onLoaded();
        }
		return ret;
	}

	private void addParameter(PreparedStatement stm, int i, Object item) throws SQLException {
		if (item instanceof String) {
			stm.setString(i, (String) item);
		}
		if (int.class.isAssignableFrom(item.getClass())) {
			stm.setInt(i, (int) item);
		}
		if (Integer.class.isAssignableFrom(item.getClass())) {
			stm.setInt(i, (Integer) item);
		}		
		if (item instanceof Date) {
			stm.setTimestamp(i, new Timestamp(((Date) item).getTime()));
		}
		if (float.class.isAssignableFrom(item.getClass())) {
			stm.setFloat(i, (float) item);
		}
		if (Float.class.isAssignableFrom(item.getClass())) {
			stm.setFloat(i, (Float) item);
		}
		if (double.class.isAssignableFrom(item.getClass())) {
			stm.setDouble(i, (double) item);
		}
		if (Double.class.isAssignableFrom(item.getClass())) {
			stm.setDouble(i, (Double) item);
		}
		if(BigDecimal.class.isAssignableFrom(item.getClass())){
			stm.setBigDecimal(i, (BigDecimal)item);
		}
		if(Byte[].class.isAssignableFrom(item.getClass()) || byte[].class.isAssignableFrom(item.getClass())) {
			stm.setBytes(i, (byte[])item);
		}
		if(short.class.isAssignableFrom(item.getClass())){
			stm.setShort(i,(short)item);
		}
		if(Short.class.isAssignableFrom(item.getClass())){
			stm.setShort(i,(short)item);
		}
		if(Long.class.isAssignableFrom(item.getClass())){
			stm.setLong(i, (long)item);
		}
		if(long.class.isAssignableFrom(item.getClass())){
			stm.setLong(i, (long)item);
		}
	}
	
	@SuppressWarnings({ "rawtypes", "unchecked" })
	private <T extends BasicModel> void processRelationships(List<T> models, DatabaseType type,BasicScript script) {
		
		for(T model:models){
			for (RelationshipInfo field: type.getRelationshipsOneToMany())
	        {
				try {
					if(script==null || (script instanceof LoadScript  && ((LoadScript)script).isLoadField(model.getClass(), field.getName()))){
						Return value=(Return)field.getField().get(model);
						if(value.hasValue()){
							continue;
						}
						Where where=new Where();
						where.addField(field.getGenericClass(), field.getFieldName());
						where.addOperator(Operator.EQUAL);
						where.addValue(model.getID());
						List<BasicModel> values=getItemsInternal(field.getGenericClass(), where,null,0,0,script);
						value.setValue(values);
						field.getField().set(model, value);
					}
				} catch(Exception e){
					e.printStackTrace();
					continue;
				}
	        }
			
			for (RelationshipInfo field: type.getRelationshipsOneToOne())
	        {
				try {
					if(script==null || (script instanceof LoadScript  && ((LoadScript)script).isLoadField(model.getClass(), field.getName()))){
						Return value=(Return)field.getField().get(model);
						if(value.hasValue()){
							continue;
						}
						String fieldName=field.getFieldName();
						FieldInfo info= type.getFieldInfo(fieldName);
						int id=0;
						if(info.isSimpleField()){
							try {
								id=(int)info.getField().get(model);
							} catch (Exception e) {
								e.printStackTrace();
							}
						}else{
							try {
								Return ret=(Return)info.getField().get(model);
								id=(int)ret.getValue();
							} catch (Exception e) {
								e.printStackTrace();
							}
						}				
						
						if(id>0){
							BasicModel child=getCacheModel(field.getType(),id);
							if(child==null){
								Where where = new Where();
						        where.addField(field.getType(), "ID");
						        where.addOperator(Operator.EQUAL);
						        where.addValue(id);
								List<BasicModel> find= getItemsInternal(field.getType(), where, null,0,0, script);
								if(find.size()>0){
									value.setValue(find.get(0));
								}else{
									value.setValue(null);
								}
							}else{
								value.setValue(child);
							}
						}else{
							value.setValue(null);
						}
						field.getField().set(model, value);
					}
				} catch (Exception e) {
					e.printStackTrace();
					continue;
				}
	        }
		}
	}

	@SuppressWarnings({ "unchecked", "rawtypes" })
	protected <T extends BasicModel> T createModel(ResultSet data, Class<T> clazz,BasicScript script)
    {
		try {
			T model = clazz.newInstance();
			model.onStartLoad();
	        DatabaseType databaseType = new DatabaseType(clazz);
	        for (FieldInfo field: databaseType.getFields())
	        {
	        	Object value = convertToJavaType(data,field.getName(),FIELD_TYPES.getFieldType(field.getType()));
	        	if (value != null)
	            {
	            	if(field.isSimpleField()){
	            		field.getField().set(model,value);
	            	}else{
	            		if(script==null || script instanceof DontRelationsScript || (script instanceof LoadScript && ((LoadScript)script).isLoadField(clazz, field.getName()))){
		            		Return ret=(Return)field.getField().get(model);
			            	ret.setValue(value);
			            	field.getField().set(model,ret);
	            		}
	            	}
	            }
	        	
	        }
	        return model;
		} catch (Exception e) {
			e.printStackTrace();
			return null;
		} 
        

    }
	

	public <T extends BasicModel> boolean saveItem(T model) throws SQLException, IllegalArgumentException, IllegalAccessException
    {
		return saveItem(model,true);
    }
	
	public <T extends BasicModel> boolean saveItem(T model,boolean commit) throws SQLException, IllegalArgumentException, IllegalAccessException
    {
		boolean ret=internalSaveItem(model);
		if(!ret){
			this.connection.rollback();
		}else{
			if(commit){
				this.connection.commit();
			}
		}
		return ret;
    }
	
	@SuppressWarnings("rawtypes")
	protected <T extends BasicModel> boolean internalSaveItem(T model) throws IllegalArgumentException, IllegalAccessException, SQLException
    {
        DatabaseType databaseType = new DatabaseType(model.getClass());
        boolean ret = false;
        model.onStartSaving();
        
        if(model.getStatus()!=Status.Delete && model.getStatus()!=Status.Normal){
	        List<RelationshipInfo> oneToOne=databaseType.getRelationshipsOneToOne();
	        if(oneToOne.size()>0){
	        	for(RelationshipInfo info:oneToOne){
	            	Return aux= (Return)info.getField().get(model);
	            	if(aux.hasValue()){
		            	BasicModel oneToOneModel=(BasicModel)aux.getValue();
		            	if(oneToOneModel!=null){
			            	Status status=oneToOneModel.getStatus();
			        		if(status!=Status.Normal){
			        			internalSaveItem((BasicModel)oneToOneModel);
			        			if(status==Status.New){
			        				model.setValue(info.getFieldName(), oneToOneModel.getID());
			        			}
			        			if(status==Status.Delete){
			        				model.setValue(info.getFieldName(), 0);
			        			}
			        		}  
		            	}
	            	}
	            }
	        }
        }
        
        
        switch (model.getStatus())
        {
            case Delete:
                ret = deleteItem(model);
                model.onFinishSaving();
                return ret;
            case New:
                ret = insertItem(model);
                break;
            case Update:
                ret = updateItem(model);
                break;
            case Normal:
            	model.onFinishSaving();
                return true;
            case Invalid:
            	model.onFinishSaving();
            	return true;
            default:
            	break;
        }

        //salva rela��es 1 para muitos
        if (ret)
        {
            List<RelationshipInfo> oneToMany=databaseType.getRelationshipsOneToMany();
            for(RelationshipInfo info:oneToMany){
            	Return aux= (Return)info.getField().get(model);
            	if(aux.hasValue()){
	            	List list=(List)aux.getValue();
	            	List<Object> removed=new ArrayList<Object>();
	            	for(Object child:list){
	            		BasicModel childModel=(BasicModel)child;
	            		if(childModel.getStatus()!=Status.Normal){
	            			childModel.onStartSaving();
	            			childModel.setValue(info.getFieldName(), model.getID());
	            			internalSaveItem((BasicModel)child);
	            			if(((BasicModel)child).getStatus()==Status.Invalid){
	            				removed.add(child);
	            			}
	            		}
	            	}
	            	for(Object obj:removed){
	            		list.remove(obj);
	            	}
            	}
            }
        }
        model.onFinishSaving();
        return ret;
    }
	
	@SuppressWarnings("unchecked")
	protected boolean insertItem(BasicModel model) throws SQLException, IllegalArgumentException, IllegalAccessException{
		DatabaseType type = new DatabaseType(model.getClass());
		
		FieldInfo identity=null;
		List<FieldInfo> fields=type.getFields();
		if(fields.size()<=1){
			throw new IllegalArgumentException("Model invalid format. Requires at least one field beyond the ID");
		}
		List<Object> values=new ArrayList<Object>();
        String listFields="";
        String tagValues="";
        for (FieldInfo field: type.getFields()){
            if (!field.isIdentity())
            {
                if(field.isSimpleField()){
                	if(String.class.isAssignableFrom(field.getType())){
                		String value=(String)field.getField().get(model);
                		if(field.getSize()>0 && value.length()>field.getSize()){
                			value=value.substring(0,(int)field.getSize());
                		}
                		values.add(value);
                	}else{
                		values.add(field.getField().get(model));
                	}
                	listFields+=field.getName()+", ";
                	tagValues+="?, ";
                }else{
					Return<Object> value=(Return<Object>)field.getField().get(model);
	                if(value!=null && value.hasValue()){
	                	if(String.class.isAssignableFrom(field.getType())){
	                		String aux=(String) value.getValue();
	                		if(field.getSize()>0 && aux.length()>field.getSize()){
	                			aux=aux.substring(0,(int)field.getSize());
	                		}
	                		values.add(aux);
	                	}else{
	                		values.add(value.getValue());
	                	}
	                	
	                	listFields+=field.getName()+", ";
	                	tagValues+="?, ";
	                }
                }
            }else{
            	identity=field;
            }
        }
        if(listFields.length()>2){
        	listFields = listFields.substring(0, listFields.length() - 2);
        }
        if(tagValues.length()>2){
        	tagValues = tagValues.substring(0, tagValues.length() - 2);
        }
        String sql = "insert into `" + type.getTableName() + "` ("+listFields+") values ("+tagValues+");";
        PreparedStatement stm= this.connection.prepareStatement(sql,Statement.RETURN_GENERATED_KEYS);
        for (int i = 0; i < values.size(); i++) {
        	addParameter(stm,i+1,convertToDbType(values.get(i)));
        }
       
        int ret=stm.executeUpdate();

        if (ret<0)
        {
            return false;
        }

        if (identity!=null)
        {
        	ResultSet rs = stm.getGeneratedKeys();
            if(rs.next())
            {
                int last_inserted_id = rs.getInt(1);
                identity.getField().setInt(model, last_inserted_id);
            }
            rs.close();
        }
        model.setStatus(Status.Normal);
        for (FileInfo field: type.getFiles()){
        	byte[] buff=null;
        	if(field.isSimpleField()){
        		if(String.class.isAssignableFrom(field.getType())){
            		String value=(String)field.getField().get(model);
            		if(value!=null){
	            		try {
							buff=value.getBytes("UTF-8");
						} catch (UnsupportedEncodingException e) {
							e.printStackTrace();
						}
            		}
            	}else{
            		buff=(byte[])field.getField().get(model);
            	}
        	}else{
        		Return<Object> value=(Return<Object>)field.getField().get(model);
                if(value!=null && value.hasValue()){
                	if(String.class.isAssignableFrom(field.getType())){
                		String aux=(String) value.getValue();
                		try {
    						buff=aux.getBytes("UTF-8");
    					} catch (UnsupportedEncodingException e) {
    						e.printStackTrace();
    					}
                	}else{
                		buff=(byte[])value.getValue();
                	}
                }
        	}
        	if(buff!=null){
        		String namefile=filesDirectory+"/"+field.getOwnerClazz().getName()+"/"+model.getID()+"/";
        		File file=new File(namefile);
        		if(!file.exists()){
        			if(!file.mkdirs()){
        				throw new IllegalArgumentException("N�o foi possivel criar a pasta "+namefile);
        			}
        		}
        		namefile+="/"+field.getName()+"."+field.getExtension();
        		file=new File(namefile);
        		if(file.exists()){
        			if(!file.delete()){
        				throw new IllegalArgumentException("N�o foi possivel apagar o arquivo "+namefile);
        			}
        		}
        		try {
					Files.write(Paths.get(namefile), buff);
				} catch (IOException e) {
					e.printStackTrace();
					throw new IllegalArgumentException("N�o foi possivel gravar o arquivo "+namefile);
				}
        	}
        }
        return true;
	}
	protected abstract Object convertToDbType(Object object);

	protected boolean updateItem(BasicModel model) throws IllegalArgumentException, IllegalAccessException, SQLException{
		DatabaseType type = new DatabaseType(model.getClass());
		
		List<FieldInfo> fields=type.getFields();
		if(fields.size()<=1){
			throw new IllegalArgumentException("Model invalid format. Requires at least one field beyond the ID");
		}

		List<Object> values=new ArrayList<Object>();
        String listFields="";
        for (FieldInfo field: type.getFields()){            
            if (!field.isIdentity())
            {
                if(field.isSimpleField()){
                	if(String.class.isAssignableFrom(field.getType())){
                		String value=(String)field.getField().get(model);
                		if(field.getSize()>0 && value.length()>field.getSize()){
                			value=value.substring(0,(int)field.getSize());
                		}
                		values.add(value);
                	}else{
                		values.add(field.getField().get(model));
                	}
                	listFields+=field.getName()+"=?, ";
                }else{
	            	@SuppressWarnings("unchecked")
					Return<Object> value=(Return<Object>)field.getField().get(model);
	                if(value!=null && value.hasValue()){
	                	if(String.class.isAssignableFrom(field.getType())){
	                		String aux=(String) value.getValue();
	                		if(field.getSize()>0 && aux.length()>field.getSize()){
	                			aux=aux.substring(0,(int)field.getSize());
	                		}
	                		values.add(aux);
	                	}else{
	                		values.add(value.getValue());
	                	}
	                	listFields+=field.getName()+"=?, ";
	                }
                }
            }
        }
        if(listFields.length()>2){
        	listFields = listFields.substring(0, listFields.length() - 2);
        }
        
        String sql = "update `" + type.getTableName() + "` set "+listFields+" where id="+model.getID()+";";
        PreparedStatement stm= this.connection.prepareStatement(sql,Statement.RETURN_GENERATED_KEYS);
        for (int i = 0; i < values.size(); i++) {
        	addParameter(stm,i+1,convertToDbType(values.get(i)));
        }
       
        int ret=stm.executeUpdate();

        if (ret<=0)
        {
            return false;
        }
        model.setStatus(Status.Normal);
        
        
        for (FileInfo field: type.getFiles()){
        	byte[] buff=null;
        	if(field.isSimpleField()){
        		if(String.class.isAssignableFrom(field.getType())){
            		String value=(String)field.getField().get(model);
            		try {
						buff=value.getBytes("UTF-8");
					} catch (UnsupportedEncodingException e) {
						e.printStackTrace();
					}
            	}else{
            		buff=(byte[])field.getField().get(model);
            	}
        	}else{
        		@SuppressWarnings("unchecked")
				Return<Object> value=(Return<Object>)field.getField().get(model);
                if(value!=null && value.hasValue()){
                	if(String.class.isAssignableFrom(field.getType())){
                		String aux=(String) value.getValue();
                		try {
    						buff=aux.getBytes("UTF-8");
    					} catch (UnsupportedEncodingException e) {
    						e.printStackTrace();
    					}
                	}else{
                		buff=(byte[])value.getValue();
                	}
                }
        	}
        	if(buff!=null){
        		String namefile=filesDirectory+"/"+field.getOwnerClazz().getName()+"/"+model.getID()+"/";
        		File file=new File(namefile);
        		if(!file.exists()){
        			if(!file.mkdirs()){
        				throw new IllegalArgumentException("N�o foi possivel criar a pasta "+namefile);
        			}
        		}
        		namefile+="/"+field.getName()+"."+field.getExtension();
        		file=new File(namefile);
        		if(file.exists()){
        			if(!file.delete()){
        				throw new IllegalArgumentException("N�o foi possivel apagar o arquivo "+namefile);
        			}
        		}
        		try {
					Files.write(Paths.get(namefile), buff);
				} catch (IOException e) {
					e.printStackTrace();
					throw new IllegalArgumentException("N�o foi possivel gravar o arquivo "+namefile);
				}
        	}
        }
        
        return true;
	}
	
	protected boolean deleteItem(BasicModel model) throws SQLException, IllegalArgumentException, IllegalAccessException
    {
        DatabaseType type = new DatabaseType(model.getClass());
        String sql = "delete from `" + type.getTableName() + "` where ";
        boolean aux=false;
        for (FieldInfo field: type.getFields())
        {
            if (field.isPrimaryKey())
            {
            	if(aux){
            		sql+=" and ";
            	}
                sql += field.getName() + "="+field.getField().get(model).toString();
                aux=true;
            }
        }
        sql+=";";

        Statement stmt = connection.createStatement();
        int ret = stmt.executeUpdate(sql);
        if(ret<=0){
        	return false;
        }
        
        model.setStatus(Status.Invalid);
        
        List<RelationshipInfo> oneToMany=type.getRelationshipsOneToMany();
        if(oneToMany.size()>0){
        	for(RelationshipInfo info:oneToMany){
        		DatabaseType relationType = new DatabaseType(info.getGenericClass());
        		sql = "delete  from `" + relationType.getTableName() + "` where "+info.getFieldName()+"="+model.getID()+";";
        		stmt = connection.createStatement();
                ret = stmt.executeUpdate(sql);
                if(ret<0){
                	return false;
                }
        	}
        }
        
        for (FileInfo field: type.getFiles()){
    		String namefile=filesDirectory+"/"+field.getOwnerClazz().getName()+"/"+model.getID()+"/";
    		File file=new File(namefile);
    		if(!file.exists()){
    			if(!file.mkdirs()){
    				throw new IllegalArgumentException("N�o foi possivel criar a pasta "+namefile);
    			}
    		}
    		namefile+="/"+field.getName()+"."+field.getExtension();
    		file=new File(namefile);
    		if(file.exists()){
    			if(!file.delete()){
    				throw new IllegalArgumentException("N�o foi possivel apagar o arquivo "+namefile);
    			}
    		}
        }

        return true;
    }
	
	public <T extends BasicModel>boolean saveItems(List<T> models) throws SQLException, IllegalArgumentException, IllegalAccessException
    {
        return saveItems(models,true);
    }

    public <T extends BasicModel>boolean saveItems(List<T> models,boolean commit) throws SQLException, IllegalArgumentException, IllegalAccessException
    {
        for (T item: models)
        {
            if(!internalSaveItem(item)){
            	this.connection.rollback();
            	return false;
            }
        }
        if(commit){
			this.connection.commit();
		}
        return true;
    }
    
    public<T extends BasicModel> T loadItem(Class<T> clazz, int modelId) throws SQLException
    {
    	return loadItem(clazz, modelId,null);
    }
    
    @SuppressWarnings("unchecked")
	public<T extends BasicModel> T loadItem(Class<T> clazz, int modelId,LoadConfig config) throws SQLException
    {
    	BasicModel ret=getCacheModel(clazz, modelId);
    	if(ret!=null){
    		return (T)ret;
    	}
        Where where = new Where();
        where.addField(clazz, "ID");
        where.addOperator(Operator.EQUAL);
        where.addValue(modelId);
        List<T> list= getItems(clazz, where,config);
        if (list.size() <= 0)
        {
            throw new IllegalArgumentException("Object "+clazz.getName()+", id:"+modelId+" not found in database.");
        }
        if (list.size() > 1)
        {
            throw new IllegalArgumentException("ID repeated in the database.");
        }
        return list.get(0);
    }
    
    @SuppressWarnings({ "unchecked", "rawtypes" })
	public<T extends BasicModel> boolean createTable(Class<T> clazz) throws SQLException
    {
        DatabaseType type=new DatabaseType(clazz);
        String sql=getCreateTable(clazz);
        
        Statement stm= this.connection.createStatement();
        int ret=stm.executeUpdate(sql);
        if(ret==0){
        	List<RelationshipInfo> relationships=type.getRelationshipsOneToMany();
        	for(RelationshipInfo info:relationships){
        		Class classChild=info.getGenericClass();
        		createTable(classChild);
        	}
        }
        this.connection.commit();
        return ret==0;
    }
    
    public <T extends BasicModel> boolean emptyTable(Class<T> clazz) throws SQLException {
    	DatabaseType type = new DatabaseType(clazz);
    	String sql = "delete from `" + type.getTableName() + "`;";
    	Statement stm = this.connection.createStatement();
    	int ret = stm.executeUpdate(sql);
    	this.connection.commit();
		return ret >= 0;
    }

	@SuppressWarnings("rawtypes")
	public abstract String converTypeToString(Class type, double size);

	public abstract String convertOperatorToString(Operator operador);
	@SuppressWarnings("rawtypes")
	public abstract String getCreateTable(Class clazz);
	
	protected abstract Object convertToJavaType(ResultSet data,String fildName,int type);
	
	public ResultSet executeCustomSQL(String sql) throws SQLException{
		Statement stm= this.connection.createStatement();
        return stm.executeQuery(sql);
	}
	public int executeCustomUpdate(String sql) throws SQLException{
		Statement stm= this.connection.createStatement();
        int ret=stm.executeUpdate(sql);
        this.connection.commit();
		return ret;
	}
	public ResultSet executeProcedure(String name,List<Object> params) throws SQLException{
		String call="call "+name+"(";
		for(int i=0;i<params.size();i++){
			if(i>0){
				call+=",";
			}
			call+="?";
		}
		call+=")";
		CallableStatement cs =this.connection.prepareCall(call);
		for(int i=0;i<params.size();i++){
			Object param=params.get(i);
			if(param.getClass().isAssignableFrom(Integer.class)){
				cs.setInt(i+1, (Integer)param);
			}
			if(param.getClass().isAssignableFrom(String.class)){
				cs.setString(i+1, (String)param);
			}
			if(param.getClass().isAssignableFrom(Float.class)){
				cs.setFloat(i+1, (Float)param);
			}
		}
		ResultSet ret=cs.executeQuery();
		this.commit();
		return ret;
	}
	
	protected void addCache(BasicModel model){
		if(!caches.containsKey(model.getClass())){
			caches.put(model.getClass(), new HashMap<Integer,BasicModel>());
		}
		HashMap<Integer,BasicModel> cache=caches.get(model.getClass());
		cache.put(model.getID(), model);
	}
	protected void addCache(Class<?> clazz,int id,StringWriter xml){
		if(!serializedCaches.containsKey(clazz)){
			serializedCaches.put(clazz, new HashMap<Integer,StringWriter>());
		}
		HashMap<Integer,StringWriter> cache=serializedCaches.get(clazz);
		cache.put(id, xml);
	}
	protected BasicModel getCacheModel(Class<?> type, int id){
		if(!caches.containsKey(type)){
			return null;
		}
		HashMap<Integer,BasicModel> cache=caches.get(type);
		if(!cache.containsKey(id)){
			return null;
		}
		return cache.get(id);
	}	
	
	protected StringWriter getSerializedCache(Class<?> type, int id){
		if(!serializedCaches.containsKey(type)){
			return null;
		}
		HashMap<Integer,StringWriter> cache=serializedCaches.get(type);
		if(!cache.containsKey(id)){
			return null;
		}
		return cache.get(id);
	}
}
