using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SaveSyncApp;

internal class UserRequestProvider : IUserRequestProvider
{
    public async void ShowRequest(int id, string message, (string content, string action)[] options, Action<IDictionary<string, string>> callback)
    {
        var result = await Task.Run(() => CustomDialog.ShowDialog("SaveSync", message, options, "confirm"));
        callback(result as Dictionary<string, string>); // 这是一个设计失误，不过不影响类型安全所以就这样吧
    }
}
