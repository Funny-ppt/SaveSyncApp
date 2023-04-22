using System.Collections.Generic;

namespace SaveSyncApp;

public delegate void OnTrackPathsChanged(object sender, TrackPathsChangeEventArgs e);
public interface ITrackPathProvider
{
    IEnumerable<string> GetPaths();
    IEnumerable<string> GetIgnorePaths();
    public void AddPath(string path);
    public void AddPaths(params string[] paths);
    public void AddPaths(IEnumerable<string> paths);
    public void RemovePath(string path);
    public void RemovePaths(params string[] paths);
    public void RemovePaths(IEnumerable<string> paths);
    public void AddIgnorePath(string path);
    public void AddIgnorePaths(params string[] paths);
    public void AddIgnorePaths(IEnumerable<string> paths);
    public void RemoveIgnorePath(string path);
    public void RemoveIgnorePaths(params string[] paths);
    public void RemoveIgnorePaths(IEnumerable<string> paths);
    event OnTrackPathsChanged? OnChanged;
}
