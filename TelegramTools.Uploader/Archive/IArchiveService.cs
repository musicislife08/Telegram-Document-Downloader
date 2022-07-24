namespace TelegramTools.Uploader.Archive;

public interface IArchiveService
{
    Task<string[]> UnArchive(string file);
}