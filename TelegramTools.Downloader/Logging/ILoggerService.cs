namespace TelegramTools.Downloader.Logging;

public interface ILoggerService
{
    void LogInformation(string? message);
    void LogDebug(string? message);
    void LogError(Exception exception, string? message);
    List<LogEntryBase> GetLogs();
}