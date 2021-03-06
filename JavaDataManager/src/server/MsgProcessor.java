package server;

import server.helpers.MsgHelper;

public abstract class MsgProcessor {

	public boolean isOwnerMsg(int type){
		int[] msgs=getMsgTypes();
		if(msgs!=null){
			for(int msg : msgs){
				if(msg==type){
					return true;
				}
			}
		}
		return false;
	}
	
	public abstract  MsgHelper processMsg(MsgHelper helper) throws Exception;
	
	public abstract int[] getMsgTypes();
	
	public abstract String getMsgName(int type); 
}
