using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace TelegramTools.Downloader.Logging;

public class AnsiConsoleLogger : ILogger
{
    private readonly string _name;
    private readonly AnsiConsoleLoggerProvider _provider;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);
        var exceptionMessage = exception != null ? exception.StackTrace : string.Empty;
        switch (logLevel)
        {
            case LogLevel.Trace:
                AnsiConsole.MarkupLineInterpolated($"[grey][{logLevel.ToString()}] {DateTimeOffset.Now}:[/] {message}");
                break;
            case LogLevel.Debug:
                AnsiConsole.MarkupLineInterpolated($"[grey][{logLevel.ToString()}] {DateTimeOffset.Now}:[/] {message}");
                break;
            case LogLevel.Information:
                AnsiConsole.MarkupLineInterpolated($"[blue][{logLevel.ToString()}] {DateTimeOffset.Now}:[/] {message}");
                break;
            case LogLevel.Warning:
                AnsiConsole.MarkupLineInterpolated($"[yellow][{logLevel.ToString()}] {DateTimeOffset.Now}:[/] {message}");
                break;
            case LogLevel.Error:
                AnsiConsole.MarkupLineInterpolated($"[orange1][{logLevel.ToString()}] {DateTimeOffset.Now}:[/] {message}");
                AnsiConsole.MarkupLineInterpolated($"[orange1]{exceptionMessage}");
                break;
            case LogLevel.Critical:
                AnsiConsole.MarkupLineInterpolated($"[red][{logLevel.ToString()}] {DateTimeOffset.Now}:[/] {message}");
                break;
            case LogLevel.None:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
        }
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None;
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        return null!;
    }
}