using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace TelegramTools.Database.Entities;

public class DuplicateFile
{
    [Key]
    public Guid Id { get; } = Guid.NewGuid();
    [Unicode(true)]
    public string OrignalName { get; set; }
    [Unicode(true)]
    public string DuplicateName { get; set; }
    public string Hash { get; set; }
    public long TelegramId { get; set; }
}