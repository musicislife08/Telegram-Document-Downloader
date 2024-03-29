namespace TelegramGroupFileDownloader;

public class ConfigValueException: Exception
{
    public ConfigValueException(string configValueName)
    : base($"Configuration Value {configValueName} cannot be missing") { }
}