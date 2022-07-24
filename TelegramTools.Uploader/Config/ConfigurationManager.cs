using Spectre.Console;
using TelegramTools.Uploader.Telegram;
using YamlDotNet.Serialization;

namespace TelegramTools.Uploader.Config;

public static class ConfigurationManager
{
    public static TgConfig GetConfiguration()
    {
        var config = new TgConfig();
        if (!File.Exists("config.yaml"))
        {
            var configRoot = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();
            configRoot.GetSection("Config").Bind(config);
            SaveConfiguration(config);
            return config;
        }
        var converter = new DeserializerBuilder().Build();
        var rawYaml = File.ReadAllText("config.yaml");
        config = converter.Deserialize<TgConfig>(rawYaml);
        return config;
    }

    public static void SaveConfiguration(TgConfig config)
    {
        var converter = new SerializerBuilder().Build();
        var yaml = converter.Serialize(config);
        File.WriteAllText("config.yaml", yaml);
    }

    public static string? WTelegramConfig(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(nameof(key));
        var config = GetConfiguration();
        if (config is null)
            throw new NullReferenceException("Config cannot be null to login to telegram");
        return key switch
        {
            "api_id" => config.ApiId,
            "api_hash" => config.ApiHash,
            "phone_number" => config.PhoneNumber,
            "Verification_code" => AnsiConsole.Prompt(new TextPrompt<string>("[bold red]Enter Verification Code:[/]")
                .PromptStyle("red").Secret()),
            "first_name" => throw new TelegramAccountException(),
            "last_name" => throw new TelegramAccountException(),
            "password" => AnsiConsole.Prompt(new TextPrompt<string>("[bold red]Enter 2fa password:[/] ").PromptStyle("red")
                .Secret()),
            "session_pathname" => Path.Combine(config.SessionPath, "tg.session"),
            _ => null
        };
    }
}