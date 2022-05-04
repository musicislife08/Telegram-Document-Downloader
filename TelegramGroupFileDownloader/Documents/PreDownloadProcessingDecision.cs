namespace TelegramGroupFileDownloader.Documents;

public enum PreDownloadProcessingDecision
{
    Nothing,
    ExistingDuplicate,
    SaveAndDownload,
    ReDownload,
    Update,
    Error
}