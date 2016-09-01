package joo.databaseManager.SQL;

import java.io.StringReader;
import java.util.ArrayList;
import java.util.HashMap;

import org.kxml2.io.KXmlParser;
import org.xmlpull.v1.XmlPullParser;

public class LoadScript extends BasicScript{

	private HashMap<String,String[]> models;
	public LoadScript(String script){
		script=script.replaceAll("\\s+","");
		models=new HashMap<String,String[]>();		
		try {
			XmlPullParser parser = new KXmlParser();
			parser.setFeature(XmlPullParser.FEATURE_PROCESS_NAMESPACES, false);
			parser.setInput(new StringReader(script));
			int type = parser.getEventType();
			if (type != XmlPullParser.START_DOCUMENT) {
				return;
			}
			type = parser.next();
			if (type != XmlPullParser.START_TAG) {
				return;
			}

			if (!parser.getName().equalsIgnoreCase("classes")) {
				return;
			}
			type = parser.next();
			if (type != XmlPullParser.START_TAG) {
				return;
			}
			if (!parser.getName().equalsIgnoreCase("class")) {
				return;
			}
			while(parser.getName().equalsIgnoreCase("class")){
				if (type != XmlPullParser.START_TAG) {
					return;
				}
				type=parser.next();
				if (type != XmlPullParser.START_TAG) {
					return;
				}
				if (!parser.getName().equalsIgnoreCase("name")) {
					return;
				}
				type=parser.next();
				String className=parser.getText();
				type=parser.next();
				type=parser.next();
				if (type != XmlPullParser.START_TAG) {
					return;
				}
				if (!parser.getName().equalsIgnoreCase("fields")) {
					return;
				}
				type=parser.next();
				if (type != XmlPullParser.START_TAG) {
					return;
				}
				if (!parser.getName().equalsIgnoreCase("field")) {
					return;
				}
				ArrayList<String> list=new ArrayList<String>();									
				while(parser.getName().equalsIgnoreCase("field")){
					type=parser.next();
					list.add(parser.getText());
					type=parser.next();
					if (type != XmlPullParser.END_TAG) {
						return;
					}
					type=parser.next();
				}
				models.put(className, list.toArray(new String[list.size()]));
				type=parser.next();
				if (type != XmlPullParser.END_TAG) {
					return;
				}
				type=parser.next();
			}
			
		} catch (Exception e) {
			e.printStackTrace();
		} 
		
	}
	public boolean isLoadField(Class<?> clazz,String fieldName){
		if(models.containsKey(clazz.getName())){
			String[] fields=models.get(clazz.getName());
			for(String field:fields){
				if(fieldName.equalsIgnoreCase(field)){
					return true;
				}
			}
		}
		return false;
	}
}
