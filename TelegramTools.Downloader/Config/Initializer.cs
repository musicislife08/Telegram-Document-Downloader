using Spectre.Console;

namespace TelegramTools.Downloader.Config;

public static class Initializer
{
    private static bool _firstRun = false;
    public static void Initialize()
    {
        var config = ConfigurationManager.GetConfiguration();
        if (string.IsNullOrWhiteSpace(config.Config.PhoneNumber))
        {
            config.Config.PhoneNumber = AnsiConsole.Prompt(new TextPrompt<string?>("Enter Phone Number in [yellow]+1xxxxxxx format:[/]"));
            ConfigurationManager.SaveConfiguration(config);
            _firstRun = true;
        }

        if (string.IsNullOrWhiteSpace(config.Config.GroupName))
        {
            config.Config.GroupName = AnsiConsole.Prompt(
                new TextPrompt<string?>("Enter Telegram group name:"));
            ConfigurationManager.SaveConfiguration(config);
            _firstRun = true;
        }

        if (string.IsNullOrWhiteSpace(config.Config.DownloadPath))
        {
            config.Config.DownloadPath = AnsiConsole.Prompt(
                                             new TextPrompt<string?>("Enter path to the folder to download files to:"))
                                         ?? Environment.CurrentDirectory;
            ConfigurationManager.SaveConfiguration(config);
            _firstRun = true;
        }

        if (string.IsNullOrWhiteSpace(config.Config.GroupName))
            throw new ConfigValueException(nameof(config.Config.GroupName));
        if (string.IsNullOrWhiteSpace(config.Config.PhoneNumber))
            throw new ConfigValueException(nameof(config.Config.PhoneNumber));
        if (string.IsNullOrWhiteSpace(config.Config.DownloadPath))
            throw new ConfigValueException(nameof(config.Config.DownloadPath));
        Directory.CreateDirectory(config.Config.DownloadPath);
        
        if (_firstRun)
        {
            AnsiConsole.MarkupLine("[bold yellow]Initial configuration created.[/]  [bold red]Before running again double check config.yaml file[/]");
            Environment.Exit(0);
        }
        WTelegram.Helpers.Log = (lvl, str) => System.Diagnostics.Debug.WriteLine($"WTelegram {lvl}: {str}");
        using var client = new WTelegram.Client(ConfigurationManager.WTelegramConfig);
        client.LoginUserIfNeeded();
        client.Dispose();
    }
}