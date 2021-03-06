package server.log;

import java.io.BufferedWriter;
import java.io.FileWriter;
import java.io.IOException;
import java.io.PrintWriter;
import java.text.DateFormat;
import java.text.SimpleDateFormat;
import java.util.Date;

public abstract class Log {
	
	private static String logDir;
	
	public static boolean DEBUG=false;
	
	public static void setDir(String dir){
		logDir=dir;
	}

	public static void started() {
		addLog("Started Server");        
	}

	public static void addLog(String msg){
		if(logDir.equals("")){
			return;
		}
		if ( DEBUG ) {
		    System.out.println(msg);
		    return;
		}
		Date date=new Date(); 	
		DateFormat df = new SimpleDateFormat("yyyy-MM-dd");
		String file=logDir+"/"+df.format(date)+".log";

		try {
			BufferedWriter writer = new BufferedWriter(new FileWriter(file,true));
			df = new SimpleDateFormat("dd/mm/yyyy HH:mm:ss");
			writer.write(df.format(date)+" - "+msg+System.getProperty("line.separator"));
			writer.flush();
			writer.close();
		} catch (IOException e) {
			e.printStackTrace();
		}
	}
	public static void addExceptionLog(Throwable err){
		if ( DEBUG ) {
		    err.printStackTrace();
		    return;
		}
		Date date=new Date(); 	
		DateFormat df = new SimpleDateFormat("yyyy-MM-dd");
		String file=logDir+"/"+df.format(date)+"_exception.log";
		try {
			BufferedWriter out = new BufferedWriter(new FileWriter(file, true));
			PrintWriter pWriter = new PrintWriter(out, true);
			err.printStackTrace(pWriter);
			out.flush();
			out.close();
		} catch (IOException e1) {
			e1.printStackTrace();
		}
	}
	
	public static void onConnected(String ip) {
		addLog("Client connect from "+ip);
	}

	public static void onProcessMsg(String msgName, int receiveSize, int sendSize,String ip) {
		addLog("Process message "+msgName+" in:"+receiveSize+" out:"+sendSize+" IP:"+ip);
	}

	public static void onUnknownMessage(int type, int receiveSize,int sendSize,String ip) {
		addLog("Unknown message "+type+" in:"+receiveSize+" out:"+sendSize+" IP:"+ip);		
	}

}
