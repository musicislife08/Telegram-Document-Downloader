using TelegramTools.Uploader.Config;
using TelegramTools.Uploader.Files;
using TelegramTools.Uploader.Queue;
using TelegramTools.Uploader.Stats;

namespace TelegramTools.Uploader.Telegram;

public class TelegramWorker
{
    private readonly ILogger<TelegramWorker> _logger;
    private readonly IQueueService _queue;
    private readonly TgConfig _config;
    private readonly ITelegramService _telegramService;
    private readonly IStatsService _statsService;

    public TelegramWorker(ILogger<TelegramWorker> logger, IQueueService queue, ITelegramService telegramService, IStatsService statsService, TgConfig config)
    {
        logger.LogDebug("Initializing {Worker}", nameof(FileWorker));
        _logger = logger;
        _queue = queue;
        _config = config;
        _telegramService = telegramService;
        _statsService = statsService;
    }

    public async Task RunAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("{Name} running at {Time}", nameof(TelegramWorker), DateTimeOffset.Now);
        while (!stoppingToken.IsCancellationRequested)
        {
            var item = _queue.Pull();
            if (item is null)
            {
                _logger.LogWarning("Queue Empty.  Exiting");
                break;
            }
            var result = await _telegramService.UploadDocumentAsync(item);
            var info = new FileInfo(item.DocumentPath!);
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (result.Status)
            {
                case UploadStatus.Successful:
                    _logger.LogInformation("Successful Upload of {Name}", info.Name);
                    _statsService.AddStat(item.DocumentSize, StatType.Successful);
                    break;
                case UploadStatus.Errored:
                    _logger.LogWarning("Errored uploading {Name}: {Message}", info.Name, result.Message);
                    _statsService.AddStat(StatType.Errored);
                    break;
                case UploadStatus.Duplicate:
                    _logger.LogInformation("File exists in group.  Skipping Upload for {Name}", info.Name);
                    _statsService.AddStat(StatType.Duplicated);
                    break;
                case UploadStatus.Filtered:
                    _logger.LogInformation("Filtered on extension.  Skipping Upload for {Name}", info.Name);
                    _statsService.AddStat(StatType.Filtered);
                    break;
                case UploadStatus.NotAllowed:
                    break;
            }
            _logger.LogInformation("Files Left To Upload: {Count}", _queue.Count());
        }
    }
}