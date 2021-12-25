using Microsoft.Extensions.Configuration;

namespace NetDaemon.AppModel.Internal.Config;

internal class YamlConfigurationProvider : FileConfigurationProvider
{
    public YamlConfigurationProvider(YamlConfigurationSource source) : base(source) { }

    public override void Load(Stream stream)
    {
        var parser = new YamlConfigurationFileParser();

        Data = parser.Parse(stream);
    }
}
