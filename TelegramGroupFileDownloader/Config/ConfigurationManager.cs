using Microsoft.Extensions.Configuration;
using Spectre.Console;
using YamlDotNet.Serialization;

namespace TelegramGroupFileDownloader.Config;

public static class ConfigurationManager
{
    private const string configName = "config.yaml";
    public static Configuration GetConfiguration()
    {
        var config = new Configuration();
        if (!File.Exists(configName))
        {
            var configRoot = new ConfigurationBuilder().AddEnvironmentVariables().Build();
            configRoot.GetSection("Config").Bind(config);
            SaveConfiguration(config);
            return config;
        }
        var converter = new DeserializerBuilder().Build();
        var permissions = Utilities.TestPermissions(configName);
        if (permissions.IsSuccessful)
        {
            var rawYaml = File.ReadAllText(configName);
            config = converter.Deserialize<Configuration>(rawYaml);
            return config;
        }
        if (permissions.Exception is not null and FileNotFoundException)
        {
            return config;
        }
        AnsiConsole.MarkupLine($"[red]Error accessing {configName}[/]");
        Environment.Exit(2);
        return config;
    }

    public static void SaveConfiguration(Configuration config)
    {
        var converter = new SerializerBuilder().Build();
        var yaml = converter.Serialize(config);
        var fi = new FileInfo(configName);
        var hasAccess = Utilities.TestPermissions(fi.FullName);
        if (!hasAccess.IsSuccessful)
        {
            if (hasAccess.Exception is not FileNotFoundException and not null)
            {
                AnsiConsole.MarkupLine($"Application Exception: {hasAccess.Exception.Message}");
                Environment.Exit(2);
            }
        }
        File.WriteAllText(configName, yaml);
    }
}