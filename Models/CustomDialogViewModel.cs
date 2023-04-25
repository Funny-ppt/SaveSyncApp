using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SaveSyncApp;

public class CustomDialogViewModel : INotifyPropertyChanged
{
    private string _title = "";
    private string _content = "";
    private readonly Dictionary<string, string> _arguments = new();

    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            OnPropertyChanged(nameof(Title));
        }
    }

    public string Content
    {
        get => _content;
        set
        {
            _content = value;
            OnPropertyChanged(nameof(Content));
        }
    }

    public void Add(string key, string value)
    {
        _arguments.Add(key, value);
    }

    public string this[string key] { get => _arguments[key]; set => _arguments[key] = value; }

    public IReadOnlyDictionary<string, string> Arguments => _arguments;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
