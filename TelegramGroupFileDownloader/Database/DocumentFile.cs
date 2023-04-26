using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace TelegramGroupFileDownloader.Database;

public class DocumentFile
{
    [Unicode] public string? Name { get; set; }

    public string? Extension { get; set; }

    [Required] [Unicode] public string? FullName { get; set; }

    [Key] public string? Hash { get; set; }

    public long? TelegramId { get; set; }
}