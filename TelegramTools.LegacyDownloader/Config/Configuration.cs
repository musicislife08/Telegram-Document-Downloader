// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
namespace TelegramTools.LegacyDownloader.Config;

public class Configuration
{
    public string? PhoneNumber { get; set; } = string.Empty;
    public string SessionPath { get; set; } = Environment.CurrentDirectory;
    public string DownloadPath { get; set; } = Environment.CurrentDirectory;
    public string? DocumentExtensionFilter { get; set; } = string.Empty;
    public string? GroupName { get; set; } = string.Empty;
}