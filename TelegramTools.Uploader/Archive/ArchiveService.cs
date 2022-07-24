using SharpCompress.Common;
using SharpCompress.Readers;
using TelegramTools.Uploader.Config;

namespace TelegramTools.Uploader.Archive;

class ArchiveService : IArchiveService
{
    private readonly ILogger<ArchiveService> _logger;
    private readonly TgConfig _config;

    public ArchiveService(ILogger<ArchiveService> logger, TgConfig config)
    {
        _logger = logger;
        _logger.LogDebug("{Name} Initialized", nameof(ArchiveService));
        _config = config;
    }
    public async Task<string[]> UnArchive(string file)
    {
        if (string.IsNullOrWhiteSpace(file))
            throw new ArgumentNullException(nameof(file));
        var info = new FileInfo(file);
        var dir = Path.Combine(_config.TempPath, info.Name[..^info.Extension.Length]);
        Directory.CreateDirectory(dir);
        _logger.LogDebug("Unarchiving {Name} to {Directory}", info.Name, dir);
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