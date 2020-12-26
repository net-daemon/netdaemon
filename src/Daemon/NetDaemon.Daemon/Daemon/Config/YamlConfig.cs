using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;
using NetDaemon.Common.Configuration;
using NetDaemon.Common.Exceptions;
using YamlDotNet.RepresentationModel;

[assembly: InternalsVisibleTo("NetDaemon.Daemon.Tests")]

namespace NetDaemon.Daemon.Config
{
    public interface IYamlConfig
    {
        string? GetSecretFromPath(string secret, string configPath);
        IEnumerable<string> GetAllConfigFilePaths();
    }

    public class YamlConfig : IYamlConfig
    {
        private readonly string _configFolder;
        private readonly Dictionary<string, Dictionary<string, string>> _secrets;

        public YamlConfig(IOptions<NetDaemonSettings> netDaemonSettings)
        {
            _configFolder = netDaemonSettings.Value.GetAppSourceDirectory();
            _secrets = GetAllSecretsFromPath(_configFolder);
        }

        public IEnumerable<string> GetAllConfigFilePaths()
        {
            return Directory.EnumerateFiles(_configFolder, "*.yaml", SearchOption.AllDirectories);
        }

        public string? GetSecretFromPath(string secret, string configPath)
        {
            if (_secrets.ContainsKey(configPath))
            {
                if (_secrets[configPath].ContainsKey(secret))
                {
                    return _secrets[configPath][secret];
                }
            }
            if (configPath != _configFolder)
            {
                var parentPath = Directory.GetParent(configPath)?.FullName ?? throw new NetDaemonException("Parent folder of config path does not exist");

                return GetSecretFromPath(secret, parentPath);
            }
            return null;
        }

        internal static Dictionary<string, string> GetSecretsFromSecretsYaml(string file)
        {
            return GetSecretsFromSecretsYaml(File.OpenText(file));
        }

        internal static Dictionary<string, string> GetSecretsFromSecretsYaml(TextReader reader)
        {
            var result = new Dictionary<string, string>();

            var yaml = new YamlStream();
            yaml.Load(reader);

            foreach (KeyValuePair<YamlNode, YamlNode> entry in (YamlMappingNode)yaml.Documents[0].RootNode)
            {
                if (entry.Key.NodeType != YamlNodeType.Scalar ||
                    entry.Value.NodeType != YamlNodeType.Scalar)
                {
                    continue;
                }

                string? secret = ((YamlScalarNode)entry.Key).Value;
                // Get the class
                string? value = ((YamlScalarNode)entry.Value).Value;

                if (secret != null && value != null)
                    result[secret] = value;
            }

            return result;
        }

        internal static Dictionary<string, Dictionary<string, string>> GetAllSecretsFromPath(string codeFolder)
        {
            var result = new Dictionary<string, Dictionary<string, string>>();
            foreach (string file in Directory.EnumerateFiles(codeFolder, "secrets.yaml", SearchOption.AllDirectories))
            {
                var fileDirectory = Path.GetDirectoryName(file);

                if (fileDirectory == null)
                    continue;

                if (!result.ContainsKey(fileDirectory))
                {
                    var secretsFromFile = GetSecretsFromSecretsYaml(file);
                    result[fileDirectory] = secretsFromFile;
                }
            }
            return result;
        }
    }
}