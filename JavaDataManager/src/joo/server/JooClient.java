package joo.server;

import java.io.ByteArrayOutputStream;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.InetSocketAddress;
import java.net.Socket;
import java.net.SocketException;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;

public class JooClient implements AutoCloseable{
	
	private static String serverDefault;
	private static int portDefault;
	
	public static void setServer(String server){
		serverDefault=server;
	}
	public static void setPort(int port){
		portDefault=port;
	}
	
	private Socket socket;
	
	public JooClient(int bufferSize){
		this.socket=new Socket();
		try {
			this.socket.setReceiveBufferSize(bufferSize);
		} catch (SocketException e) {
			e.printStackTrace();
		}		
	}
	public JooClient(){
		this.socket=new Socket();
		try {
			this.socket.setReceiveBufferSize(65536);
			this.socket.setSendBufferSize(65536);
		} catch (SocketException e) {
			e.printStackTrace();
		}		
	}
	public boolean connect(){
		if(serverDefault!=null && serverDefault!="" && portDefault>0){		
			return connect(serverDefault, portDefault);
		}
		throw new IllegalArgumentException("Valores default para conexão não consigurados.");
	}
	public boolean connect(String address,int port){
		try{
			this.socket.connect(new InetSocketAddress(address, port),0);
		}catch(Exception err){
			err.printStackTrace();
			return false;
		}
		return true;
	}
	
	
	public void disconnect(){
		try{
			OutputStream out=socket.getOutputStream();
			ByteBuffer msgBuffer = ByteBuffer.allocate(5).order(ByteOrder.LITTLE_ENDIAN);
			msgBuffer.put((byte)'j');
			msgBuffer.put((byte)'o');
			msgBuffer.put((byte)'o');
			msgBuffer.put((byte)1);
			msgBuffer.put((byte)MSG_TYPES.DISCONNECT);
			out.write(msgBuffer.array());
			
			byte[] buff=receive();
			socket.close();
		}catch(Exception err){
			err.printStackTrace();
		}
	}
	public boolean isConnected(){
		return socket.isConnected();
	}
	
	public boolean send(byte[] buffer){
		try{
			OutputStream out=socket.getOutputStream();
			ByteBuffer msgBuffer = ByteBuffer.allocate(buffer.length+9).order(ByteOrder.LITTLE_ENDIAN);
			msgBuffer.put((byte)'j');
			msgBuffer.put((byte)'o');
			msgBuffer.put((byte)'o');
			msgBuffer.put((byte)1);
			msgBuffer.put((byte)MSG_TYPES.USER_MSG);
			msgBuffer.putInt(buffer.length);
			msgBuffer.put(buffer);
			byte[] buff=msgBuffer.array();
//			System.out.println("Enviado "+buff.length+" bytes.");
			out.write(buff);
			return true;
		}catch(Exception err){
			err.printStackTrace();
			return false;
		}
	}
	public byte[] receive(){
		try {
			InputStream input= socket.getInputStream();
			ByteArrayOutputStream msg=new ByteArrayOutputStream();
			boolean isHeader=false;
			int msgSize=0;
			while(true){
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
								case MSG_TYPES.USER_MSG:
									byte[] sizeArray=new byte[4];
									input.read(sizeArray);
									ByteBuffer sizeBuffer=ByteBuffer.wrap(sizeArray).order(ByteOrder.LITTLE_ENDIAN);
									msgSize=sizeBuffer.getInt();
									isHeader=true;
								break;
								default:
									return null;
							}
						}
					}else{
						Thread.sleep(300);
					}
				}else{
					if(qtd>0){
						if(msg.size()+qtd>msgSize){
							byte[] buff=new byte[msgSize-msg.size()];
							input.read(buff);
							msg.write(buff);
						}else{
							byte[] buff=new byte[qtd];
							input.read(buff);
							msg.write(buff);
						}
						if(msg.size()==msgSize){
							byte[] buff=msg.toByteArray();
							return buff;
						}						
					}else{
						Thread.sleep(300);
					}
				}
			}			
		} catch (Exception e) {
			e.printStackTrace();
			return null;
		}
		
	}
	public int dataAvaliable(){
		try {
			return socket.getInputStream().available();
		} catch (Exception e) {
			e.printStackTrace();
			return 0;
		}
		
	}
	public void close() throws Exception {
		disconnect();		
	}
}
