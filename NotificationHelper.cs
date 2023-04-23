using Microsoft.Toolkit.Uwp.Notifications;
using SaveSyncApp.Properties;
using System;

namespace SaveSyncApp;

public static class NotificationHelper
{
    public static void ShowNotification(int id, string message, bool important = false) => ShowNotification(id, message, important, new[] { ("确认", "confirm") }, Array.Empty<(string, string)>());
    public static void ShowNotification(int id, string message, bool important, (string content, string action)[] buttons, (string key, string value)[] args)
    {
        if (id < (int)Settings.Default.NotificationLevel * 1000 && !important)
        {
            return;
        }

        var builder = new ToastContentBuilder()
            .SetToastScenario(important ? ToastScenario.Reminder : ToastScenario.Default)
            .SetToastDuration(ToastDuration.Short)
            .AddArgument("id", id)
            .AddText(message);
        foreach (var (key, value) in args)
        {
            builder.AddArgument(key, value);
        }
        foreach (var (content, action) in buttons)
        {
            builder.AddButton(new ToastButton()
                .SetContent(content)
                .AddArgument("action", action));
        }
        builder.Show();
    }
}
