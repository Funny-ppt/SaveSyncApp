using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaveSyncApp;

public class TrackPathProvider : ITrackPathProvider
{
    List<string> _paths { get; set; } = new List<string>();

    public event OnTrackPathsChanged OnChanged;

    public void AddPath(string path)
    {
        _paths.Add(path);
        OnChanged?.Invoke(this, new (path, ChangeAction.Add));
    }
    public void AddPaths(IEnumerable<string> paths)
    {
        foreach (var path in paths) AddPath(path);
    }
    public void AddPaths(params string[] path) => AddPaths((IEnumerable<string>)path);
    public void RemovePath(string path)
    {
        _paths.Remove(path);
        OnChanged?.Invoke(this, new(path, ChangeAction.Remove));
    }
    public void RemovePaths(IEnumerable<string> paths)
    {
        foreach (var path in paths) RemovePath(path);
    }
    public void RemovePaths(params string[] path) => RemovePaths((IEnumerable<string>)path);

    public IEnumerable<string> GetPaths() => _paths;
    public IEnumerable<string> GetIgnorePaths() => Enumerable.Empty<string>(); // todo
}
