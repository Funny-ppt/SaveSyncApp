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

    Dictionary<string, Action<IDictionary<string, string>>> _callbacks = new();
    static IDictionary<string, string> ParseArguments(string arguments)
    {
        var result = new Dictionary<string, string>();

        var keyValuePairs = arguments.Split(';');
        foreach (var keyValuePair in keyValuePairs)
        {
            var keyValue = keyValuePair.Split('=');
            if (keyValue.Length == 2)
            {
                result[keyValue[0]] = keyValue[1];
            }
        }

        return result;
    }
    void OnActivated(ToastNotificationActivatedEventArgsCompat e)
    {
        var result = ParseArguments(e.Argument);
        if (result.TryGetValue("guid", out var guid))
        {
            if (_callbacks.Keys.Contains(guid))
            {
                _callbacks[guid](result);
                _callbacks.Remove(guid);
            }
        }
    }
    public void ShowRequest(int id, string message, (string content, string action)[] options, Action<IDictionary<string, string>> callback)
    {
        var guid = Guid.NewGuid();
        var guid_str = guid.ToString();
        _callbacks[guid_str] = callback;
        NotificationHelper.ShowNotification(id, message, true, ToastScenario.Alarm, options, new[] { ("guid", guid_str) });
    }
}
