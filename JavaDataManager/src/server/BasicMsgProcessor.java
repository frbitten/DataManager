package server;

import java.util.ArrayList;
import java.util.List;

import server.helpers.MsgHelper;
import server.log.Log;
import databaseManager.BasicModel;
import databaseManager.DatabaseManager;
import databaseManager.SQL.LoadConfig;
import databaseManager.SQL.OrderBy;
import databaseManager.SQL.Where;
import databaseManager.SQL.Where.Operator;
import databaseManager.connection.DataBaseConnection;

public class BasicMsgProcessor extends MsgProcessor {

	public static final int ERROR=0;
	public static final int SAVE=1;
	public static final int SAVE_ARRAY=2;
	public static final int GET=3;
	public static final int GET_ALL=4;
	public static final int GET_SOME=5;

	protected MsgHelper save(BasicModel model){
		MsgHelper ret=new MsgHelper(SAVE);;
		try{
			DataBaseConnection sql= DatabaseManager.getInstance().openConnection();
			if(sql.saveItem(model)){
				ret.addValue(model);
			}else{
				MsgHelper errorMsg=new MsgHelper(BasicMsgProcessor.ERROR);
				errorMsg.addValue("Erro ao salvar objeto.");
				return errorMsg;
			}
			sql.close();					
		}catch(Exception e){
			Log.addExceptionLog(e);
			e.printStackTrace();
			MsgHelper errorMsg=new MsgHelper(BasicMsgProcessor.ERROR);
			if(e.getMessage()!=null){
				errorMsg.addValue(e.getMessage());
			}else{
				errorMsg.addValue(e.getClass().getName());
			}
			return errorMsg;
		}
		return ret;
	}
	protected MsgHelper save(List<BasicModel> models){
		MsgHelper ret=new MsgHelper(SAVE_ARRAY);
		try{
			DataBaseConnection sql= DatabaseManager.getInstance().openConnection();
			for (BasicModel model:models) {
				if(!sql.saveItem(model,false)){
					ret.addValue(0);
					return ret;
				}
			}
			sql.commit();
			sql.close();
			ret.addValue(1);
		}catch(Exception e){
			Log.addExceptionLog(e);
			e.printStackTrace();
			MsgHelper errorMsg=new MsgHelper(BasicMsgProcessor.ERROR);
			if(e.getMessage()!=null){
				errorMsg.addValue(e.getMessage());
			}else{
				errorMsg.addValue(e.getClass().getName());
			}
			return errorMsg;
		}
		return ret;
	}
	
	@SuppressWarnings({ "rawtypes", "unchecked" })
	protected MsgHelper get(String className,int id,LoadConfig config){
		MsgHelper ret=new MsgHelper(GET);
		try{
			Class clazz=Class.forName(className);
			DataBaseConnection sql= DatabaseManager.getInstance().openConnection();
			BasicModel model= sql.loadItem(clazz, id,config);
			ret.addValue(model);			
			sql.close();
			
		}catch(Exception e){
			Log.addExceptionLog(e);
			e.printStackTrace();
			MsgHelper errorMsg=new MsgHelper(BasicMsgProcessor.ERROR);
			errorMsg.addValue(e.getMessage());
			return errorMsg;
		}
		return ret;
	}
	@SuppressWarnings({ "rawtypes", "unchecked" })
	protected MsgHelper getAll(String className,OrderBy orderBy, LoadConfig config){
		MsgHelper ret=new MsgHelper(GET_ALL);
		try {
			Class clazz=Class.forName(className);
			DataBaseConnection sql= DatabaseManager.getInstance().openConnection();
			List<BasicModel> items=sql.getItems(clazz,null,orderBy,config);
			for (BasicModel item : items) {
				ret.addValue(item);
			}
			sql.close();
			
		} catch (Exception e) {
			Log.addExceptionLog(e);
			e.printStackTrace();
			MsgHelper errorMsg=new MsgHelper(BasicMsgProcessor.ERROR);
			errorMsg.addValue(e.getMessage());
			return errorMsg;
		}
		return ret;
	}
	
	@SuppressWarnings({ "unchecked", "rawtypes" })
	@Override
	public MsgHelper processMsg(MsgHelper helper) {
		MsgHelper ret=new MsgHelper(helper.getType());
		switch(helper.getType()){
			case SAVE:
				return save(helper.getValue(0));
			case SAVE_ARRAY:{
				List<BasicModel> models=new ArrayList<BasicModel>();
				for (int i = 0; i < helper.getCountValues(); i++) {
					models.add(helper.getValue(i));					
				}
				return save(models);
			}
				
			case GET:
			{
				if(helper.getCountValues()>2){
					int type=helper.getValueToInt(2);
					LoadConfig config=new LoadConfig(type);
					if(type==LoadConfig.TYPE_SCRIPT){
						config.setData(helper.getValueToBuffer(3));
					}
					return get(helper.getValueToString(0),helper.getValueToInt(1),config);
				}else{
					return get(helper.getValueToString(0),helper.getValueToInt(1),null);
				}
			}
			case GET_ALL:
			{
				LoadConfig config=null;
				OrderBy orderby=null;
				switch (helper.getCountValues()) {
				case 5:
					orderby=new OrderBy(OrderBy.ORDER.values()[helper.getValueToInt(4)]);
					try {
						Class clazz = Class.forName(helper.getValueToString(0));
						orderby.addItem(clazz,helper.getValueToString(3));
					} catch (ClassNotFoundException e) {
						Log.addExceptionLog(e);
						e.printStackTrace();
						MsgHelper errorMsg=new MsgHelper(BasicMsgProcessor.ERROR);
						errorMsg.addValue(e.getMessage());
						return errorMsg;
					}
				case 3:
					int type=helper.getValueToInt(1);
					if(type>=0){
						config=new LoadConfig(type);
						byte[] buff=helper.getValueToBuffer(2);
						if(buff.length>0){
							config.setData(buff);
						}
					}
				}
				
				return getAll(helper.getValueToString(0),orderby,config);
			}
			case GET_SOME:
			{
				try {
					Class clazz=Class.forName(helper.getValueToString(0));
					Where where=new Where();
					for (int i = 1; i < helper.getCountValues(); i++) {
						if(i>1){
							where.addOperator(Operator.OR);
						}
						where.addField(clazz, "ID");
						where.addOperator(Operator.EQUAL);
						where.addValue(helper.getValueToInt(i));
					}
					DataBaseConnection sql= DatabaseManager.getInstance().openConnection();
					List<BasicModel> items=sql.getItems(clazz,where);
					sql.close();
					for (BasicModel item : items) {
						ret.addValue(item);
					}
				} catch (Exception e) {
					e.printStackTrace();
					MsgHelper errorMsg=new MsgHelper(BasicMsgProcessor.ERROR);
					errorMsg.addValue(e.getMessage());
					return errorMsg;
				}
			}			
			break;
			default:
				MsgHelper errorMsg=new MsgHelper(BasicMsgProcessor.ERROR);
				errorMsg.addValue("Mensagem desconhecida");
				return errorMsg;
		}
		return ret;
	}

	@Override
	public int[] getMsgTypes() {
		return new int[]{SAVE,SAVE_ARRAY,GET,GET_ALL,GET_SOME};
	}

	@Override
	public String getMsgName(int type) {
		switch(type){
			case SAVE:
				return "SAVE";
			case SAVE_ARRAY:
				return "SAVE_ARRAY";	
			case GET:
				return "GET";
			case GET_ALL:
				return "GET_ALL";
			case GET_SOME:
				return "GET_SOME";
			default:
				return "UNKNOWN";
		}
	}

}
