package databaseManager.helpers;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.StringReader;
import java.io.StringWriter;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.zip.GZIPInputStream;
import java.util.zip.GZIPOutputStream;

import org.kxml2.io.KXmlParser;
import org.kxml2.io.KXmlSerializer;
import org.xmlpull.v1.XmlPullParser;
import org.xmlpull.v1.XmlPullParserException;
import org.xmlpull.v1.XmlSerializer;

import databaseManager.BasicModel;
import databaseManager.BasicModel.Status;
import databaseManager.type.DatabaseType;
import databaseManager.type.FieldInfo;
import databaseManager.type.RelationshipInfo;

public abstract class SerializerHelper {

	public static final String TAG_RELATIONSHIP_ONE_TO_ONE="relationshipsOneToOne"; 
	public static final String TAG_RELATIONSHIP_ONE_TO_MANY="relationshipsOneToMany";
	public static final String TAG_FIELDS="fields";
	public static final String TAG_MODEL="model";
	public static final String TAG_CHILDREN="children";
	public static final String TAG_CLASS="class";
	public static final String TAG_ID="ID";
	public static final String TAG_FIELD_NAME="fieldName";
	public static final String TAG_RELATIONSHIP_ITEM="item";
	public static final String TAG_STATUS="status";
	
	
	public static byte[] serializer(BasicModel model){
		try{
			String xml=serializerToXML(model);
			if(xml!=null){
				ByteArrayOutputStream outputStream = new ByteArrayOutputStream();
				GZIPOutputStream gzipOutputStream = new GZIPOutputStream(outputStream);
	            gzipOutputStream.write(xml.getBytes("UTF-8"));
	            gzipOutputStream.close();
				return outputStream.toByteArray();
			}
		}catch(Exception err){
			err.printStackTrace();
		}
		return null;
	}
	
	
	public static String serializerToXML(BasicModel model){
		try{
			List<BasicModel> list=new ArrayList<BasicModel>();
			List<Integer> isSerialized=new ArrayList<Integer>();
			XmlSerializer xmlSerializer = new KXmlSerializer();
		    StringWriter writer = new StringWriter();
		    xmlSerializer.setOutput(writer);
		    xmlSerializer.startDocument("UTF-8", true);
		    xmlSerializer.startTag("", "serializer");
		    list=serializeModel(model, xmlSerializer);
		    isSerialized.add(model.hashCode());
		    xmlSerializer.startTag("", TAG_CHILDREN);
		    while(list.size()>0){
		    	BasicModel aux=list.remove(0);
	    		boolean find=false;
	    		for(Integer id:isSerialized){
	    			if(id==aux.hashCode()){
	    				find=true;
	    				break;
	    			}
	    		}
	    		if(find){
	    			continue;
	    		}
	    		isSerialized.add(aux.hashCode());		    	
		    	list.addAll(serializeModel(aux, xmlSerializer));
		    }
		    xmlSerializer.endTag("", TAG_CHILDREN);
		    xmlSerializer.endTag("", "serializer");
		    xmlSerializer.endDocument();
		    return writer.toString();
		}catch(Exception err){
			err.printStackTrace();
		}
		return null;
	}
	private static List<BasicModel> serializeModel(BasicModel model,XmlSerializer xmlSerializer) throws IllegalArgumentException, IllegalStateException, IOException, IllegalAccessException{
		List<BasicModel> ret=new ArrayList<BasicModel>();
		xmlSerializer.startTag("", TAG_MODEL);
	    xmlSerializer.startTag("", TAG_CLASS);
	    xmlSerializer.text(model.getClass().getName());
	    xmlSerializer.endTag("", TAG_CLASS);
	    xmlSerializer.startTag("", TAG_ID);
	    xmlSerializer.text(String.valueOf(model.hashCode()));
	    xmlSerializer.endTag("", TAG_ID);
	    xmlSerializer.startTag("", TAG_FIELDS);
	    DatabaseType type=new DatabaseType(model.getClass());
	    for(FieldInfo info:type.getFields()){
	    	String value=model.getValueByString(info.getName());
	    	if(value!=null){
		    	xmlSerializer.startTag("", info.getName());
		    	xmlSerializer.text(value);
		    	xmlSerializer.endTag("", info.getName());
	    	}
	    }
	    xmlSerializer.startTag("", TAG_STATUS);
    	switch (model.getStatus()) {
		case Delete:
			xmlSerializer.text("delete");
			break;
		case New:
			xmlSerializer.text("new");
			break;
		case Normal:
			xmlSerializer.text("normal");
			break;
		case Update:
			xmlSerializer.text("update");
			break;
		case Invalid:
			xmlSerializer.text("invalid");
			break;
		}	    
    	xmlSerializer.endTag("", TAG_STATUS);
    	xmlSerializer.endTag("", TAG_FIELDS);
    	xmlSerializer.startTag("", TAG_RELATIONSHIP_ONE_TO_ONE);
    	for(RelationshipInfo info:type.getRelationshipsOneToOne()){
    		BasicModel rela=(BasicModel)model.getValue(info.getName());
    		if(rela==null){
    			continue;
    		}
    		xmlSerializer.startTag("", info.getName());
	    	xmlSerializer.startTag("", TAG_CLASS);
	    	xmlSerializer.text(info.getType().getName());
	    	xmlSerializer.endTag("", TAG_CLASS);
	    	xmlSerializer.startTag("", TAG_FIELD_NAME);
	    	xmlSerializer.text(info.getFieldName());
	    	xmlSerializer.endTag("", TAG_FIELD_NAME);
	    	xmlSerializer.startTag("", TAG_ID);	    	
	    	xmlSerializer.text(String.valueOf(rela.hashCode()));
	    	ret.add(rela);
	    	xmlSerializer.endTag("", TAG_ID);
	    	xmlSerializer.endTag("", info.getName());
	    }
    	xmlSerializer.endTag("", TAG_RELATIONSHIP_ONE_TO_ONE);
    	xmlSerializer.startTag("", TAG_RELATIONSHIP_ONE_TO_MANY);
    	for(RelationshipInfo info:type.getRelationshipsOneToMany()){
    		List<?> items=(List<?>)model.getValue(info.getName());
    		if(items==null){
    			continue;
    		}
    		xmlSerializer.startTag("", info.getName());
    		for(Object item:items){
    			BasicModel modelChild=(BasicModel)item;
	    		xmlSerializer.startTag("", TAG_RELATIONSHIP_ITEM);
		    	xmlSerializer.startTag("", TAG_CLASS);
		    	xmlSerializer.text(info.getGenericClass().getName());
		    	xmlSerializer.endTag("", TAG_CLASS);
		    	xmlSerializer.startTag("", TAG_FIELD_NAME);
		    	xmlSerializer.text(info.getFieldName());
		    	xmlSerializer.endTag("", TAG_FIELD_NAME);
		    	xmlSerializer.startTag("", TAG_ID);
		    	xmlSerializer.text(String.valueOf(modelChild.hashCode()));
		    	ret.add(modelChild);
		    	xmlSerializer.endTag("", TAG_ID);
		    	xmlSerializer.endTag("", TAG_RELATIONSHIP_ITEM);
    		}
	    	xmlSerializer.endTag("", info.getName());
	    }
    	xmlSerializer.endTag("", TAG_RELATIONSHIP_ONE_TO_MANY);
	    xmlSerializer.endTag("", TAG_MODEL);
	    return ret;
	}
	
	public static <T extends BasicModel> T deserialize(byte[] buffer) {
		try {
			GZIPInputStream input = new GZIPInputStream(
					new ByteArrayInputStream(buffer));
			ByteArrayOutputStream outputStream = new ByteArrayOutputStream();
			byte[] buff = new byte[1048576];
			int count = input.read(buff);
			while (count != -1) {
				outputStream.write(buff, 0, count);
				count = input.read(buff);
			}
			buffer = outputStream.toByteArray();
			String xml = new String(buffer, "UTF-8");
			return deserializeByXML(xml);
		} catch (IOException err) {
			err.printStackTrace();
		}
		return null;
	}

	@SuppressWarnings("unchecked")
	public static <T extends BasicModel> T deserializeByXML(String xml) {
		try {
			Map<String, Map<Integer, BasicModel>> cache = new HashMap<String, Map<Integer, BasicModel>>();
			// load xml
			XmlPullParser parser = new KXmlParser();
			parser.setFeature(XmlPullParser.FEATURE_PROCESS_NAMESPACES, false);
			parser.setInput(new StringReader(xml));
			int type = parser.getEventType();
			if (type == XmlPullParser.START_DOCUMENT) {
				type = parser.next();
			} else {
				return null;
			}

			if (type != XmlPullParser.START_TAG) {
				return null;
			}

			if (!parser.getName().equalsIgnoreCase("serializer")) {
				return null;
			}
			type = parser.next();
			if (type != XmlPullParser.START_TAG) {
				return null;
			}

			T ret = (T) loadModel(parser, cache);
			type = parser.next();
			if (type != XmlPullParser.START_TAG) {
				throw new IllegalArgumentException(
						"xml formato invalido. N�o possui tag <children>.");
			}
			if (!parser.getName().equalsIgnoreCase(TAG_CHILDREN)) {
				throw new IllegalArgumentException(
						"xml formato invalido. N�o possui tag <children>.");
			}
			type = parser.next();
			List<BasicModel> loaded=new ArrayList<BasicModel>();
			while (!parser.getName().equalsIgnoreCase(TAG_CHILDREN)) {
				if (type == XmlPullParser.START_TAG) {
					BasicModel o=loadModel(parser, cache);
					loaded.add(o);
				}
				type = parser.next();
			}
			for(BasicModel model : loaded){
				model.onFinishDeserialize();
			}
			ret.onFinishDeserialize();
			return ret;
		} catch (IOException err) {
			err.printStackTrace();
		} catch (IllegalArgumentException e) {
			e.printStackTrace();
		} catch (XmlPullParserException e) {
			e.printStackTrace();
		}
		return null;
	}
	
	private static BasicModel loadModel(XmlPullParser parser,Map<String,Map<Integer,BasicModel>> cache) {
		try{
		int type=parser.getEventType();
		if(type!=XmlPullParser.START_TAG){
        	throw new IllegalArgumentException("xml formato invalido. N�o come�a com tab <model>.");
        }
		
        if(!parser.getName().equalsIgnoreCase(TAG_MODEL)){
        	throw new IllegalArgumentException("xml formato invalido. N�o come�a com tab <model>.");
        }
        
        type=parser.next();
        if(type!=XmlPullParser.START_TAG){
        	throw new IllegalArgumentException("xml formato invalido. Tag <class> n�o foi encontrada.");
        }
        if(!parser.getName().equalsIgnoreCase(TAG_CLASS)){
        	throw new IllegalArgumentException("xml formato invalido. Tag <class> n�o foi encontrada.");
        }
        type=parser.next();
        if(type!=XmlPullParser.TEXT){
        	throw new IllegalArgumentException("xml formato invalido. Tag <class> sem valor.");
        }
        String className=parser.getText();
        type=parser.next();
        if(type!=XmlPullParser.END_TAG){
        	throw new IllegalArgumentException("xml formato invalido. Tag <class> n�o foi finalizada.");
        }
        type=parser.next();
        if(type!=XmlPullParser.START_TAG){
        	throw new IllegalArgumentException("xml formato invalido. Tag <ID> n�o foi encontrada.");
        }
        if(!parser.getName().equalsIgnoreCase(TAG_ID)){
        	throw new IllegalArgumentException("xml formato invalido. Tag <ID> n�o foi encontrada.");
        }
        type=parser.next();
        if(type!=XmlPullParser.TEXT){
        	throw new IllegalArgumentException("xml formato invalido. Tag <ID> n�o foi encontrada.");
        }
        int id=Integer.parseInt(parser.getText());
        if(id==0){
    		throw new IllegalArgumentException("N�o foi encontrado valor para a tag ID.");
    	}
        type=parser.next();
        
        
        BasicModel o=null;
        //check cache para ver se o objeto ja foi criado
        if(cache.containsKey(className)){
    		Map<Integer,BasicModel> objs=cache.get(className);
    		if(objs.containsKey(id)){
    			o=objs.get(id);
    		}
    	}else{
    		Map<Integer, BasicModel> map=new HashMap<Integer, BasicModel>();
        	cache.put(className, map);
    	}
    	if(o==null){
    		// n�o achou no cache ent�o cria
    		try{
            	Class<?> clazz=Class.forName(className);
    	        o = (BasicModel)clazz.newInstance();
            	Map<Integer,BasicModel> map=cache.get(className);
            	map.put(id, o);
            }catch(Exception err){
            	throw new IllegalArgumentException("Classe "+className+" n�o foi inicializada.");
            }
    	}
        o.onStartDeserialize();
        type=parser.next();
        while(!parser.getName().equalsIgnoreCase(TAG_MODEL)){
        	if(type==XmlPullParser.START_TAG){
	        	String name=parser.getName();
	        	
        		if(name.equalsIgnoreCase(TAG_FIELDS)){
        			loadFields(parser,o);
        		}
        		Status status=o.getStatus();
        		if(name.equalsIgnoreCase(TAG_RELATIONSHIP_ONE_TO_ONE)){
        			loadRelationshipOneToOne(parser,o,cache);
        		}
        		if(name.equalsIgnoreCase(TAG_RELATIONSHIP_ONE_TO_MANY)){
        			loadRelationshipOneToMany(parser,o,cache);
        		}
        		o.setStatus(status);
	        }
        	type=parser.next();
        }
		return o;
		}catch(Exception e){
			e.printStackTrace();
		}
		return null;
	}
	private static void loadRelationshipOneToMany(XmlPullParser parser,BasicModel model,Map<String,Map<Integer,BasicModel>> cache) throws IOException, XmlPullParserException {
		int nodeType=parser.next();
		while(!parser.getName().equalsIgnoreCase(TAG_RELATIONSHIP_ONE_TO_MANY)){
			if(nodeType==XmlPullParser.START_TAG){
				String propertyName=parser.getName();
				nodeType=parser.next();
				List<BasicModel> items=new ArrayList<BasicModel>();
				while(parser.getName()==null || !parser.getName().equalsIgnoreCase(propertyName)){
					if(nodeType==XmlPullParser.START_TAG && parser.getName().equalsIgnoreCase(TAG_RELATIONSHIP_ITEM)){
						nodeType=parser.next();
						String className="";
						String fieldName="";
						int id=0;
						while(!parser.getName().equalsIgnoreCase(TAG_RELATIONSHIP_ITEM)){
							if(nodeType==XmlPullParser.START_TAG && parser.getName().equalsIgnoreCase(TAG_CLASS)){
								nodeType=parser.next();
								if(nodeType==XmlPullParser.TEXT){
									className=parser.getText();
								}else {
									throw new IllegalArgumentException("xml formato invalido. N�o foi encontrado o valor para a tag <"+TAG_CLASS+"> dentro da tag <"+TAG_RELATIONSHIP_ITEM+">.");
								}
							}
							if(nodeType==XmlPullParser.START_TAG && parser.getName().equalsIgnoreCase(TAG_FIELD_NAME)){
								nodeType=parser.next();
								if(nodeType==XmlPullParser.TEXT){
									fieldName=parser.getText();
								}else{
									throw new IllegalArgumentException("xml formato invalido. N�o foi encontrado o valor para a tag <"+TAG_FIELD_NAME+"> dentro da tag <"+TAG_RELATIONSHIP_ITEM+">.");
								}
							}
							if(nodeType==XmlPullParser.START_TAG && parser.getName().equalsIgnoreCase(TAG_ID) ){
								nodeType=parser.next();
								if(nodeType==XmlPullParser.TEXT){
									id=Integer.parseInt(parser.getText());
								}else{
									throw new IllegalArgumentException("xml formato invalido. N�o foi encontrado o valor para a tag <"+TAG_ID+"> dentro da tag <"+TAG_RELATIONSHIP_ITEM+">.");
								}
							}
							nodeType=parser.next();
						}
						if(className==null || className==""){
							throw new IllegalArgumentException("xml formato invalido. N�o foi encontrada a tag <"+TAG_CLASS+"> dentro da tag <"+TAG_RELATIONSHIP_ITEM+">.");
						}
						if(fieldName==null || fieldName==""){
							throw new IllegalArgumentException("xml formato invalido. N�o foi encontrada a tag <"+TAG_FIELD_NAME+"> dentro da tag <"+TAG_RELATIONSHIP_ITEM+">.");
						}
						if(id==0){
							throw new IllegalArgumentException("xml formato invalido. N�o foi encontrada a tag <"+TAG_ID+"> dentro da tag <"+TAG_RELATIONSHIP_ITEM+">.");
						}
						
						BasicModel o=null;
				        //check cache para ver se o objeto ja foi criado
				        if(cache.containsKey(className)){
				    		Map<Integer,BasicModel> objs=cache.get(className);
				    		if(objs.containsKey(id)){
				    			o=objs.get(id);
				    		}
				    	}else{
				    		Map<Integer, BasicModel> map=new HashMap<Integer, BasicModel>();
				        	cache.put(className, map);
				    	}
				    	if(o==null){
				    		// n�o achou no cache ent�o cria
				    		try{
				            	Class<?> clazz=Class.forName(className);
				    	        o = (BasicModel)clazz.newInstance();
				    	        Map<Integer,BasicModel> map=cache.get(className);
				            	map.put(id, o);
				            }catch(Exception err){
				            	throw new IllegalArgumentException("Classe "+className+" n�o foi inicializada.");
				            }
				    	}
				    	items.add(o);
					}
					nodeType=parser.next();
				}
				try{
					model.setValue(propertyName, items);
//					for(BasicModel o:items){
//						Map<Integer,BasicModel> map=cache.get(o.getClass().getName());
//		            	map.put(o.getID(), o);
//					}
				}catch(Exception er){
					//throw new IllegalArgumentException("N�o foi possivel setar o valor na propriedade "+propertyName+".");
					er.printStackTrace();
					//ignora real�es que n�o existem no objeto que esta sendo criado.
				}
			}
			nodeType=parser.next();
		}
		
	}
	
	private static void loadRelationshipOneToOne(XmlPullParser parser,BasicModel model,Map<String,Map<Integer,BasicModel>> cache) throws XmlPullParserException, IOException {
		int nodeType=parser.next();
		while(!parser.getName().equalsIgnoreCase(TAG_RELATIONSHIP_ONE_TO_ONE)){
			if(nodeType==XmlPullParser.START_TAG){
				String propertyName=parser.getName();
				nodeType=parser.next();
				String className="";
				String fieldName="";
				int id=0;
				while(parser.getName()==null || !parser.getName().equalsIgnoreCase(propertyName)){
					if(nodeType==XmlPullParser.START_TAG && parser.getName().equalsIgnoreCase(TAG_CLASS)){
						nodeType=parser.next();
						if(nodeType==XmlPullParser.TEXT){
							className=parser.getText();
						}else {
							throw new IllegalArgumentException("xml formato invalido. N�o foi encontrado o valor para a tag <"+TAG_CLASS+"> dentro da tag <"+TAG_RELATIONSHIP_ITEM+">.");
						}
					}
					if(nodeType==XmlPullParser.START_TAG && parser.getName().equalsIgnoreCase(TAG_FIELD_NAME)){
						nodeType=parser.next();
						if(nodeType==XmlPullParser.TEXT){
							fieldName=parser.getText();
						}else{
							throw new IllegalArgumentException("xml formato invalido. N�o foi encontrado o valor para a tag <"+TAG_FIELD_NAME+"> dentro da tag <"+TAG_RELATIONSHIP_ITEM+">.");
						}
					}
					if(nodeType==XmlPullParser.START_TAG && parser.getName().equalsIgnoreCase(TAG_ID) ){
						nodeType=parser.next();
						if(nodeType==XmlPullParser.TEXT){
							id=Integer.parseInt(parser.getText());
						}else{
							throw new IllegalArgumentException("xml formato invalido. N�o foi encontrado o valor para a tag <"+TAG_ID+"> dentro da tag <"+TAG_RELATIONSHIP_ITEM+">.");
						}
					}
					nodeType=parser.next();
				}
				if(className==null || className==""){
					throw new IllegalArgumentException("xml formato invalido. N�o foi encontrada a tag <"+TAG_CLASS+"> dentro da tag <"+propertyName+">.");
				}
				if(fieldName==null || fieldName==""){
					throw new IllegalArgumentException("xml formato invalido. N�o foi encontrada a tag <"+TAG_FIELD_NAME+"> dentro da tag <"+propertyName+">.");
				}
				if(id==0){
					throw new IllegalArgumentException("xml formato invalido. N�o foi encontrada a tag <"+TAG_ID+"> dentro da tag <"+propertyName+">.");
				}
				
				BasicModel o=null;
		        //check cache para ver se o objeto ja foi criado
		        if(cache.containsKey(className)){
		    		Map<Integer,BasicModel> objs=cache.get(className);
		    		if(objs.containsKey(id)){
		    			o=objs.get(id);
		    		}
		    	}else{
		    		Map<Integer, BasicModel> map=new HashMap<Integer, BasicModel>();
		        	cache.put(className, map);
		    	}
		    	if(o==null){
		    		// n�o achou no cache ent�o cria
		    		try{
		            	Class<?> clazz=Class.forName(className);
		            	o = (BasicModel)clazz.newInstance();
		            }catch(Exception err){
		            	throw new IllegalArgumentException("Classe "+className+" n�o foi inicializada.");
		            }
		    	}
		    	try{
		    		model.setValue(propertyName, o);
		    		Map<Integer,BasicModel> map=cache.get(className);
	            	map.put(id, o);
		    	}catch(Exception err){
		    		//throw new IllegalArgumentException("N�o foi possivel setar o valor na propriedade "+propertyName+".");
		    		err.printStackTrace();
		    		//ignora real�es que n�o existem no objeto que esta sendo criado
		    	}
			}
			nodeType=parser.next();
		}
		
	}
	private static void loadFields(XmlPullParser parser,BasicModel model) throws XmlPullParserException, IOException {
		int nodeType=parser.next();
		while(!parser.getName().equalsIgnoreCase(TAG_FIELDS)){
			if(nodeType==XmlPullParser.START_TAG){
				String tagName=parser.getName();
				nodeType=parser.next();
				if(nodeType==XmlPullParser.TEXT){
					if(tagName.equalsIgnoreCase(TAG_STATUS)){
						switch (parser.getText()) {
						case "update":
							model.setStatus(Status.Update);
							break;
						case "new":
							model.setStatus(Status.New);
							break;
						case "normal":
							model.setStatus(Status.Normal);
							break;
						case "delete":
							model.setStatus(Status.Delete);
							break;
						case "invalid":
							model.setStatus(Status.Invalid);
							break;
						}
					}else{
						try{
							model.setValueByString(tagName, parser.getText());
						}catch(Exception err){
				    		throw new IllegalArgumentException("N�o foi possivel setar o valor na propriedade "+tagName+".");
				    	}
					}
					nodeType=parser.next();
				} else {
					try{
						model.setValueByString(tagName, "");
					}catch(Exception err){
			    		throw new IllegalArgumentException("N�o foi possivel setar o valor na propriedade "+tagName+".");
			    	}
				}
			}
			nodeType=parser.next();
		}
		
	}
	
}
