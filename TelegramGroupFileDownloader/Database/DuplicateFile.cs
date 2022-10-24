using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace TelegramGroupFileDownloader.Database;

public class DuplicateFile
{
    [Key]
    public Guid Id { get; } = Guid.NewGuid();

    [Unicode()]
    public string? OrignalName { get; set; }

    [Unicode()]
    public string? DuplicateName { get; set; }

    public string? Hash { get; set; }
    public long TelegramId { get; set; }
}