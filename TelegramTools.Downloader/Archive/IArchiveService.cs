namespace TelegramTools.Downloader.Archive;

public interface IArchiveService
{
    Task<string[]> UnArchive(string file);
}