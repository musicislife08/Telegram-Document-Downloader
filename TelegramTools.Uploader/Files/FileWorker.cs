using System.Security.Cryptography;
using TelegramTools.Uploader.Archive;
using TelegramTools.Uploader.Config;
using TelegramTools.Uploader.Queue;
using TelegramTools.Uploader.Stats;

namespace TelegramTools.Uploader.Files;

internal class FileWorker
{
    private readonly ILogger<FileWorker> _logger;
    private readonly IQueueService _queue;
    private readonly TgConfig _config;
    private readonly IArchiveService _archiveService;
    private readonly IStatsService _statsService;

    public FileWorker(ILogger<FileWorker> logger, IQueueService queue, IArchiveService archiveService, IStatsService statsService, TgConfig config)
    {
        logger.LogDebug("Initializing {Worker}", nameof(FileWorker));
        _logger = logger;
        _queue = queue;
        _config = config;
        _archiveService = archiveService;
        _statsService = statsService;
    }

    public async Task RunAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("{Name} running at: {Time}",nameof(FileWorker), DateTimeOffset.Now);
            var files = Directory.GetFiles(_config.UploadPath!, "*", SearchOption.AllDirectories);
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
            _logger.LogInformation("Skipping filtered extension: {Name}", info.Name);
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
        _logger.LogInformation("Added {Name}", info.Name);
    }

    private static bool IsArchive(string extension)
    {
        var archives = new string[]
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