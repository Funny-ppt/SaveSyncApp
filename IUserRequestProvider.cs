using System;
using System.Collections.Generic;

namespace SaveSyncApp;

internal interface IUserRequestProvider
{
    void ShowRequest(int id, string message, (string content, string action)[] options, Action<IDictionary<string, string>> callback);
}
