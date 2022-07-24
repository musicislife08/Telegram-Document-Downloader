namespace TelegramTools.Database.Entities;

public class Duplicate
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string OriginalName { get; set; }
    public byte[] Hash { get; set; }
    public long TelegramId { get; set; }
}