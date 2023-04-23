using System;
using System.Diagnostics;

namespace SaveSyncApp;

internal class FileChangeMatchEventArgs
{
    public string ChangedFile;
    public string ForegroundProcessFile;
    public string ForegroundWindowTitle;
    public string? MatchFriendlyName = null;
    public MatchType MatchType = MatchType.NotMatch;

    public FileChangeMatchEventArgs(string changedFileName, string forgroundProcessName, string forgoundWindowTitle)
    {
        ChangedFile = changedFileName ?? throw new ArgumentNullException(nameof(changedFileName));
        ForegroundProcessFile = forgroundProcessName ?? throw new ArgumentNullException(nameof(forgroundProcessName));
        ForegroundWindowTitle = forgoundWindowTitle ?? throw new ArgumentNullException(nameof(forgoundWindowTitle));
    }
}
