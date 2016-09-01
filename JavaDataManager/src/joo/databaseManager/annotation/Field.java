package joo.databaseManager.annotation;

import java.lang.annotation.Documented;
import java.lang.annotation.Inherited;
import java.lang.annotation.ElementType;
import java.lang.annotation.Retention;
import java.lang.annotation.RetentionPolicy;
import java.lang.annotation.Target;

@Documented
@Target(ElementType.FIELD)
@Inherited
@Retention(RetentionPolicy.RUNTIME)
public @interface Field {
	
	/* binary values
	 * FILE     1000
     * PRIMARY  0101
     * IDENTITY 0011 
     * NOT NULL 0001
     * NULL     0000
     * */

	public static final int PRIMARY_KEY =5;
    public static final int IDENTITY=3;
    public static final int NOT_NULL=1;
    public static final int NULL=0;	
	
	String Name();
	double Size() default -1;
	int Type() default NOT_NULL;
}
