using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SaveSyncApp;

public class Profile
{
    public int Version { get; set; }
    public ConcurrentDictionary<string, ProfileItem>? Items { get; set; }

    public List<string>? TrackPaths { get; set; }
    public List<string>? IgnorePaths { get; set; }

    public ProfileItem? this[string path]
    {
        get => Items.GetValueOrDefault(path);
        set => Items[path] = value ?? throw new ArgumentNullException(nameof(path));
    }
}
