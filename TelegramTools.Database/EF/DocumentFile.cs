using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace TelegramTools.Database.Entities;

public class DocumentFile
{
    public long? TelegramId { get; set; }
    [Unicode(true)]
    public string? Name { get; set; }
    public string? Extension { get; set; }
    [Required]
    [Unicode(true)]
    public string? FullName { get; set; }
    [Key]
    public string? Hash { get; set; }
}