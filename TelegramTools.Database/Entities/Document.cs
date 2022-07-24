namespace TelegramTools.Database.Entities;

public class Document
{
    public long TelegramId { get; set; }
    public string Name { get; set; }
    public string Extension { get; set; }
    public string Path { get; set; }
    public byte[] Hash { get; set; }
}