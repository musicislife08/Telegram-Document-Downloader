namespace TelegramTools.LegacyDownloader.Documents;

public enum PreDownloadProcessingDecision
{
    Nothing,
    ExistingDuplicate,
    SaveAndDownload,
    ReDownload,
    Update,
    Error
}