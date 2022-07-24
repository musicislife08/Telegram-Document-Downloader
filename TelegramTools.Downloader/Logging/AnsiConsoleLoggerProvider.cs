using Microsoft.Extensions.Logging;

namespace TelegramTools.Downloader.Logging;

public class AnsiConsoleLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return new AnsiConsoleLogger();
    }
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}