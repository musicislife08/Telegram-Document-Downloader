using System.Collections.Concurrent;
using TelegramTools.Downloader.Files;
using TelegramTools.Downloader.Logging;

namespace TelegramTools.Downloader.Queue;

public class QueueService : IQueueService
{
    private readonly ConcurrentQueue<UploadDocument> _queue;
    private readonly ConcurrentQueue<LogEntryBase> _logQueue;
    private int _count;
    private int _logCount;
    public QueueService()
    {
        _queue = new ConcurrentQueue<UploadDocument>();
        _logQueue = new ConcurrentQueue<LogEntryBase>();
        _count = 0;
        _logCount = 0;
    }
    public void Put(UploadDocument? document)
    {
        if (document is null)
            throw new ArgumentNullException(nameof(document));
        if (string.IsNullOrWhiteSpace(document.DocumentPath))
            throw new ArgumentNullException(nameof(document));
        _queue.Enqueue(document);
        _count += 1;
    }

    public UploadDocument? Pull()
    {
        _queue.TryDequeue(out var res);
        _count -= 1;
        return res;
    }

    public int Count()
    {
        return _count;
    }

    public void Put(LogEntryBase? entry)
    {
        if (entry is null)
            throw new ArgumentNullException(nameof(entry));
        if (string.IsNullOrWhiteSpace(entry.Message))
            throw new ArgumentNullException(nameof(entry));
        if (entry is LogEntryError { Exception: null })
            throw new ArgumentNullException(nameof(entry));
        _logQueue.Enqueue(entry);
        _logCount += 1;
    }

    public LogEntryBase? PullLogs()
    {
        _logQueue.TryDequeue(out var res);
        _logCount -= 1;
        return res;
    }

    public int LogsCount()
    {
        return _logCount;
    }
}