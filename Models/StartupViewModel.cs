using System.ComponentModel;

namespace SaveSyncApp;

public class StartupViewModel : INotifyPropertyChanged
{
    string _logs = string.Empty;

    public bool IsSaveSyncActive => App.Context.SaveSync != null;
    public string Logs
    {
        get => _logs;
        set
        {
            _logs = value;
            PropertyChanged?.Invoke(this, new(nameof(Logs)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public StartupViewModel()
    {
        App.Context.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == "SaveSync")
            {
                PropertyChanged?.Invoke(this, new(nameof(IsSaveSyncActive)));
            }
        };
    }
}