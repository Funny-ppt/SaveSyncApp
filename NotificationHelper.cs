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

    public static void ShowNotification(int id, string message, bool important = false) => ShowNotification(id, message, important, ("确认", "confirm"));
    public static void ShowNotification(int id, string message, bool important, params (string content, string action)[] buttons)
    {
        if (Settings.Default.NotificationLevel == NotificationLevel.Minimum && !important)
        {
            return;
        }

        var builder = new ToastContentBuilder()
            .SetToastScenario(important ? ToastScenario.Reminder : ToastScenario.Default)
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
