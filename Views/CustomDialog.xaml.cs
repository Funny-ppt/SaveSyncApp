using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace SaveSyncApp;

/// <summary>
/// CustomDialog.xaml 的交互逻辑
/// </summary>
public partial class CustomDialog : Window
{
    public CustomDialogViewModel ViewModel => (CustomDialogViewModel)DataContext;

    // optimize: 如果SaveSync在此前是前台窗口，则显示在其中央，否则显示在屏幕中央
    public CustomDialog()
    {
        InitializeComponent();
    }

    private void Initialize(string title, string content, (string buttonText, string buttonValue)[] buttons, (string key, string value)[] args)
    {
        ViewModel.Title = title;
        ViewModel.Content = content;

        foreach (var (key, value) in args)
        {
            ViewModel.Add(key, value);
        }

        foreach (var (buttonText, buttonValue) in buttons)
        {
            var button = new Button { Content = buttonText, Margin = new Thickness(0, 0, 10, 0), Width = 60 };
            button.Click += (sender, e) => {
                ViewModel["action"] = buttonValue;
                DialogResult = true;
                Close();
            };
            ButtonPanel.Children.Add(button);
        }
    }

    public static IReadOnlyDictionary<string, string>? ShowDialog(string title, string content, (string buttonText, string buttonValue)[] buttons, string? defaultAction = null, (string key, string value)[]? args = null)
        => App.Current.Dispatcher.Invoke(() => ShowDialogImpl(title, content, buttons, defaultAction, args));
    static IReadOnlyDictionary<string, string>? ShowDialogImpl(string title, string content, (string buttonText, string buttonValue)[] buttons, string? defaultAction, (string key, string value)[]? args)
    {
        var customDialog = new CustomDialog();
        customDialog.Initialize(title, content, buttons, args ?? Array.Empty<(string, string)>());

        // 如果当前上下文是窗口，请使用以下代码将对话框的Owner设置为当前窗口
        // customDialog.Owner = Application.Current.MainWindow;

        bool? dialogResult = customDialog.ShowDialog();

        if (dialogResult.HasValue && dialogResult.Value)
        {
            return customDialog.ViewModel.Arguments;
        }

        return defaultAction == null ? null : new Dictionary<string, string>() { { "action", defaultAction } };
    }
}
