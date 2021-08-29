using System.Collections.Generic;
using YamlDotNet.RepresentationModel;

namespace NetDaemon.Daemon.Config
{
    public interface IYamlConfig
    {
        IEnumerable<YamlConfigEntry> GetAllConfigs();
    }
}