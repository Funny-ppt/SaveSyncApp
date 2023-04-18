using System;

namespace SaveSyncApp;

public class ProfileItem
{
    public string ProcessName { get; set; }
    public string UserFriendlyName { get; set; }
    public string SavePath { get; set; }
    public DateTime RecentChangeDate { get; set; }
}
