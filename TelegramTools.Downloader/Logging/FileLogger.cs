namespace TelegramTools.Downloader.Logging;

public static class FileLogger
{
    private static string _errorFile;
    private static string _logFile;

    static FileLogger()
    {
        _errorFile = Path.Combine(Environment.CurrentDirectory, "errors.log");
        _logFile = Path.Combine(Environment.CurrentDirectory, "logs.log");
    }

    static void WriteLogToFile(LogEntryBase logEntry)
    {
        switch (logEntry)
        {
            case LogEntry:
                File.WriteAllText(_logFile,
                    $"[{logEntry.LogLevel.ToString()}] [{logEntry.DateTime.ToLocalTime()}]: {logEntry.Message}");
                break;
            case LogEntryError entry:
                File.WriteAllText(_errorFile, 
                    $"[{entry.LogLevel.ToString()}] [{entry.DateTime.ToLocalTime()}]: {entry.Message}");
                if (entry.Exception?.StackTrace is null)
                    break;
                File.WriteAllText(_errorFile, entry.Exception.StackTrace.ToString());
                break;
            default:
                throw new ArgumentNullException(nameof(logEntry));
        }
    }
}