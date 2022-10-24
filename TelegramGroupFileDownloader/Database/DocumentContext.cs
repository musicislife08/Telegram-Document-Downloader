using Microsoft.EntityFrameworkCore;

namespace TelegramGroupFileDownloader.Database;

public class DocumentContext : DbContext
{
    public DbSet<DocumentFile> DocumentFiles { get; set; } = null!;
    public DbSet<DuplicateFile> DuplicateFiles { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=DocumentFiles.db;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DocumentFile>(entity =>
        {
            entity.HasIndex(e => e.Hash).IsUnique();
            entity.HasIndex(e => e.Name);
        });
        modelBuilder.Entity<DuplicateFile>(entity =>
        {
            entity.HasIndex(e => e.Id).IsUnique();
            entity.HasIndex(e => e.Hash);
        });
    }
}