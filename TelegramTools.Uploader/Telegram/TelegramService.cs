using ByteSizeLib;
using MimeDetective;
using MimeDetective.Definitions;
using MimeDetective.Definitions.Licensing;
using TelegramTools.Uploader.Config;
using TelegramTools.Uploader.Files;
using TL;
using WTelegram;
using ConfigurationManager = TelegramTools.Uploader.Config.ConfigurationManager;

namespace TelegramTools.Uploader.Telegram;

public class TelegramService : ITelegramService
{
    private readonly ILogger<TelegramService> _logger;
    private readonly TgConfig _config;
    private readonly Client _client;

    public TelegramService(ILogger<TelegramService> logger, TgConfig config)
    {
        _logger = logger;
        _logger.LogDebug("{Name} Initializing", nameof(TelegramService));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _client = new Client(ConfigurationManager.WTelegramConfig);
        _client.CollectAccessHash = true;
        _client.PingInterval = 60;
        _client.MaxAutoReconnects = 30;
    }
    public async Task<UploadResult> UploadDocumentAsync(UploadDocument? document)
    {
        if (document is null)
            throw new ArgumentNullException(nameof(document));
        if (string.IsNullOrWhiteSpace(document.DocumentPath))
            throw new NullReferenceException($"{nameof(document.DocumentPath)} Cannot Be blank");
        if (document.Sha256Hash is null)
            throw new NullReferenceException($"{nameof(document.Sha256Hash)} Cannot Be null");
        if (document.DocumentSize is null)
            throw new NullReferenceException($"{nameof(document.DocumentSize)} Cannot be null");
        _logger.LogDebug("Starting File Upload.  File: {File}", document.DocumentPath);
        await _client.LoginUserIfNeeded();
        var groups = await _client.Messages_GetAllChats();
        var group = (Channel)groups.chats.First(x => x.Value.Title == _config.GroupName && x.Value.IsActive).Value;
        var hc = _client.GetAccessHashFor<Channel>(group.ID);
        var info = new FileInfo(document.DocumentPath);
        var inspector = new ContentInspectorBuilder() {
            Definitions = new ExhaustiveBuilder() {
                UsageType = UsageType.PersonalNonCommercial
            }.Build()
        }.Build();
        var mime = inspector.Inspect(document.DocumentPath).OrderByDescending(x => x.Points);
        var mimeType = mime.First().Definition.File.MimeType;
        var msgs = await _client.Messages_Search(new InputPeerChannel(group.ID, hc), info.Name, new InputMessagesFilterDocument());
        // var docs = msgs.Messages;
        // var msg = docs.First() as Message;
        // var msgMedia = msg.media as MessageMediaDocument;
        // var doc = msgMedia.document as Document;
        // var hash = await client.Upload_GetFileHashes(doc.ToFileLocation());
        // //var doc = msg. as Document;
        // var matches = document.Sha256Hash == hash.First().hash;
        // var rawDocumentResult = await client.Messages_GetDocumentByHash(doc.file_reference, (int)document.DocumentSize, mimeType);
        if (msgs.Count > 0)
        {
            return new UploadResult()
            {
                Status = UploadStatus.Duplicate,
                Message = $"{info.Name} Already exists"
            };
        }

        try
        {
            _logger.LogInformation("Uploading {Name}", info.Name);
            await using var stream = File.OpenRead(document.DocumentPath);
            var result = await _client.UploadFileAsync(stream, info.Name, Progress);
            await _client.SendMediaAsync(group, "", result);
        }
        catch (RpcException e)
        {
            return new UploadResult()
            {
                Status = UploadStatus.Errored,
                Message = $"Upload Error: {e.Message}"
            };
        }
        return new UploadResult() {Status = UploadStatus.Successful};
    }

    private void Progress(long a, long b)
    {
        _logger.LogInformation("Progressing: {A} of {B}", ByteSize.FromBytes(a).ToString(), ByteSize.FromBytes(b).ToString());
    }
}