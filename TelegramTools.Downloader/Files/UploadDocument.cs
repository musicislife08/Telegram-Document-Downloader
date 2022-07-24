namespace TelegramTools.Downloader.Files;

public class UploadDocument
{
    public string? DocumentPath { get; set; }
    public string? DescriptionDocumentPath { get; set; }
    public string? CoverImagePath { get; set; }
    public byte[]? Sha256Hash { get; set; }
    public long? DocumentSize { get; set; }
    public int? CoverImageSize { get; set; }
}