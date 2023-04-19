using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SaveSyncApp;

public static class LoggerExtensions
{
    public static ILoggingBuilder AddCustom(this ILoggingBuilder builder)
    {
        builder.Services.AddSingleton<ILoggerProvider>(new LoggerProvider());
        return builder;
    }
}
