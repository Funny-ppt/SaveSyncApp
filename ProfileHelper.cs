using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;

namespace SaveSyncApp;

public static class ProfileHelper
{
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
