using Microsoft.Extensions.Configuration;

namespace TelegramTools.Configuration;

public class YamlConfigurationProvider : FileConfigurationProvider
{
    // ReSharper disable once SuggestBaseTypeForParameterInConstructor
    public YamlConfigurationProvider(YamlConfigurationSource source) : base(source) { }

    public override void Load(Stream stream)
    {
        var parser = new YamlConfigurationFileParser();
       
        Data = parser.Parse(stream);
    }
}