package databaseManager.SQL;

public class Parameter {
	
	  public String Name;    
      public Object Value;
	public Parameter()
	{
		  this.Name = "";
          this.Value = null;
	}
	
	public Parameter(String name,Object value)
	{
		  this.Name = name;
          this.Value = value;
	}
	


	  public String getName() {
		return Name;
	}

	public void setName(String name) {
		Name = name;
	}

	public Object getValue() {
		return Value;
	}

	public void setValue(Object value) {
		Value = value;
	}

 	
    	
}
