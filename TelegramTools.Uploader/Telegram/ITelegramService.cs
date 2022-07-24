using TelegramTools.Uploader.Files;

namespace TelegramTools.Uploader.Telegram;

public interface ITelegramService
{
    Task<UploadResult> UploadDocumentAsync(UploadDocument document);
}