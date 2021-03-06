package databaseManager.helpers;

import java.math.BigDecimal;
import java.text.SimpleDateFormat;
import java.util.Date;

import javax.xml.bind.DatatypeConverter;

import databaseManager.FIELD_TYPES;

public abstract class ConvertHelper {

	public static String toString(Object value){
		if(value==null){
			return null;
		}
		switch (FIELD_TYPES.getFieldType(value.getClass())) {
		case FIELD_TYPES.STRING:
			return (String)value;
		case FIELD_TYPES.INT:
			return value.toString();
		case FIELD_TYPES.DOUBLE:
			return Double.toString((Double)value);
		case FIELD_TYPES.FLOAT:
			return Float.toString((Float)value);
		case FIELD_TYPES.DATE:
			SimpleDateFormat dt = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss");
	        return dt.format((Date)value);
		case FIELD_TYPES.BIG_DECIMAL:
			return ((BigDecimal)value).toString();
		case FIELD_TYPES.BYTE_ARRAY:
			return "<![CDATA[" + DatatypeConverter.printBase64Binary((byte[]) value) + "]]>";
		case FIELD_TYPES.SHORT:
			return value.toString();
		case FIELD_TYPES.LONG:
			return value.toString();
		}
		return null;
	}
}
