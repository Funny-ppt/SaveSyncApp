using Microsoft.Extensions.Logging;
using System;

namespace SaveSyncApp;

public class Logger : ILogger
{
    readonly string _name;

    public Logger(string name)
    {
        _name = name;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null; // todo
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (formatter == null) throw new ArgumentNullException(nameof(formatter));

        var message = formatter(state, exception);

        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        message = $"{logLevel}: {message}";

        if (exception != null)
        {
            message += Environment.NewLine + Environment.NewLine + exception;
        }

        App.Current.LogMessage(message, _name);
    }
}
