package joo.server.helpers;

import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.UnsupportedEncodingException;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;

import joo.databaseManager.BasicModel;
import joo.databaseManager.helpers.SerializerHelper;

public class MsgHelper {

	private final static byte TYPE_OBJ =1;
	private final static byte TYPE_INT=2;
	private final static byte TYPE_STRING=3;
	private final static byte TYPE_DATE=4;
	private final static byte TYPE_BUFFER=5;
	private final static byte TYPE_DOUBLE=6;
	private final static byte TYPE_FLOAT=7;
	
	private int type;
	private List<Object> values; 
	
	public MsgHelper(int type){
		values=new ArrayList<Object>();
		this.type=type;
	}
	public MsgHelper(byte[] buffer){
		values=new ArrayList<Object>();
		ByteBuffer byteBuffer=ByteBuffer.wrap(buffer).order(ByteOrder.LITTLE_ENDIAN);
		type=byteBuffer.getInt();
		int count=byteBuffer.getInt();
		for (int i = 0; i < count; i++) {
			byte typeValue=byteBuffer.get();
			int sizeValue=byteBuffer.getInt();
			switch (typeValue) {
				case TYPE_OBJ:
				{
					byte[] valueBuffer=new byte[sizeValue];
					byteBuffer.get(valueBuffer);
					
					BasicModel value = SerializerHelper.deserialize(valueBuffer);
					values.add(value);
				}
				break;
				case TYPE_INT:
				values.add(byteBuffer.getInt());
				break;
				case TYPE_DATE:
				{
					byte[] valueBuffer=new byte[sizeValue];
					byteBuffer.get(valueBuffer);
					String strDate=new String(valueBuffer);
					SimpleDateFormat formater = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss");
					try {
						Date date = formater.parse(strDate);
						values.add(date);
					} catch (ParseException e) {
						e.printStackTrace();
					}
				}
				break;
				case TYPE_STRING:
				{
					byte[] valueBuffer=new byte[sizeValue];
					byteBuffer.get(valueBuffer);
					String value;
					try {
						value = new String(valueBuffer,"UTF-8");
						values.add(value);
					} catch (UnsupportedEncodingException e) {
						e.printStackTrace();
					}
				}
				break;
				case TYPE_BUFFER:
				{
					byte[] valueBuffer=new byte[sizeValue];
					byteBuffer.get(valueBuffer);
					values.add(valueBuffer);
				}
				break;
				case TYPE_DOUBLE:
					values.add(byteBuffer.getDouble());
					break;
				case TYPE_FLOAT:
					values.add(byteBuffer.getFloat());
					break;
				default:
					throw new IllegalArgumentException("Buffer com dados invalidos. Tido de valor não reconhecido");
			}
		}
		System.out.println();
	}
	
	public void addValue(BasicModel obj){
		values.add(obj);
	}
	public void addValue(int value){
		values.add(value);
	}
	public void addValue(String value){
		values.add(value);
	}
	public void addValue(Date value){
		values.add(value);
	}
	public void addValue(byte[] buffer){
		values.add(buffer);
	}
	public void addValue(Double value) {
		values.add(value);
	}
	public void addValue(Float value){
		values.add(value);
	}
	
	public void addValue(Object value){
		values.add(value);
	}
	
	public BasicModel getValue(int index){
		return (BasicModel)values.get(index);
	}
	public int getValueToInt(int index){
		return (int)values.get(index);
	}
	public String getValueToString(int index){
		return (String)values.get(index);
	}
	public Date getValueToDate(int index){
		return (Date)values.get(index);
	}
	public byte[] getValueToBuffer(int index){
		return (byte[])values.get(index);
	}
	public Double getValueToDouble(int index){
		return (Double)values.get(index);
	}
	public Float getValueToFloat(int index){
		return (Float)values.get(index);
	}
	
	public int getType(){
		return type;
	}
	public int getCountValues(){
		return values.size();
	}
	
	public byte[] toArray(){
		try{
			ByteArrayOutputStream out=new ByteArrayOutputStream();
			ByteBuffer typeBuffer=ByteBuffer.allocate(4).order(ByteOrder.LITTLE_ENDIAN);
			typeBuffer.putInt(type);
			out.write(typeBuffer.array());
			ByteBuffer sizeBuffer=ByteBuffer.allocate(4).order(ByteOrder.LITTLE_ENDIAN);
			sizeBuffer.putInt(values.size());
			out.write(sizeBuffer.array());
			for (int i = 0; i < values.size(); i++) {
				Object value=values.get(i);
				
				if(BasicModel.class.isInstance(value)){
					out.write(TYPE_OBJ);
					byte[] buffer=SerializerHelper.serializer((BasicModel)value);
					if(buffer==null){
						continue;
					}
					ByteBuffer size=ByteBuffer.allocate(4).order(ByteOrder.LITTLE_ENDIAN);
					size.putInt(buffer.length);
					out.write(size.array());
					out.write(buffer);
				}else{
					if(String.class.isInstance(value)){
						out.write(TYPE_STRING);
						byte[] buffer=((String)value).getBytes("UTF-8");
						ByteBuffer size=ByteBuffer.allocate(4).order(ByteOrder.LITTLE_ENDIAN);
						size.putInt(buffer.length);
						out.write(size.array());
						out.write(buffer);
					}else{
						if(Date.class.isInstance(value)){
							out.write(TYPE_DATE);
							SimpleDateFormat formater = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss");
							byte[] buffer=formater.format((Date)value).getBytes();
							ByteBuffer size=ByteBuffer.allocate(4).order(ByteOrder.LITTLE_ENDIAN);
							size.putInt(buffer.length);
							out.write(size.array());
							out.write(buffer);
						}else{
							if(int.class.isInstance(value) || Integer.class.isInstance(value)){
								out.write(TYPE_INT);
								 ByteBuffer size=ByteBuffer.allocate(4).order(ByteOrder.LITTLE_ENDIAN);
								size.putInt(4);
								out.write(size.array());
								ByteBuffer b = ByteBuffer.allocate(4).order(ByteOrder.LITTLE_ENDIAN);
								b.putInt((int)value);
								out.write(b.array());
							}else{
								if(byte[].class.isInstance(value)){
									byte[] buffer=(byte[])value;
									out.write(TYPE_BUFFER);
									ByteBuffer size=ByteBuffer.allocate(4).order(ByteOrder.LITTLE_ENDIAN);
									size.putInt(buffer.length);
									out.write(size.array());
									out.write(buffer);
								}else{
									if(Double.class.isInstance(value)) {
										out.write(TYPE_DOUBLE);
										ByteBuffer size=ByteBuffer.allocate(4).order(ByteOrder.LITTLE_ENDIAN);
										size.putInt(8);
										out.write(size.array());
										ByteBuffer b = ByteBuffer.allocate(8).order(ByteOrder.LITTLE_ENDIAN);
										b.putDouble((Double)value);
										out.write(b.array());
									} else {									
										if(Float.class.isInstance(value)) {
											out.write(TYPE_FLOAT);
											ByteBuffer size=ByteBuffer.allocate(4).order(ByteOrder.LITTLE_ENDIAN);
											size.putInt(4);
											out.write(size.array());
											ByteBuffer b = ByteBuffer.allocate(4).order(ByteOrder.LITTLE_ENDIAN);
											b.putFloat((Float)value);
											out.write(b.array());
										} else {
											throw new IllegalArgumentException("Valor do tipo "+value.getClass().getName()+" não é permitido.");
										}
									}
								}
							}
						}
					}
				}
			}
			return out.toByteArray();
		}catch(IOException err){
			err.printStackTrace();
			return null;
		}
	}

}
