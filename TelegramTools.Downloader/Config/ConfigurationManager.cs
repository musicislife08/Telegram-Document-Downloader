using Microsoft.Extensions.Configuration;
using Spectre.Console;
using TelegramTools.Downloader.Exceptions;
using YamlDotNet.Serialization;

namespace TelegramTools.Downloader.Config;

public static class ConfigurationManager
{
    public static ConfigBase GetConfiguration()
    {
        var config = new ConfigBase();
        if (!File.Exists("config.yaml"))
        {
            var configRoot = new ConfigurationBuilder().AddEnvironmentVariables().Build();
            configRoot.GetSection("Config").Bind(config);
            SaveConfiguration(config);
            return config;
        }
        var converter = new DeserializerBuilder().Build();
        var rawYaml = File.ReadAllText("config.yaml");
        config = converter.Deserialize<ConfigBase>(rawYaml);
        return config;
    }

    public static void SaveConfiguration(ConfigBase config)
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
            "api_id" => config.Config.ApiId,
            "api_hash" => config.Config.ApiHash,
            "phone_number" => config.Config.PhoneNumber,
            "Verification_code" => AnsiConsole.Prompt(new TextPrompt<string>("[bold red]Enter Verification Code:[/]")
                .PromptStyle("red").Secret()),
            "first_name" => throw new TelegramAccountException(),
            "last_name" => throw new TelegramAccountException(),
            "password" => AnsiConsole.Prompt(new TextPrompt<string>("[bold red]Enter 2fa password:[/] ").PromptStyle("red")
                .Secret()),
            "session_pathname" => Path.Combine(config.Config.SessionPath, "tg.session"),
            _ => null
        };
    }
}