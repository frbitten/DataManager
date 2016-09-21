using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Database;

namespace Server
{
    public class MsgHelper
    {
        public enum TYPES : short
        {
            OBJ =1,
	        INT=2,
	        STRING=3,
	        DATE=4,
	        BUFFER=5,
	        DOUBLE=6,
	        FLOAT=7
        }
	
	    private int type;
	    private List<Object> values; 
	
	    public MsgHelper(int type){
		    values=new List<Object>();
		    this.type=type;
	    }
	    public MsgHelper(byte[] buffer,String assemblyFullName){
		    values=new List<Object>();
            int index=0;
		    type=BitConverter.ToInt32(buffer,index);
            index+=sizeof(int);
		    int count=BitConverter.ToInt32(buffer,index);
            index+=sizeof(int);
		    for (int i = 0; i < count; i++) {
			    byte typeValue=buffer[index];
                index+=1;
			    int sizeValue=BitConverter.ToInt32(buffer,index);
                index+=sizeof(int);
			    switch (typeValue) {
				    case (int)TYPES.OBJ:
				    {
					    byte[] valueBuffer=new byte[sizeValue];
					    Buffer.BlockCopy(buffer,index,valueBuffer,0,sizeValue);
                        index+=sizeValue;
					   
					    BasicModel value = JavaSerializer.Deserialize<BasicModel>(valueBuffer,assemblyFullName);
					    values.Add(value);
				    }
				    break;
                    case (int)TYPES.INT:
				    values.Add(BitConverter.ToInt32(buffer,index));
                    index+=sizeof(int);
				    break;
                    case (int)TYPES.DATE:
				    {
					    byte[] valueBuffer=new byte[sizeValue];
					    Buffer.BlockCopy(buffer,index,valueBuffer,0,sizeValue);
                        index+=sizeValue;
					    String strDate=Encoding.UTF8.GetString(valueBuffer);
					   
						DateTime date = DateTime.Parse(strDate);
						values.Add(date);					   
				    }
				    break;
                    case (int)TYPES.STRING:
				    {
					    byte[] valueBuffer=new byte[sizeValue];
					    Buffer.BlockCopy(buffer,index,valueBuffer,0,sizeValue);
                        index+=sizeValue;
					    String value=Encoding.UTF8.GetString(valueBuffer);
					    values.Add(value);
				    }
				    break;
                    case (int)TYPES.BUFFER:
				    {
					    byte[] valueBuffer=new byte[sizeValue];
					    Buffer.BlockCopy(buffer,index,valueBuffer,0,sizeValue);
                        index+=sizeValue;
					    values.Add(valueBuffer);
				    }
				    break;
                    case (int)TYPES.DOUBLE:
					    values.Add(BitConverter.ToDouble(buffer,index));
					    break;
                    case (int)TYPES.FLOAT:
					    values.Add(BitConverter.ToSingle(buffer,index));
					    break;
				    default:
					    throw new ArgumentException("Buffer com dados invalidos. Tido de valor não reconhecido");
			    }
		    }
	    }
	
	    public void AddValue(BasicModel obj){
		    values.Add(obj);
	    }
	    public void AddValue(int value){
		    values.Add(value);
	    }
	    public void AddValue(String value){
		    values.Add(value);
	    }
	    public void AddValue(DateTime value){
		    values.Add(value);
	    }
	    public void AddValue(byte[] buffer){
		    values.Add(buffer);
	    }
	    public void AddValue(Double value) {
		    values.Add(value);
	    }
	    public void AddValue(float value){
		    values.Add(value);
	    }
        public void AddValue(Decimal value)
        {
            values.Add(value);
        }
	
	    public BasicModel GetValue(int index){
		    return (BasicModel)values[index];
	    }
	    public int GetValueToInt(int index){
		    return (int)values[index];
	    }
	    public String GetValueToString(int index){
		    return (String)values[index];
	    }
	    public DateTime GetValueToDate(int index){
		    return (DateTime)values[index];
	    }
	    public byte[] GetValueToBuffer(int index){
		    return (byte[])values[index];
	    }
	    public Double GetValueToDouble(int index){
		    return (Double)values[index];
	    }
	    public float GetValueToFloat(int index){
		    return (float)values[index];
	    }
	
	    public int GetMsgType(){
		    return type;
	    }
	    public int GetCountValues(){
		    return values.Count;
	    }
	
	    public byte[] ToArray(){
			MemoryStream outStream=new MemoryStream();
			outStream.Write(BitConverter.GetBytes(type),0,sizeof(int));
			outStream.Write(BitConverter.GetBytes(values.Count),0,sizeof(int));
			for (int i = 0; i < values.Count; i++) {
				Object value=values[i];
				
				if(typeof(BasicModel).IsInstanceOfType(value)){
                    outStream.WriteByte((int)TYPES.OBJ);
					byte[] buffer=JavaSerializer.Serializer((BasicModel)value);
					if(buffer==null){
						continue;
					}
					outStream.Write(BitConverter.GetBytes(buffer.Length),0,sizeof(int));
					outStream.Write(buffer,0,buffer.Length);
				}
				if(typeof(String).IsInstanceOfType(value)){
                    outStream.WriteByte((int)TYPES.STRING);
					byte[] buffer=Encoding.UTF8.GetBytes((String)value);
					outStream.Write(BitConverter.GetBytes(buffer.Length),0,sizeof(int));
					outStream.Write(buffer,0,buffer.Length);
				}
				if(typeof(DateTime).IsInstanceOfType(value)){
                    outStream.WriteByte((int)TYPES.DATE);
                    byte[] buffer = Encoding.UTF8.GetBytes(((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss"));
					outStream.Write(BitConverter.GetBytes(buffer.Length),0,sizeof(int));
					outStream.Write(buffer,0,buffer.Length);
				}
				if(typeof(int).IsInstanceOfType(value)){
                    outStream.WriteByte((int)TYPES.INT);
					outStream.Write(BitConverter.GetBytes(sizeof(int)),0,sizeof(int));
					outStream.Write(BitConverter.GetBytes((int)value),0,sizeof(int));
				}
				if(typeof(byte[]).IsInstanceOfType(value)){
					byte[] buffer=(byte[])value;
                    outStream.WriteByte((int)TYPES.BUFFER);
					outStream.Write(BitConverter.GetBytes(buffer.Length),0,sizeof(int));
					outStream.Write(buffer,0,buffer.Length);
				}
				if(typeof(Double).IsInstanceOfType(value)) {
                    outStream.WriteByte((int)TYPES.DOUBLE);
					outStream.Write(BitConverter.GetBytes(sizeof(Double)),0,sizeof(int));
					outStream.Write(BitConverter.GetBytes((Double)value),0,sizeof(Double));
				} 								
				if(typeof(float).IsInstanceOfType(value)) {
                    outStream.WriteByte((int)TYPES.FLOAT);
					outStream.Write(BitConverter.GetBytes(sizeof(float)),0,sizeof(int));
					outStream.Write(BitConverter.GetBytes((float)value),0,sizeof(float));
				}
                if (typeof(Decimal).IsInstanceOfType(value))
                {
                    outStream.WriteByte((int)TYPES.FLOAT);
                    outStream.Write(BitConverter.GetBytes(sizeof(float)), 0, sizeof(int));
                    outStream.Write(BitConverter.GetBytes((float)value), 0, sizeof(float));
                }
			}
			return outStream.ToArray();
	    }
    }
}
