namespace TelegramGroupFileDownloader;

public class PathTestResult
{
    public bool IsSuccessful { get; set; }
    public string? Reason { get; set; }
    public Exception? Exception { get; set; }
}