namespace SaveSyncApp;

internal class NotificationProvider : INotificationProvider
{
    public void ShowNotification(int id, string message, bool important = false) => NotificationHelper.ShowNotification(id, message, important);
}
