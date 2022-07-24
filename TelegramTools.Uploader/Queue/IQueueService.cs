using TelegramTools.Uploader.Files;

namespace TelegramTools.Uploader.Queue;

public interface IQueueService
{
    void Put(UploadDocument? document);
    UploadDocument? Pull();
    int Count();
}