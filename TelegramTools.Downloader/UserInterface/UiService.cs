using TelegramTools.Downloader.Logging;
using TelegramTools.Downloader.Stats;

namespace TelegramTools.Downloader.UserInterface;

public class UiService : IUiService
{
    private readonly ILoggerService _logger;
    private readonly IStatsService _stats;

    public UiService(ILoggerService loggerService, IStatsService statsService)
    {
        _logger = loggerService;
        _stats = statsService;
    }
    public async Task RunAsync()
    {
        throw new NotImplementedException();
    }
}