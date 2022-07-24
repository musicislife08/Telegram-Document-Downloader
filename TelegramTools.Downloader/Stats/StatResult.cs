namespace TelegramTools.Downloader.Stats;

public record StatResult
{
    public long Uploaded { get; set; }
    public long Duplicate { get; set; }
    public long Errored { get; set; }
    public long Existing { get; set; }
    public long Filtered { get; set; }
    public long UploadedBytes { get; set; }
};