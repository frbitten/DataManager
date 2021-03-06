package server;

import java.net.Socket;
import java.util.ArrayList;
import java.util.List;

import server.helpers.MsgHelper;
import server.log.Log;

public class ProcessorHandler extends ClientHandler {

	protected List<MsgProcessor> processors;
	
	public ProcessorHandler(Socket socket,long lockedMaxTime) {
		super(socket,lockedMaxTime);
		processors=new ArrayList<MsgProcessor>();
	}
	
	public void addMsgProcessor(MsgProcessor processor){
		processors.add(processor);
	}

	@Override
	protected void onProcess(byte[] buffer) {
		try{
			MsgHelper helper=new MsgHelper(buffer);
			
			for (MsgProcessor processor : processors) {
				if(processor.isOwnerMsg(helper.getType())){
					MsgHelper ret=processor.processMsg(helper);
					if(ret!=null){
						byte[] sendBuffer=ret.toArray();
						send(sendBuffer);
						Log.onProcessMsg(processor.getMsgName(helper.getType()), buffer.length,sendBuffer.length,socket.getInetAddress().getHostAddress());
						return;
					}
				}
			}
			MsgHelper error=new MsgHelper(BasicMsgProcessor.ERROR);
			error.addValue("Mensagem de id "+helper.getType()+" desconhecida.");
			byte[] sendBuffer=error.toArray();
			send(sendBuffer);
			Log.onUnknownMessage(helper.getType(),buffer.length,sendBuffer.length,socket.getInetAddress().getHostAddress());
		}catch( Exception err){
			Log.addExceptionLog(err);
			MsgHelper error=new MsgHelper(BasicMsgProcessor.ERROR);
			error.addValue(err.toString());
			byte[] sendBuffer=error.toArray();
			send(sendBuffer);
		}
	}
	
	@Override
	public void start(){
		List<Integer> msgTypes=new ArrayList<Integer>();
		for (MsgProcessor processor : processors) {
			int[] types=processor.getMsgTypes();
			if(types!=null){
				for(int type:types){
					if(!msgTypes.contains(type)){
						msgTypes.add(type);
					}else{
						throw new IllegalArgumentException("Tipo de mensagem "+type+" duplicado. Servidor n�o pode ser iniciad");
					}
				}
			}
		}
		super.start();
	}

}
