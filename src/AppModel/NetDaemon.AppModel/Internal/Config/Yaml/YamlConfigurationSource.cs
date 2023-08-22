using Microsoft.Extensions.Configuration;

namespace NetDaemon.AppModel.Internal.Config;

internal class YamlConfigurationSource : FileConfigurationSource
{
    public override IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        FileProvider ??= builder.GetFileProvider();
        return new YamlConfigurationProvider(this);
    }
}