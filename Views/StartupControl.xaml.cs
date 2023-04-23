using SaveSyncApp.Properties;
using System;
using System.Windows;
using System.Windows.Controls;

namespace SaveSyncApp;

/// <summary>
/// StartupControl.xaml 的交互逻辑
/// </summary>
public partial class StartupControl : UserControl
{
    public StartupViewModel Model { get; }

    public StartupControl()
    {
        InitializeComponent();

        DataContext = Model = new();
        App.Current.LogMessageImpl += (msg, name) => Model.Logs += $"{DateTime.Now} {msg}\n";


        if (Settings.Default.RunAfterStart || Environment.CommandLine.Contains("--run-after-start"))
        {
            App.Context.StartNewSaveSync();
        }
    }

    private void SwitchStateButton_Click(object sender, RoutedEventArgs e)
    {
        if (Model.IsSaveSyncActive)
        {
            App.Context.SaveSync = null;
        }
        else
        {
            App.Context.StartNewSaveSync();
        }
    }

    private void LogTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var scrollViewer = LogScrollViewer;

        if (scrollViewer != null)
        {
            bool isAtBottom = Math.Abs(scrollViewer.VerticalOffset - scrollViewer.ScrollableHeight) < 1;

            if (isAtBottom)
            {
                scrollViewer.ScrollToEnd();
            }
        }
    }
}
