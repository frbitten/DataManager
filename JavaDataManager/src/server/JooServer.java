package server;

import java.io.IOException;
import java.net.ServerSocket;
import java.net.Socket;
import java.net.SocketTimeoutException;
import java.util.ArrayList;
import java.util.List;

import server.log.Log;


public class JooServer<T extends ClientHandler> extends Thread{
	private ServerSocket socket;
	private boolean isRun;
	private List<T> clients;
	private Class<T> typeHandler;
	private int port;
	private long lockedMaxTime;
		
	public JooServer(int port,Class<T> typeHandler,String logDir,long lockedMaxTime){
		clients=new ArrayList<T>();
		this.lockedMaxTime=lockedMaxTime;
		this.typeHandler =typeHandler;
		this.port=port;
		isRun=false;
		if(!logDir.equals("")){
			Log.setDir(logDir);
		}
	}
	
	public void run()
	{
		while(isRun){
			try{
				Thread.sleep(300);
				updateClients();
				Socket client = socket.accept();
				client.setReceiveBufferSize(65536);
				T handler=(T)typeHandler.getConstructor(Socket.class,long.class).newInstance(client,lockedMaxTime);
				clients.add(handler);
				Log.onConnected(client.getInetAddress().getHostAddress());
				onClientConnected(handler);
				handler.start();
				onHandlerStarted(handler);
			}catch(SocketTimeoutException s)
			{
			}catch(Exception e)
			{
				Log.addExceptionLog(e);
				isRun=false;
			}
		}
	}

	private void updateClients(){
		int i=0;
		while (i < clients.size()) {
			if(!clients.get(i).isActive()){
				clients.remove(i);
				continue;
			}
			
			if(clients.get(i).isLocked()){
				ClientHandler lockedClient=clients.remove(i);
				lockedClient.stopProcess();
				continue;
			}
			i++;
		}
		Log.addLog("Clients amount:"+clients.size());
	}
	
	@Override
	public void start(){
		try{
			if(socket==null){
				socket=new ServerSocket(port);
				socket.setSoTimeout(30000);
			}
		}catch(Exception err){
			err.printStackTrace();
			return;
		}
		isRun=true;
		super.start();
		Log.started();
	}	
	
	public boolean isRun(){
		return this.isRun;
	}
	
	public void killProccess() {
		this.isRun = false;
		try {
			if(socket!=null){
				socket.close();
			}
		} catch (IOException e) {
			e.printStackTrace();
		}
		for (T t : clients) {
			t.stopProcess();
		}
		clients.clear();
	}
	
	public int getCountClients(){
		return clients.size();
	}
	
	public void onClientConnected(ClientHandler handler){
		
	}
	public void onHandlerStarted(ClientHandler handler){
		
	}

}
