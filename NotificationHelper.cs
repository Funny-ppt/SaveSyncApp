using Microsoft.Toolkit.Uwp.Notifications;
using SaveSyncApp.Properties;

namespace SaveSyncApp;

public static class NotificationHelper
{
    public static event OnActivated OnActivated
    {
        add => ToastNotificationManagerCompat.OnActivated += value;
        remove => ToastNotificationManagerCompat.OnActivated -= value;
    }

    public static void ShowNotification(int id, string message) => ShowNotification(id, message, ("确认", "confirm"));
    public static void ShowNotification(int id, string message, params (string content, string action)[] buttons)
    {
        if (Settings.Default.NotificationLevel == NotificationLevel.Minimum && id < 100)
        {
            return;
        }

        var builder = new ToastContentBuilder()
            .SetToastScenario(ToastScenario.Reminder)
            .SetToastDuration(ToastDuration.Short)
            .AddArgument("id", id)
            .AddText(message);
        foreach (var (content, action) in buttons)
        {
            builder.AddButton(new ToastButton()
                .SetContent(content)
                .AddArgument("action", action));
        }
        builder.Show();
    }
}
