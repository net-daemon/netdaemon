using System.IO;
using YamlDotNet.RepresentationModel;

namespace NetDaemon.Daemon.Config
{
    public interface IYamlConfigReader
    {
        YamlStream GetYamlStream(string path);
    }
}