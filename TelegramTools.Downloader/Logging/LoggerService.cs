using Microsoft.Extensions.Logging;
using TelegramTools.Downloader.Queue;

namespace TelegramTools.Downloader.Logging;

public class LoggerService : ILoggerService
{
    private readonly IQueueService _queue;
    private readonly List<LogEntryBase> _logs;

    public LoggerService(IQueueService queueService)
    {
        _queue = queueService;
        _logs = new List<LogEntryBase>();
    }
    public void LogInformation(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentNullException(nameof(message));
        _logs.Add(new LogEntry()
        {
            LogLevel = LogLevel.Information,
            Message = message
        });
    }

    public void LogDebug(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentNullException(nameof(message));
        _logs.Add(new LogEntry()
        {
            LogLevel = LogLevel.Debug,
            Message = message
        });
    }

    public void LogError(Exception? exception, string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentNullException(nameof(message));
        if (exception is null)
        {
            _logs.Add(new LogEntry()
            {
                LogLevel = LogLevel.Information,
                Message = message
            });
            return;
        }
        _logs.Add(new LogEntryError()
        {
            LogLevel = LogLevel.Error,
            Message = message,
            Exception = exception
        });
    }

    public List<LogEntryBase> GetLogs()
    {
        return _logs;
    }

    public void RemoveLogs(IEnumerable<LogEntryBase> logs)
    {
        foreach (var VARIABLE in _logs)
        {
            
        }
    }
}