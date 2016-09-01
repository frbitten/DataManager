package joo.databaseManager.annotation;

public @interface RelationshipManyToMany {
	String Name();
	String IntermediateTable();
	Class<Filter> Filter();
}
