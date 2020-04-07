using JoySoftware.HomeAssistant.NetDaemon.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using YamlDotNet.RepresentationModel;

[assembly: InternalsVisibleTo("NetDaemon.Daemon.Tests")]

namespace JoySoftware.HomeAssistant.NetDaemon.DaemonRunner.Service.Config
{
    public class YamlAppConfig
    {
        private readonly IEnumerable<Type> _types;
        private readonly YamlStream _yamlStream;

        private readonly List<INetDaemonApp> _instances;
        private readonly YamlConfig _yamlConfig;
        private readonly string _yamlFilePath;

        public YamlAppConfig(IEnumerable<Type> types, TextReader reader, YamlConfig yamlConfig, string yamlFilePath)
        {
            _types = types;
            _yamlStream = new YamlStream();
            _yamlStream.Load(reader);
            _instances = new List<INetDaemonApp>();
            _yamlConfig = yamlConfig;
            _yamlFilePath = yamlFilePath;
        }

        public IEnumerable<INetDaemonApp> Instances
        {
            get
            {
                // For each app instance defined in the yaml config
                foreach (KeyValuePair<YamlNode, YamlNode> app in (YamlMappingNode)_yamlStream.Documents[0].RootNode)
                {
                    string? appId = null;
                    try
                    {
                        if (app.Key.NodeType != YamlNodeType.Scalar ||
                            app.Value.NodeType != YamlNodeType.Mapping)
                        {
                            continue;
                        }

                        appId = ((YamlScalarNode)app.Key).Value;
                        // Get the class

                        string? appClass = GetTypeNameFromClassConfig((YamlMappingNode)app.Value);
                        Type appType = _types.Where(n => n.FullName?.ToLowerInvariant() == appClass)
                                                       .FirstOrDefault();

                        if (appType != null)
                        {
                            var instance = InstanceAndSetPropertyConfig(appType, ((YamlMappingNode)app.Value), appId);
                            if (instance != null)
                            {
                                instance.Id = appId;
                                _instances.Add(instance);
                            }
                        }
                    }
                    catch (System.Exception e)
                    {
                        throw new ApplicationException($"Error instancing application {appId}", e);
                    }
                }

                return _instances;
            }
        }


        public INetDaemonApp? InstanceAndSetPropertyConfig(Type netDaemonAppType, YamlMappingNode appNode, string? appId)
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
                        SetPropertyFromYaml(netDaemonApp, prop, (YamlSequenceNode)entry.Value);
                        break;

                    case YamlNodeType.Scalar:
                        SetPropertyFromYaml(netDaemonApp, prop, (YamlScalarNode)entry.Value);
                        break;

                    case YamlNodeType.Mapping:
                        var map = (YamlMappingNode)entry.Value;
                        break;
                }
            }

            return netDaemonApp;
        }

        public void SetPropertyFromYaml(INetDaemonApp app, PropertyInfo prop, YamlSequenceNode seq)
        {
            if (prop.PropertyType.IsGenericType && prop.PropertyType?.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                Type listType = prop.PropertyType?.GetGenericArguments()[0] ??
                    throw new NullReferenceException($"The property {prop.Name} of Class {app.GetType().Name} is not compatible with configuration");

                IList list = listType.CreateListOfPropertyType() ??
                    throw new NullReferenceException("Failed to create listtype, plese check {prop.Name} of Class {app.GetType().Name}");

                foreach (YamlNode item in seq.Children)
                {
                    if (item.NodeType != YamlNodeType.Scalar)
                    {
                        throw new NotSupportedException($"The property {prop.Name} of Class {app.GetType().Name} is not compatible with configuration");
                    }
                    var scalarNode = (YamlScalarNode)item;
                    ReplaceSecretIfExists(scalarNode);
                    var value = ((YamlScalarNode)item).ToObject(listType) ??
                        throw new NotSupportedException($"The class {app.GetType().Name} and property {prop.Name} has wrong type in items");

                    list.Add(value);
                }
                // Bind the list to the property
                prop.SetValue(app, list);
            }
        }

        private void ReplaceSecretIfExists(YamlScalarNode scalarNode)
        {
            if (scalarNode.Tag != "!secret" && scalarNode.Value != null)
                return;

            var secretReplacement = _yamlConfig.GetSecretFromPath(scalarNode.Value!, Path.GetDirectoryName(_yamlFilePath)!);

            scalarNode.Value = secretReplacement ?? throw new ApplicationException($"{scalarNode.Value!} not found in secrets.yaml");
        }

        public void SetPropertyFromYaml(INetDaemonApp app, PropertyInfo prop, YamlScalarNode sc)
        {
            ReplaceSecretIfExists(sc);
            var scalarValue = sc.ToObject(prop.PropertyType) ??
                throw new NotSupportedException($"The class {app.GetType().Name} and property {prop.Name} unexpected value {sc.Value} is wrong type");

            // Bind the list to the property
            prop.SetValue(app, scalarValue);
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
        private readonly string _configFolder;
        private readonly Dictionary<string, Dictionary<string, string>> _secrets;

        public YamlConfig(string codeFolder)
        {
            _configFolder = codeFolder;
            _secrets = GetAllSecretsFromPath(_configFolder);
        }

        internal IEnumerable<string> GetAllConfigFilePaths()
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
    }
}