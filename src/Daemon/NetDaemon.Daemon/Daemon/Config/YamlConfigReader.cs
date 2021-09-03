using System.IO;
using YamlDotNet.RepresentationModel;

namespace NetDaemon.Daemon.Config
{
    public class YamlConfigReader : IYamlConfigReader
    {
        private readonly IIoWrapper _io;

        public YamlConfigReader(IIoWrapper io)
        {
            _io = io;
        }

        public YamlStream GetYamlStream(string path)
        {
            using TextReader textReader = new StringReader(_io.ReadFile(path));

            var yamlStream = new YamlStream();
            yamlStream.Load(textReader);

            return yamlStream;
        }
    }
}