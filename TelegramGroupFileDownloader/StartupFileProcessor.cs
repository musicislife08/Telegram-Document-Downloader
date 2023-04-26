using Microsoft.EntityFrameworkCore;
using TelegramGroupFileDownloader.Database;

namespace TelegramGroupFileDownloader;

public static class StartupFileProcessor
{
    public static async Task ProcessExistingFiles(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentNullException(nameof(path));
        await using var db = new DocumentContext();
        var dir = new DirectoryInfo(path);
        var files = dir.GetFiles();
        foreach (var file in files)
        {
            var hash = Utilities.GetFileHash(file.FullName);
            var existing = await db.DocumentFiles.AnyAsync(x => x.Hash == hash);
            if (!existing)
            {
                await db.DocumentFiles.AddAsync(new()
                {
                    Name = file.Name,
                    FullName = file.FullName,
                    Extension = file.Extension,
                    Hash = hash
                });
            }
        }
    }
}