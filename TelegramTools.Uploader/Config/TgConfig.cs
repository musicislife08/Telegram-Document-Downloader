using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace TelegramTools.Uploader.Config;

public class TgConfig
{
    [JsonIgnore]
    [YamlIgnore]
    public string ApiId { get; } = "2252206";
    [JsonIgnore]
    [YamlIgnore]
    public string ApiHash { get; } = "4dcf9af0c05042ca938a0a44bfb522dd";
    [JsonIgnore]
    [YamlIgnore]
    public string SessionPath { get; } = Environment.CurrentDirectory;
    public string TempPath { get; set; } = Path.Combine(Path.GetTempPath(), "telegram_downloader");
    public string? UploadPath { get; set; }
    public string? PhoneNumber { get; set; } = string.Empty;
    public string? GroupName { get; set; }
    public bool UnzipBeforeUpload { get; set; } = true;
    public string? DocumentExtensionFilter { get; set; } =
        ".pdf,.doc,.docx,.xls,.csv,.xlsx,.txt,.epub,.mobi,.azw3,.azw,.zip,.rar,.7z,.tar.gz,.pptx,.djvu";
};