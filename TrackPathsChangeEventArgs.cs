using System;

namespace SaveSyncApp;

public enum ChangeAction
{
    Add = 0,
    Remove = 1,
}

public class TrackPathsChangeEventArgs : EventArgs
{
    public string Path { get; }
    public ChangeAction Action { get; }
    public TrackPathsChangeEventArgs(string path, ChangeAction action)
    {
        Path = path;
        Action = action;
    }
}