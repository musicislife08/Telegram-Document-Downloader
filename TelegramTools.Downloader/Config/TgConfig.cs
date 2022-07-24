using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace TelegramTools.Downloader.Config;

public class TgConfig
{
    private const string _apiId = "2252206";
    private const string _apiHash = "4dcf9af0c05042ca938a0a44bfb522dd";
    private const string _defaultDocumentFilterString 
        = ".pdf,.doc,.docx,.xls,.csv,.xlsx,.txt,.epub,.mobi,.azw3,.azw,.zip,.rar,.7z,.tar.gz,.bz2,.pptx,.djvu";
    [JsonIgnore]
    [YamlIgnore]
    public string ApiId { get; } = _apiId;
    [JsonIgnore]
    [YamlIgnore]
    public string ApiHash { get; } = _apiHash;
    [JsonIgnore]
    [YamlIgnore]
    public string? DocumentExtensionFilter { get; set; } = _defaultDocumentFilterString;
    public string? GroupName { get; set; }
    public bool LogToFiles { get; set; } = true;
    public string? PhoneNumber { get; set; } = string.Empty;
    public string SessionPath { get; set; } = Environment.CurrentDirectory;
    public string TempPath { get; set; } = Path.Combine(Path.GetTempPath(), "telegram_downloader");
    public string? DownloadPath { get; set; }
    public string? UploadPath { get; set; }
    public bool UnzipArchives { get; set; } = true;
};