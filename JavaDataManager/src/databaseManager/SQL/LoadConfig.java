package databaseManager.SQL;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;

import databaseManager.DatabaseManager;

public class LoadConfig {
	public static int TYPE_DONT_RELATIONS=0;
	public static int TYPE_SCRIPT=1;
	private int type;
	private byte[] data;
	
	public LoadConfig(int type){
		this.type=type;
	}
	
	public int getType(){
		return type;
	}
	
	public byte[] getData(){
		return data;
	}
	public void setData(byte[] data){
		this.data=data;
	}
	
	public void setDataByResource(String path){
		ClassLoader loader=DatabaseManager.class.getClassLoader();
		
		final InputStream input = loader.getResourceAsStream(path);
		if(input==null){
			return;
		}
		BufferedReader buffer = new BufferedReader(new InputStreamReader(input));
		try {
			String script="";
			String line = buffer.readLine();
			while(line!=null){
				script+=line;
				line=buffer.readLine();
			}
			data=script.getBytes();
		} catch (IOException e) {
			e.printStackTrace();
		} 
		
	}
	
}
