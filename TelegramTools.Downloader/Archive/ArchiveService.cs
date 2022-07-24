using Microsoft.Extensions.Options;
using SharpCompress.Common;
using SharpCompress.Readers;
using TelegramTools.Downloader.Config;
using TelegramTools.Downloader.Logging;

namespace TelegramTools.Downloader.Archive;

class ArchiveService : IArchiveService
{
    private readonly ILoggerService _logger;
    private readonly TgConfig _config;

    public ArchiveService(ILoggerService logger, IOptions<TgConfig> config)
    {
        _logger = logger;
        _logger.LogDebug($"{nameof(ArchiveService)} Initialized");
        _config = config.Value;
    }
    public async Task<string[]> UnArchive(string file)
    {
        if (string.IsNullOrWhiteSpace(file))
            throw new ArgumentNullException(nameof(file));
        var info = new FileInfo(file);
        var dir = Path.Combine(_config.TempPath, info.Name[..^info.Extension.Length]);
        Directory.CreateDirectory(dir);
        _logger.LogDebug($"Unarchiving {info.Name} to {dir}");
        await using var stream = info.OpenRead();
        var reader = ReaderFactory.Open(stream);
        while (reader.MoveToNextEntry())
        {
            if (!reader.Entry.IsDirectory)
            {
                reader.WriteEntryToDirectory(dir, new ExtractionOptions() {ExtractFullPath = false, Overwrite = true});
            }
        }
        return Directory.GetFiles(dir);
    }
}