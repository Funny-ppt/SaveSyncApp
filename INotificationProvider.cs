namespace SaveSyncApp;

public interface INotificationProvider
{
    void ShowNotification(int id, string message, bool important = false);
}
