namespace TelegramTools.Downloader.Logging;

public class LogEntryError : LogEntryBase
{
    public Exception? Exception { get; set; }
}