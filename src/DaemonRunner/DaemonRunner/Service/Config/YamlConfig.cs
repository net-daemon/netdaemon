using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using JoySoftware.HomeAssistant.NetDaemon.Common;
using YamlDotNet.RepresentationModel;

[assembly: InternalsVisibleTo("NetDaemon.Daemon.Tests")]
namespace JoySoftware.HomeAssistant.NetDaemon.DaemonRunner.Service.Config
{
    public class YamlAppConfig
    {
        private readonly IEnumerable<Type> _types;
        private readonly YamlStream _yamlStream;

        public YamlAppConfig(IEnumerable<Type> types, TextReader reader)
        {
            _types = types;
            _yamlStream = new YamlStream();
            _yamlStream.Load(reader);
        }

        public INetDaemonApp? Instance
        {
            get
            {

                // For each app instance defined in the yaml config
                foreach (KeyValuePair<YamlNode, YamlNode> app in (YamlMappingNode)_yamlStream.Documents[0].RootNode)
                {
                    if (app.Key.NodeType != YamlNodeType.Scalar ||
                        app.Value.NodeType != YamlNodeType.Mapping)
                    {
                        continue;
                    }

                    string? appId = ((YamlScalarNode)app.Key).Value;
                    // Get the class

                    string? appClass = GetTypeNameFromClassConfig((YamlMappingNode)app.Value);
                    Type appType = _types.Where(n => n.Name.ToLowerInvariant() == appClass)
                                                   .FirstOrDefault();

                    if (appType != null)
                    {
                        return InstanceAndSetPropertyConfig(appType, ((YamlMappingNode)app.Value));
                    }
                }

                return null;
            }
        }
        public INetDaemonApp? InstanceAndSetPropertyConfig(Type netDaemonAppType, YamlMappingNode appNode)
        {
            var netDaemonApp = (INetDaemonApp?)Activator.CreateInstance(netDaemonAppType);

            if (netDaemonApp == null)
                return null;

            foreach (KeyValuePair<YamlNode, YamlNode> entry in appNode.Children)
            {
                string? scalarPropertyName = ((YamlScalarNode)entry.Key).Value;
                // Just continue to next configuration if null or class declaration
                if (scalarPropertyName == null) continue;
                if (scalarPropertyName == "class") continue;

                var prop = netDaemonAppType.GetYamlProperty(scalarPropertyName) ??
                    throw new MissingMemberException($"{scalarPropertyName} is missing from the type {netDaemonAppType}");

                var valueType = entry.Value.NodeType;


                switch (valueType)
                {
                    case YamlNodeType.Sequence:
                        netDaemonApp.SetPropertyFromYaml(prop, (YamlSequenceNode)entry.Value);
                        break;

                    case YamlNodeType.Scalar:
                        netDaemonApp.SetPropertyFromYaml(prop, (YamlScalarNode)entry.Value);
                        break;

                    case YamlNodeType.Mapping:
                        var map = (YamlMappingNode)entry.Value;
                        break;
                }

            }

            return netDaemonApp;
        }

        private string? GetTypeNameFromClassConfig(YamlMappingNode appNode)
        {
            KeyValuePair<YamlNode, YamlNode> classChild = appNode.Children.Where(n =>
                                   ((YamlScalarNode)n.Key)?.Value?.ToLowerInvariant() == "class").FirstOrDefault();

            if (classChild.Key == null || classChild.Value == null)
            {
                return null;
            }

            if (classChild.Value.NodeType != YamlNodeType.Scalar)
            {
                return null;
            }
            return ((YamlScalarNode)classChild.Value)?.Value?.ToLowerInvariant();
        }
    }
    public class YamlConfig
    {
        private readonly string _configFixturePath;
        private readonly Dictionary<string, Dictionary<string, string>> _secrets;

        public YamlConfig(string configFixturePath)
        {
            _configFixturePath = configFixturePath;
            _secrets = GetAllSecretsFromPath(_configFixturePath);

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
            if (configPath != _configFixturePath)
            {
                return GetSecretFromPath(secret, Directory.GetParent(configPath).FullName);
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

        // internal static IList<INetDaemonApp> InstanceAppFromConfig(IDictionary<Type> )


    }

}