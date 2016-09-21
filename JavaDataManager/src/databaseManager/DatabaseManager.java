package databaseManager;

import java.sql.SQLException;

import databaseManager.connection.DataBaseConnection;
import databaseManager.connection.MySqlServerConnection;
import databaseManager.connection.SqliteConnection;
import databaseManager.connection.DataBaseConnection.DATABASE_TYPE;

public class DatabaseManager{
	public static DatabaseManager instance;
	private static DATABASE_TYPE typeDefault;
	private static String serverDefault;
	private static String databaseDefaut;
	private static String userDefault;
	private static String passwordDefault;
	private static String filesDir;
	
	public static void setFilesDirectory(String directory){
		filesDir=directory;
	}
	public static void setDatabaseType(DATABASE_TYPE type){
		typeDefault=type;
	}
	public static void setServer(String server){
		serverDefault=server;
	}
	public static void setDatabase(String database){
		databaseDefaut=database;
	}
	public static void setUser(String user){
		userDefault=user;
	}
	public static void setPassword(String password){
		passwordDefault=password;
	}
	private DatabaseManager(){
		
	}
	public static  DatabaseManager getInstance(){
	
		if(instance == null){
			instance = new DatabaseManager();
		}
		return instance;
	}
	public DataBaseConnection openConnection() throws IllegalArgumentException, SQLException{
		return openConnection(typeDefault, serverDefault, databaseDefaut, userDefault, passwordDefault,filesDir);
	}
	
	public DataBaseConnection openConnection(DATABASE_TYPE type, String server,String database,String user,String password,String filesDirectory) throws SQLException,IllegalArgumentException{
		DataBaseConnection connection=null;
		switch (type) {
		case MYSQL:
			connection=new MySqlServerConnection(type,server,database,user,password,filesDirectory);
			break;
		case POSTGREE:
			break;
			
		case SQLITE:
			connection = new SqliteConnection(type,server,database,user,password,filesDirectory);
			break;
			
		default:
			throw new IllegalArgumentException("Tipo de banco de dados não suportado.");
		}
		if(connection!=null){
			connection.open();
		}
		return connection;
	}
}
