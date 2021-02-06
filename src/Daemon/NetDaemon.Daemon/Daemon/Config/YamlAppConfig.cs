using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using NetDaemon.Common;
using NetDaemon.Common.Exceptions;
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

        [SuppressMessage("", "CA1508")]
        [SuppressMessage("", "CA1065")]
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
                        Type? appType = _types.FirstOrDefault(n => n.FullName?.ToLowerInvariant() == appClass);

                        if (appType != null)
                        {
                            var instance = InstanceAndSetPropertyConfig(appType, (YamlMappingNode)app.Value, appId);
                            if (instance != null)
                            {
                                instance.Id = appId;
                                instances.Add(instance);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        throw new NetDaemonException($"Error instancing application {appId}", e);
                    }
                }

                return instances;
            }
        }

        public INetDaemonAppBase? InstanceAndSetPropertyConfig(
            Type netDaemonAppType,
            YamlMappingNode appNode,
            string? appId)
        {
            _ = appNode ??
                throw new NetDaemonArgumentNullException(nameof(appNode));

            var netDaemonApp = (INetDaemonAppBase?)Activator.CreateInstance(netDaemonAppType);

            foreach (KeyValuePair<YamlNode, YamlNode> entry in appNode.Children)
            {
                string? scalarPropertyName = ((YamlScalarNode)entry.Key).Value;
                try
                {
                    // Just continue to next configuration if null or class declaration
                    if (scalarPropertyName == null) continue;
                    if (scalarPropertyName == "class") continue;

                    var prop = netDaemonAppType.GetYamlProperty(scalarPropertyName) ??
                            throw new MissingMemberException($"{scalarPropertyName} is missing from the type {netDaemonAppType}");

                    var valueType = entry.Value.NodeType;

                    var instance = InstanceProperty(netDaemonApp, prop.PropertyType, entry.Value);

                    prop.SetValue(netDaemonApp, instance);
                }
                catch (Exception e)
                {
                    throw new NetDaemonException($"Failed to set value {scalarPropertyName} for app {appId}", e);
                }
            }

            return netDaemonApp;
        }

        [SuppressMessage("", "CA1508")] // Weird bug that this should not warn!
        private object? InstanceProperty(object? parent, Type instanceType, YamlNode node)
        {
            if (node.NodeType == YamlNodeType.Scalar)
            {
                var scalarNode = (YamlScalarNode)node;
                ReplaceSecretIfExists(scalarNode);
                return ((YamlScalarNode)node).ToObject(instanceType,parent);
            }
            else if (node.NodeType == YamlNodeType.Sequence)
            {
                if (instanceType.IsGenericType && instanceType?.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    Type listType = instanceType?.GetGenericArguments()[0] ??
                                    throw new NetDaemonNullReferenceException($"The property {instanceType?.Name} of Class {parent?.GetType().Name} is not compatible with configuration");

                    IList list = listType.CreateListOfPropertyType() ??
                                throw new NetDaemonNullReferenceException("Failed to create listtype, please check {prop.Name} of Class {app.GetType().Name}");

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
                    object? result = null;

                    switch (valueType)
                    {
                        case YamlNodeType.Sequence:
                            result = InstanceProperty(instance, childProp.PropertyType, (YamlSequenceNode)entry.Value);

                            break;

                        case YamlNodeType.Scalar:
                            result = InstanceProperty(instance, childProp.PropertyType, (YamlScalarNode)entry.Value);
                            break;

                        case YamlNodeType.Mapping:
                            // Maps are not currently supported (var map = (YamlMappingNode)entry.Value;)
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

            scalarNode.Value = secretReplacement ?? throw new NetDaemonException($"{scalarNode.Value!} not found in secrets.yaml");
        }

        private static string? GetTypeNameFromClassConfig(YamlMappingNode appNode)
        {
            KeyValuePair<YamlNode, YamlNode> classChild = appNode.Children.FirstOrDefault(n =>
                ((YamlScalarNode)n.Key)?.Value?.ToLowerInvariant() == "class");

            if (classChild.Key == null || classChild.Value == null)
            {
                return null;
            }

            if (classChild.Value.NodeType != YamlNodeType.Scalar)
            {
                return null;
            }
            var scalarNode = (YamlScalarNode)classChild.Value;
            return scalarNode.Value?.ToLowerInvariant();
        }
    }
}