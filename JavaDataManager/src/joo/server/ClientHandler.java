package joo.server;

import java.io.ByteArrayOutputStream;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.Socket;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.util.Date;

import joo.server.log.Log;


public abstract class ClientHandler extends Thread  {
	
	protected Socket socket;
	protected boolean isRun;
	protected InputStream input;
	protected OutputStream output;
	protected Date startDate;
	protected long lockedMaxTime;
	public ClientHandler(Socket socket,long lockedMaxTime){
		this.socket=socket;
		this.lockedMaxTime=lockedMaxTime;
		try {
			input=socket.getInputStream();
			output=socket.getOutputStream();
		} catch (Exception e) {
			Log.addExceptionLog(e);
			isRun=false;
			return;
		}
		isRun=true;
	}
	public boolean isActive(){
		return isRun;
	}
	
	public boolean isLocked(){
		long start=startDate.getTime();
		long actual=new Date().getTime();
		if(actual-start>lockedMaxTime){
			return true;
		}else{
			return false;
		}
	}
	
	public boolean send(byte[] buffer){
		try{
			ByteBuffer msgBuffer = ByteBuffer.allocate(buffer.length+9).order(ByteOrder.LITTLE_ENDIAN);
			msgBuffer.put((byte)'j');
			msgBuffer.put((byte)'o');
			msgBuffer.put((byte)'o');
			msgBuffer.put((byte)1);
			msgBuffer.put((byte)MSG_TYPES.USER_MSG);
			msgBuffer.putInt(buffer.length);
			msgBuffer.put(buffer);
			byte[] buff=msgBuffer.array();
			output.write(buff);
//			.println("Enviado "+buff.length+" bytes.");
			return true;
		}catch(Exception err){
			err.printStackTrace();
			return false;
		}
	}
	
	public void run()
	{
		ByteArrayOutputStream msg=new ByteArrayOutputStream();
		boolean isHeader=false;
		int msgSize=0;
		this.startDate=new Date();
		while(isRun){
			try {
				int qtd=input.available();
				if(!isHeader){
					if(qtd>=5){
						boolean isTag=false;
						for (int i = 0; i < qtd-2; i++) {
							if(input.read()==106){
								if(input.read()==111){
									if(input.read()==111){
										isTag=true;
										break;
									}
								}							
							}
						}
						if(isTag){
							int version=input.read();
							int type=input.read();
							
							switch (type) {
							case MSG_TYPES.DISCONNECT:
								isRun=false;
								ByteBuffer msgBuffer = ByteBuffer.allocate(5).order(ByteOrder.LITTLE_ENDIAN);
								msgBuffer.put((byte)'j');
								msgBuffer.put((byte)'o');
								msgBuffer.put((byte)'o');
								msgBuffer.put((byte)1);
								msgBuffer.put((byte)MSG_TYPES.DISCONNECT);
								byte[] buff=msgBuffer.array();
								output.write(buff);
								break;
							case MSG_TYPES.USER_MSG:
								byte[] sizeArray=new byte[4];
								input.read(sizeArray);
								ByteBuffer sizeBuffer=ByteBuffer.wrap(sizeArray).order(ByteOrder.LITTLE_ENDIAN);
								msgSize=sizeBuffer.getInt();
								isHeader=true;
								break;
							case MSG_TYPES.PING:
								break;

							default:
								break;
							}
							
						}
					}else{
						Thread.sleep(300);
					}
				}else{
					if(qtd>0){
						for (int i = 0; i < qtd; i++) {
							byte value=(byte)input.read();
							msg.write(value);
							if(msg.size()==msgSize){
								byte[] buff=msg.toByteArray();
//								System.out.println("Recebido "+buff.length+" bytes.");
								onProcess(buff);								
								msg=new ByteArrayOutputStream();
								isHeader=false;
								break;
							}
						}
					}else{
						Thread.sleep(300);
					}
				}
			} catch (Exception e) {
				Log.addExceptionLog(e);
				isRun=false;
			}
		}
		
		
		try {
			socket.close();
		} catch (Exception e) {
			Log.addExceptionLog(e);
		}
	}
	
	protected abstract void onProcess(byte[] buffer);
	
	public void stopProcess(){
		this.isRun=false;
	}
}
