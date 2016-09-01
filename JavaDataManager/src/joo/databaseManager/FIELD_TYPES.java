package joo.databaseManager;

import java.math.BigDecimal;
import java.util.Date;

public abstract class FIELD_TYPES {
	public static final int STRING=0;
	public static final int INT=1;
	public static final int FLOAT=2;
	public static final int DATE=3;
	public static final int DOUBLE=4;
	public static final int BIG_DECIMAL=5;
	public static final int BYTE_ARRAY=6;
	public static final int SHORT=7;
	public static final int LONG=8;
	
	public static int getFieldType(Class<?> type){
		if(String.class.isAssignableFrom(type)){
			return STRING;
		}
		if(int.class.isAssignableFrom(type)){
			return INT;
		}
		if(Integer.class.isAssignableFrom(type)){
			return INT;
		}
		if(float.class.isAssignableFrom(type)){
			return FLOAT;
		}
		if(double.class.isAssignableFrom(type)){
			return DOUBLE;
		}
		if(Float.class.isAssignableFrom(type)){
			return FLOAT;
		}
		if(Double.class.isAssignableFrom(type)){
			return DOUBLE;
		}
		if(Date.class.isAssignableFrom(type)){
			return DATE;
		}
		if(BigDecimal.class.isAssignableFrom(type)){
			return BIG_DECIMAL;
		}
		if(byte[].class.isAssignableFrom(type) ||
				Byte[].class.isAssignableFrom(type)){
			return BYTE_ARRAY;
		}
		if(short.class.isAssignableFrom(type) || Short.class.isAssignableFrom(type)){
			return SHORT;
		}
		
		if(long.class.isAssignableFrom(type) || Long.class.isAssignableFrom(type)){
			return LONG;
		}
		
		throw new IllegalArgumentException("Tipo "+type.getName()+" não suportado");
	}
}
