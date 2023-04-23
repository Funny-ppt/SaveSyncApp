using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SaveSyncApp;

internal class UserRequestProvider : IUserRequestProvider
{
    public void ShowRequest(int id, string message, (string content, string action)[] options, Action<IDictionary<string, object>> callback)
    {
        var dispalyMessage = $"{message}\n\n";
        dispalyMessage = options.Length switch
        {
            <= 1 => dispalyMessage,
            2 => dispalyMessage +
                 $"是|Yes={options[0].content}\n" +
                 $"否|No={options[1].content}",
            3 => dispalyMessage +
                 $"是|Yes={options[0].content}\n" +
                 $"否|No={options[1].content}\n" +
                 $"取消|Cancel={options[2].content}",
            _ => throw new ArgumentException(null, nameof(options))
        };
        var buttons = options.Length switch
        {
            <= 1 => MessageBoxButton.OK,
            2 => MessageBoxButton.YesNo,
            3 => MessageBoxButton.YesNoCancel,
            _ => throw new ArgumentException(null, nameof(options)) // never reaches
        };
        var result = MessageBox.Show(message, "SaveSync", buttons);
        var action = result switch
        {
            MessageBoxResult.None => options.Length == 1 ? options[0].action : "",
            MessageBoxResult.OK => options.Length == 1 ? options[0].action : "",
            MessageBoxResult.Yes => options[1].action,
            MessageBoxResult.No => options[2].action,
            MessageBoxResult.Cancel => options[3].action,
        };
        callback(new Dictionary<string, object> { { "id", id }, { "action", action } });
    }
}
