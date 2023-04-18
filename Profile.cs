using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SaveSyncApp;

public class Profile
{
    public ConcurrentDictionary<string, ProfileItem> Items { get; set; }
    public string SyncPath { get; set; }

    public List<string> TrackPaths { get; set; }
    public List<string> IgnorePaths { get; set; }

    public ProfileItem this[string path]
    {
        get => Items.TryGetValue(path, out ProfileItem item) ? item : null;
        set => Items[path] = value;
    }
}
