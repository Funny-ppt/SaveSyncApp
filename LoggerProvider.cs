using Microsoft.Extensions.Logging;

namespace SaveSyncApp;

public class LoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new Logger(categoryName);

    public void Dispose()
    {
        // 暂时没有输出到文件/网络位置的功能，没有需要清理的资源
    }
}