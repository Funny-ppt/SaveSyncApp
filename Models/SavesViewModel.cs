using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace SaveSyncApp;

public class SavesViewModel : INotifyPropertyChanged
{
    public bool IsSaveSyncActive => App.Context.SaveSync != null;
    public IEnumerable<ProfileItem> Saves => App.Context.Profile.Items.Values.OrderByDescending(i => i.RecentChangeDate);

    public void RefreshProfile()
    {
        PropertyChanged?.Invoke(this, new(nameof(Saves)));
    }
    public void RefreshProfileCache(bool save)
    {
        App.Context.RefreshProfileCache(save);
        PropertyChanged?.Invoke(this, new(nameof(Saves)));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public SavesViewModel()
    {
        App.Context.PropertyChanged += (sender, e) =>
        {
            switch (e.PropertyName)
            {
                case "SaveSync":
                    PropertyChanged?.Invoke(this, new(nameof(IsSaveSyncActive)));
                    break;
                case "Profile":
                    PropertyChanged?.Invoke(this, new(nameof(Saves)));
                    break;
            }
        };
    }
}