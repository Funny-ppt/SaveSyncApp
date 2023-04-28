using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace SaveSyncApp;

public class Profile
{
    public int Version { get; set; }
    public bool EnableCompression { get; set; }
    public ConcurrentDictionary<string, ProfileItem>? Items { get; set; }

    public ObservableCollection<string>? TrackPaths { get; set; }
    public ObservableCollection<string>? IgnorePaths { get; set; }

    public ProfileItem? this[string path]
    {
        get => Items.GetValueOrDefault(path);
        set => Items[path] = value ?? throw new ArgumentNullException(nameof(path));
    }

    public static Profile LoadProfile(IServiceProvider services)
    {
        var profileProvider = services.GetRequiredService<IProfileProvider>();
        var profileVersionManagement = services.GetRequiredService<IProfileVersionManagement>();

        if (profileProvider.TryGetProfile(out var profile))
        {
            Debug.Assert(profile != null);
            profileVersionManagement.UpdateToCurrentVersion(profile);
        }
        else
        {
            profile = profileVersionManagement.GetDefaultProfile();
        }
        return profile;
    }
}
