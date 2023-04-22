using System;
using System.Text.Json.Serialization;

namespace SaveSyncApp;

public class ProfileItem
{
    public string? ProcessName { get; set; }
    public string? UserFriendlyName { get; set; }
    public string? SavePath { get; set; }
    [JsonIgnore]
    public string? IconPath { get; set; }
    public DateTime RecentChangeDate { get; set; }
}
