using ByteSizeLib;
using TelegramTools.Downloader.Logging;

namespace TelegramTools.Downloader.Stats;

public class StatsService : IStatsService
{
    private readonly ILoggerService _logger;
    private long _uploadedFiles;
    private long _duplicateFiles;
    private long _filteredFiles;
    private long _erroredFiles;
    private long _existingFiles;
    private long _sizeBytes;

    public StatsService(ILoggerService logger)
    {
        _logger = logger;
        _logger.LogDebug($"Initializing {nameof(StatsService)}");
        _uploadedFiles = 0;
        _duplicateFiles = 0;
        _filteredFiles = 0;
        _erroredFiles = 0;
        _existingFiles = 0;
        _sizeBytes = 0;
    }

    public void AddStat(StatType type)
    {
        if (type is StatType.Successful)
            throw new ArgumentException("StatType cannot be Successful without size argument");
        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (type)
        {
            case StatType.Duplicated:
                _duplicateFiles += 1;
                break;
            case StatType.Filtered:
                _filteredFiles += 1;
                break;
            case StatType.Existing:
                _existingFiles += 1;
                break;
            case StatType.Errored:
                _erroredFiles += 1;
                break;
        }
        
    }
    public void AddStat(long? sizeBytes, StatType type)
    {
        if (sizeBytes is null)
            throw new ArgumentNullException(nameof(sizeBytes));
        if (type is not StatType.Successful)
            throw new ArgumentException("StatsType with size can only be type Successful");
        _logger.LogDebug($"Adding Stat.  Bytes: {sizeBytes}, StatType: {Enum.GetName(type)}");
        _sizeBytes += (long)sizeBytes; 
        _uploadedFiles += 1;
    }

    public StatResult GetStats()
    {
        _logger.LogDebug(
            $"Getting Stats.  Dupe: {_duplicateFiles}, Error: {_erroredFiles}, Filtered: {_filteredFiles}, Uploaded: {_uploadedFiles}, Existing: {_existingFiles}, Size: {ByteSize.FromBytes(_sizeBytes).ToBinaryString()}");
        return new StatResult()
        {
            Duplicate = _duplicateFiles,
            Errored = _erroredFiles,
            Filtered = _filteredFiles,
            Uploaded = _uploadedFiles,
            Existing = _existingFiles,
            UploadedBytes = _sizeBytes
        };
    }
}