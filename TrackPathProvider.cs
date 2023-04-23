using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SaveSyncApp;

public class TrackPathProvider : ITrackPathProvider
{
    List<string> _paths { get; set; } = new List<string>();
    List<string> _ignorePaths { get; set; } = new List<string>();

    public event OnTrackPathsChanged? OnChanged;

    public void AddPath(string path)
    {
        path = SpecialFolders.ReplacePlaceholdersWithPaths(path);
        _paths.Add(path);
        OnChanged?.Invoke(this, new (path, ChangeAction.Add));
    }
    public void AddPaths(IEnumerable<string> paths)
    {
        foreach (var path in paths) AddPath(path);
    }
    public void AddPaths(params string[] paths) => AddPaths((IEnumerable<string>)paths);
    public void RemovePath(string path)
    {
        _paths.Remove(path);
        OnChanged?.Invoke(this, new(path, ChangeAction.Remove));
    }
    public void RemovePaths(IEnumerable<string> paths)
    {
        foreach (var path in paths) RemovePath(path);
    }
    public void RemovePaths(params string[] paths) => RemovePaths((IEnumerable<string>)paths);


    public void AddIgnorePath(string path)
    {
        path = SpecialFolders.ReplacePlaceholdersWithPaths(path);
        _ignorePaths.Add(path);
    }
    public void AddIgnorePaths(IEnumerable<string> paths)
    {
        foreach (var path in paths) AddIgnorePath(path);
    }
    public void AddIgnorePaths(params string[] paths) => AddIgnorePaths((IEnumerable<string>)paths);
    public void RemoveIgnorePath(string path)
    {
        _ignorePaths.Remove(path);
    }
    public void RemoveIgnorePaths(IEnumerable<string> paths)
    {
        foreach (var path in paths) RemoveIgnorePath(path);
    }
    public void RemoveIgnorePaths(params string[] paths) => RemoveIgnorePaths((IEnumerable<string>)paths);

    public IEnumerable<string> GetPaths() => _paths;
    public IEnumerable<string> GetIgnorePaths() => _ignorePaths;
}
