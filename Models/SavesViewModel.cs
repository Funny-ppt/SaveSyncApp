using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace SaveSyncApp;

public class SavesViewModel : INotifyPropertyChanged
{
    public bool IsSaveSyncActive => App.Context.SaveSync != null;
    public Profile Profile => App.Context.Profile;

    public void RefreshProfile()
    {
        PropertyChanged?.Invoke(this, new(nameof(Profile)));
    }
    public void RefreshProfileCache()
    {
        App.Context.RefreshProfileCache();
        PropertyChanged?.Invoke(this, new(nameof(Profile)));
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
                    PropertyChanged?.Invoke(this, new(nameof(Profile)));
                    break;
            }
        };
    }
}