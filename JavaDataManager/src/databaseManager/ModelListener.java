package databaseManager;

import databaseManager.BasicModel.Status;

public interface ModelListener {
	public void statusChange(BasicModel model, Status oldStatus);
	public void propertyChange(BasicModel model, String property);
}
