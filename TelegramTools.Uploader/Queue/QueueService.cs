using System.Collections.Concurrent;
using TelegramTools.Uploader.Files;

namespace TelegramTools.Uploader.Queue;

public class QueueService : IQueueService
{
    private readonly ILogger<QueueService> _logger;
    private readonly ConcurrentQueue<UploadDocument> _queue;
    private int _count;

    public QueueService(ILogger<QueueService> logger)
    {
        _logger = logger;
        _queue = new ConcurrentQueue<UploadDocument>();
        _count = 0;
    }
    public void Put(UploadDocument? document)
    {
        if (document is null)
            throw new ArgumentNullException(nameof(document));
        if (string.IsNullOrWhiteSpace(document.DocumentPath))
            throw new ArgumentNullException(nameof(document));
        _queue.Enqueue(document);
        _count += 1;
        _logger.LogDebug("Added {Document} to queue", document.DocumentPath);
    }

    public UploadDocument? Pull()
    {
        var success = _queue.TryDequeue(out var res);
        if (!success) return null;
        _count -= 1;
        return res;
    }

    public int Count()
    {
        return _count;
    }
}