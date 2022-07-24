namespace TelegramTools.Uploader.Telegram;
public record UploadResult
{
    public UploadStatus Status { get; init; }
    public string Message { get; init; } = "";
}