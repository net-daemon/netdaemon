using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;
using NetDaemon.Common.Configuration;
using NetDaemon.Common.Exceptions;
using YamlDotNet.RepresentationModel;

[assembly: InternalsVisibleTo("NetDaemon.Daemon.Tests")]

namespace NetDaemon.Daemon.Config
{
    public class YamlConfigProvider : IYamlConfig
    {
        private readonly IYamlConfigReader _yamlConfigReader;
        private readonly string _configFolder;
        private readonly IYamlSecretsProvider _yamlSecretsProvider;

        public YamlConfigProvider(IOptions<NetDaemonSettings> netDaemonSettings, IYamlConfigReader yamlConfigReader)
        {
            _yamlConfigReader = yamlConfigReader;
            _ = netDaemonSettings ??
                throw new NetDaemonArgumentNullException(nameof(netDaemonSettings));

            _configFolder = netDaemonSettings.Value.GetAppSourceDirectory();
            _yamlSecretsProvider = new YamlSecretsProvider(_configFolder, _yamlConfigReader);
        }

        public IEnumerable<YamlConfigEntry> GetAllConfigs()
        {
            var yamlConfigs = Directory.EnumerateFiles(_configFolder, "*.yaml", SearchOption.AllDirectories);

            return yamlConfigs.Select(path => new YamlConfigEntry(path, _yamlConfigReader, _yamlSecretsProvider));
        }
    }
}