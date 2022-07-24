using TelegramTools.Downloader.Files;
using TelegramTools.Downloader.Logging;

namespace TelegramTools.Downloader.Queue;

public interface IQueueService
{
    void Put(UploadDocument? document);
    UploadDocument? Pull();
    int Count();

    void Put(LogEntryBase? entry);
    LogEntryBase? PullLogs();
    int LogsCount();
}