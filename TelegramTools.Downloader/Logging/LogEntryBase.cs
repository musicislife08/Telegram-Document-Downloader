using Microsoft.Extensions.Logging;

namespace TelegramTools.Downloader.Logging;

public abstract class LogEntryBase
{
    public LogLevel LogLevel { get; set; }
    public string? Message { get; set; }
    public DateTimeOffset DateTime { get; } = DateTimeOffset.Now;
}