using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using TelegramTools.Downloader.Archive;
using TelegramTools.Downloader.Config;
using TelegramTools.Downloader.Logging;
using TelegramTools.Downloader.Queue;
using TelegramTools.Downloader.Stats;

namespace TelegramTools.Downloader.Files;

internal class FileWorker
{
    private readonly ILoggerService _logger;
    private readonly IQueueService _queue;
    private readonly TgConfig _config;
    private readonly IArchiveService _archiveService;
    private readonly IStatsService _statsService;

    public FileWorker(ILoggerService logger, IQueueService queue, IArchiveService archiveService,
        IStatsService statsService, IOptions<TgConfig> config)
    {
        logger.LogDebug($"Initializing {nameof(FileWorker)}");
        _logger = logger;
        _queue = queue;
        _config = config.Value;
        _archiveService = archiveService;
        _statsService = statsService;
    }

    public async Task RunAsync(CancellationToken stoppingToken)
    {
        var files = Directory.GetFiles(_config.DownloadPath!, "*", SearchOption.AllDirectories);
        var archiveFiles = new List<string>();
        foreach (var file in files)
        {
            var info = new FileInfo(file);
            if (IsArchive(info.Extension))
            {
                archiveFiles.Add(file);
                continue;
            }

            await ProcessFile(info, stoppingToken);
        }

        foreach (var archive in archiveFiles)
        {
            var aFiles = await _archiveService.UnArchive(archive);
            foreach (var aFile in aFiles)
            {
                await ProcessFile(new FileInfo(aFile), stoppingToken);
            }
        }
    }

    private async Task ProcessFile(FileInfo info, CancellationToken stoppingToken)
    {
        var wantedExtensions = _config.DocumentExtensionFilter!.Split(",");
        if (!wantedExtensions.Contains(info.Extension))
        {
            _logger.LogInformation($"Skipping filtered extension: {info.Name}");
            _statsService.AddStat(StatType.Filtered);
            return;
        }

        using var sha256 = SHA256.Create();
        await using var stream = info.OpenRead();
        var hash = await sha256.ComputeHashAsync(stream, stoppingToken);
        var doc = new UploadDocument()
        {
            DocumentPath = info.FullName,
            DocumentSize = info.Length,
            Sha256Hash = hash
        };
        _queue.Put(doc);
        _logger.LogInformation($"Added {info.Name}");
    }

    private static bool IsArchive(string extension)
    {
        var archives = new[]
        {
            ".zip",
            ".rar",
            ".7z",
            ".tar",
            ".tar.gz"
        };
        if (string.IsNullOrWhiteSpace(extension))
            throw new ArgumentNullException(nameof(extension));
        return archives.Contains(extension);
    }
}