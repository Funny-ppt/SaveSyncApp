using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace SaveSyncApp;

/// <summary>
/// StartupControl.xaml 的交互逻辑
/// </summary>
public partial class StartupControl : UserControl
{
    public class ViewModel : INotifyPropertyChanged
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

        public event PropertyChangedEventHandler PropertyChanged;

        public ViewModel()
        {
            App.Context.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(SaveSync))
                {
                    PropertyChanged(IsSaveSyncActive, new(nameof(IsSaveSyncActive)));
                }
            };
        }
    }
    public ViewModel Model { get; }

    public StartupControl()
    {
        InitializeComponent();

        DataContext = Model = new();
        App.Current.LogMessageImpl += (msg, name) => Model.Logs += $"{DateTime.Now} {msg}";
    }

    private void SwitchStateButton_Click(object sender, RoutedEventArgs e)
    {
        App.Context.SaveSync = App.Context.SaveSync == null ? new SaveSync(App.Context.ServiceProvider) : null;
    }
}
