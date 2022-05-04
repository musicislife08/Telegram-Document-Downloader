using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using TelegramGroupFileDownloader.Database;

namespace TelegramGroupFileDownloader.Documents;

static class DocumentManager
{
       public static async Task<PreDownloadProcessingDecision> DecidePreDownload(FileInfo file, long telegramId, long size,
              CancellationToken stoppingToken = new())
       {
              await using var db = new DocumentContext();
              var dbExists = await db.DocumentFiles.AnyAsync(x => x.TelegramId == telegramId, stoppingToken);
              if (!dbExists)
              {
                     var dbFile = await db.DocumentFiles.FirstOrDefaultAsync(
                            // ReSharper disable once SpecifyStringComparison
                            x => x.Name.ToLower() == file.Name.ToLower(), stoppingToken);
                     if (dbFile is not null)
                     {
                            dbExists = true;
                            dbFile.TelegramId = telegramId;
                            db.DocumentFiles.Update(dbFile);
                            await db.SaveChangesAsync(stoppingToken);
                     }
              }
              if (dbExists)
                     return PreDownloadProcessingDecision.Nothing;
              var fileExists = file.Exists;
              long fileSize = 0;
              if (fileExists)
                     fileSize = file.Length;
              switch (fileExists)
              {
                     case true when !dbExists && fileSize == size:
                            return PreDownloadProcessingDecision.Update;
                     case true when !dbExists:
                            return PreDownloadProcessingDecision.ReDownload;
              }

              var dupeExists = await db.DuplicateFiles.AnyAsync(x =>
                     x.TelegramId == telegramId, stoppingToken);
              if (!dupeExists)
              {
                     var dbFile =
                            await db.DuplicateFiles.FirstOrDefaultAsync(x => x.DuplicateName == file.Name,
                                   stoppingToken);
                     if (dbFile is not null)
                     {
                            dupeExists = true;
                            dbFile.TelegramId = telegramId;
                            db.DuplicateFiles.Update(dbFile);
                            await db.SaveChangesAsync(stoppingToken);
                     }
              }
              if (dupeExists)
                     return PreDownloadProcessingDecision.ExistingDuplicate;
              return !fileExists && !dbExists
                     ? PreDownloadProcessingDecision.SaveAndDownload
                     : PreDownloadProcessingDecision.Error;
       }
       public static async Task<PostDownloadProcessingDecision> DecidePostDownload(FileInfo file, string hash,
              CancellationToken stoppingToken = new())
       {
              await using var db = new DocumentContext();
              var isDuplicateByHash = await db.DocumentFiles.AnyAsync(x => x.Hash == hash, stoppingToken);
              return isDuplicateByHash ? PostDownloadProcessingDecision.ProcessDuplicate : PostDownloadProcessingDecision.Nothing;
       }

       private static string GetFileHash(string filename)
       {
              using var sha256 = SHA256.Create();
              using var stream = File.OpenRead(filename);
              var hash = sha256.ComputeHash(stream);
              return BitConverter.ToString(hash).Replace("-", "");
       }
}