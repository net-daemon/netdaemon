using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NetDaemon.Common;
using YamlDotNet.RepresentationModel;

namespace NetDaemon.Daemon.Config
{
    public class YamlAppConfig
    {
        private readonly IEnumerable<Type> _types;
        private readonly YamlStream _yamlStream;
        private readonly IYamlConfig _yamlConfig;
        private readonly string _yamlFilePath;

        public YamlAppConfig(IEnumerable<Type> types, TextReader reader, IYamlConfig yamlConfig, string yamlFilePath)
        {
            _types = types;
            _yamlStream = new YamlStream();
            _yamlStream.Load(reader);

            _yamlConfig = yamlConfig;
            _yamlFilePath = yamlFilePath;
        }

        public IEnumerable<INetDaemonAppBase> Instances
        {
            get
            {
                var instances = new List<INetDaemonAppBase>();
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
                                instances.Add(instance);
                            }
                        }
                    }
                    catch (System.Exception e)
                    {
                        throw new ApplicationException($"Error instancing application {appId}", e);
                    }
                }

                return instances;
            }
        }

        public INetDaemonAppBase? InstanceAndSetPropertyConfig(Type netDaemonAppType, YamlMappingNode appNode, string? appId)
        {
            var netDaemonApp = (INetDaemonAppBase?)Activator.CreateInstance(netDaemonAppType);

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

                var instance = InstanceProperty(netDaemonApp, prop.PropertyType, entry.Value);

                prop.SetValue(netDaemonApp, instance);
            }

            return netDaemonApp;
        }

        private object? InstanceProperty(Object? parent, Type instanceType, YamlNode node)
        {

            if (node.NodeType == YamlNodeType.Scalar)
            {
                var scalarNode = (YamlScalarNode)node;
                ReplaceSecretIfExists(scalarNode);
                return ((YamlScalarNode)node).ToObject(instanceType);
            }
            else if (node.NodeType == YamlNodeType.Sequence)
            {
                if (instanceType.IsGenericType && instanceType?.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    Type listType = instanceType?.GetGenericArguments()[0] ??
                                    throw new NullReferenceException($"The property {instanceType?.Name} of Class {parent?.GetType().Name} is not compatible with configuration");

                    IList list = listType.CreateListOfPropertyType() ??
                                 throw new NullReferenceException("Failed to create listtype, plese check {prop.Name} of Class {app.GetType().Name}");

                    foreach (YamlNode item in ((YamlSequenceNode)node).Children)
                    {

                        var instance = InstanceProperty(null, listType, item) ??
                                       throw new NotSupportedException($"The class {parent?.GetType().Name} has wrong type in items");

                        list.Add(instance);
                    }

                    return list;
                }
            }
            else if (node.NodeType == YamlNodeType.Mapping)
            {
                var instance = Activator.CreateInstance(instanceType);

                foreach (KeyValuePair<YamlNode, YamlNode> entry in ((YamlMappingNode)node).Children)
                {
                    string? scalarPropertyName = ((YamlScalarNode)entry.Key).Value;
                    // Just continue to next configuration if null or class declaration
                    if (scalarPropertyName == null) continue;

                    var childProp = instanceType.GetYamlProperty(scalarPropertyName) ??
                                    throw new MissingMemberException($"{scalarPropertyName} is missing from the type {instanceType}");

                    var valueType = entry.Value.NodeType;
                    Object? result = null;

                    switch (valueType)
                    {
                        case YamlNodeType.Sequence:
                            result = InstanceProperty(instance, childProp.PropertyType, (YamlSequenceNode)entry.Value);

                            break;

                        case YamlNodeType.Scalar:
                            result = InstanceProperty(instance, childProp.PropertyType, (YamlScalarNode)entry.Value);
                            break;

                        case YamlNodeType.Mapping:
                            var map = (YamlMappingNode)entry.Value;
                            break;
                    }
                    childProp.SetValue(instance, result);
                }
                return instance;
            }
            return null;
        }

        private void ReplaceSecretIfExists(YamlScalarNode scalarNode)
        {
            if (scalarNode.Tag != "!secret" && scalarNode.Value != null)
                return;

            var secretReplacement = _yamlConfig.GetSecretFromPath(scalarNode.Value!, Path.GetDirectoryName(_yamlFilePath)!);

            scalarNode.Value = secretReplacement ?? throw new ApplicationException($"{scalarNode.Value!} not found in secrets.yaml");
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
}