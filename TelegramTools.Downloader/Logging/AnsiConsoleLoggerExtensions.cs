using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TelegramTools.Downloader.Logging;

public static class AnsiConsoleLoggerExtensions
{
    public static ILoggingBuilder AddAnsiConsoleLogger(this ILoggingBuilder builder)
    {
        builder.Services.AddSingleton<ILoggerProvider, AnsiConsoleLoggerProvider>();
        return builder;
    }
}