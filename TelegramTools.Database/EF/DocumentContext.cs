using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TelegramTools.Database.Entities;

namespace TelegramTools.Database;

public class DocumentContext : DbContext
{
    public DbSet<DocumentFile> DocumentFiles { get; set; }
    public DbSet<DuplicateFile> DuplicateFiles { get; set; }

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

// ReSharper disable once UnusedMember.Global
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DocumentContext> 
{ 
    public DocumentContext CreateDbContext(string[] args) 
    { 
        // IConfigurationRoot configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile(@Directory.GetCurrentDirectory() + "/../MyCookingMaster.API/appsettings.json").Build(); 
        var builder = new DbContextOptionsBuilder<DocumentContext>();
        builder.UseSqlite(); 
        return new DocumentContext(); 
    } 
}