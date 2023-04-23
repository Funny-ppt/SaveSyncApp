using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;

namespace SaveSyncApp;

internal class UserRequestProvider : IUserRequestProvider
{
    public UserRequestProvider()
    {
        ToastNotificationManagerCompat.OnActivated += OnActivated;
    }

    Dictionary<string, Action<IDictionary<string, object>>> _callbacks = new();
    void OnActivated(ToastNotificationActivatedEventArgsCompat e)
    {
        if (e.UserInput.TryGetValue("guid", out var guid) && guid is string guid_str)
        {
            if (_callbacks.Keys.Contains(guid_str))
            {
                _callbacks[guid_str](e.UserInput);
                _callbacks.Remove(guid_str);
            }
        }
    }
    public void ShowRequest(int id, string message, (string content, string action)[] options, Action<IDictionary<string, object>> callback)
    {
        var guid = Guid.NewGuid();
        var guid_str = guid.ToString();
        _callbacks[guid_str] = callback;
        NotificationHelper.ShowNotification(id, message, true, options, new[] { ("guid", guid_str) });
    }
}
