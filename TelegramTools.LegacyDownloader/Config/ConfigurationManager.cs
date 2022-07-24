using Microsoft.Extensions.Configuration;
using YamlDotNet.Serialization;

namespace TelegramTools.LegacyDownloader.Config;

public static class ConfigurationManager
{
    public static Configuration GetConfiguration()
    {
        var config = new Configuration();
        if (!File.Exists("config.yaml"))
        {
            var configRoot = new ConfigurationBuilder().AddEnvironmentVariables().Build();
            configRoot.GetSection("Config").Bind(config);
            SaveConfiguration(config);
            return config;
        }
        var converter = new DeserializerBuilder().Build();
        var rawYaml = File.ReadAllText("config.yaml");
        config = converter.Deserialize<Configuration>(rawYaml);
        return config;
    }

    public static void SaveConfiguration(Configuration config)
    {
        var converter = new SerializerBuilder().Build();
        var yaml = converter.Serialize(config);
        File.WriteAllText("config.yaml", yaml);
    }
}