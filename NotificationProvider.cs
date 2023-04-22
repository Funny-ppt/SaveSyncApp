namespace SaveSyncApp;

internal class NotificationProvider : INotificationProvider
{
    public void ShowNotification(int id, string message) => NotificationHelper.ShowNotification(id, message);
}
