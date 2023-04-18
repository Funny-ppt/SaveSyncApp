using System.Collections.Generic;

namespace SaveSyncApp;

public delegate void OnTrackPathsChanged(object sender, TrackPathsChangeEventArgs e);
public interface ITrackPathProvider
{
    IEnumerable<string> GetPaths();
    IEnumerable<string> GetIgnorePaths();
    public void AddPath(string path);
    public void RemovePath(string path);
    event OnTrackPathsChanged OnChanged;
}
