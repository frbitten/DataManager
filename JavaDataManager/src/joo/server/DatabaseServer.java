package joo.server;

import java.util.ArrayList;
import java.util.List;

import joo.server.log.Log;

@SuppressWarnings("rawtypes")
public class DatabaseServer extends JooServer {

	List<Class<?>> msgProcessors;
	
	@SuppressWarnings("unchecked")
	public DatabaseServer(int port,String logDir,long lockedMaxTime) {
		super(port, ProcessorHandler.class,logDir,lockedMaxTime);
		msgProcessors=new ArrayList<Class<?>>();
	}
	
	public void addProcessor(Class<?> processorType){
		msgProcessors.add(processorType);
	}
	
	@Override
	public void onClientConnected(ClientHandler handler){
		for (int i = 0; i < msgProcessors.size(); i++) {
			Class<?> clazz=msgProcessors.get(i);
			try {
				((ProcessorHandler)handler).addMsgProcessor((MsgProcessor)clazz.getConstructor().newInstance());
			} catch (Exception e) {
				Log.addExceptionLog(e);
			} 
		}
	}
}
