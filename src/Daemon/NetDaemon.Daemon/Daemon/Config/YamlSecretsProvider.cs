using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using NetDaemon.Common.Exceptions;
using YamlDotNet.RepresentationModel;

namespace NetDaemon.Daemon.Config
{
    public class YamlSecretsProvider : IYamlSecretsProvider
    {
        private readonly string _configFolder;
        private readonly IYamlConfigReader _yamlConfigReader;

        private readonly Dictionary<string, Dictionary<string, string>> _secrets;

        public YamlSecretsProvider(string configFolder, IYamlConfigReader yamlConfigReader)
        {
            _configFolder = configFolder;
            _yamlConfigReader = yamlConfigReader;

            _secrets = GetAllSecrets();
        }

        public string? GetSecretFromPath(string secret, string configPath)
        {
            while (true)
            {
                if (_secrets.ContainsKey(configPath) && _secrets[configPath].ContainsKey(secret))
                {
                    return _secrets[configPath][secret];
                }

                if (configPath == _configFolder) return null;

                configPath = Directory.GetParent(configPath)?.FullName ?? throw new NetDaemonException("Parent folder of config path does not exist");
            }
        }

        internal Dictionary<string, Dictionary<string, string>> GetAllSecrets()
        {
            var result = new Dictionary<string, Dictionary<string, string>>();
            foreach (string file in Directory.EnumerateFiles(_configFolder, "secrets.yaml", SearchOption.AllDirectories))
            {
                var fileDirectory = Path.GetDirectoryName(file);

                if (fileDirectory == null)
                    continue;

                if (!result.ContainsKey(fileDirectory))
                {
                    result[fileDirectory] = GetSecretsFromFile(file) ??
                                            new Dictionary<string, string>();
                }
            }

            return result;
        }

        internal Dictionary<string, string> GetSecretsFromFile(string file)
        {
            var yamlStream = _yamlConfigReader.GetYamlStream(file);

            return GetSecretsFromYaml(yamlStream);
        }

        [SuppressMessage("", "CA1508")] // TODO: Need to refactor this
        internal static Dictionary<string, string> GetSecretsFromYaml(YamlStream yaml)
        {
            var result = new Dictionary<string, string>();

            foreach (KeyValuePair<YamlNode, YamlNode> entry in (YamlMappingNode) yaml.Documents[0].RootNode)
            {
                if (entry.Key.NodeType != YamlNodeType.Scalar ||
                    entry.Value.NodeType != YamlNodeType.Scalar)
                {
                    continue;
                }

                string? secret = ((YamlScalarNode) entry.Key).Value;
                // Get the class
                string? value = ((YamlScalarNode) entry.Value).Value;

                if (secret != null && value != null)
                    result[secret] = value;
            }

            return result;
        }
    }
}