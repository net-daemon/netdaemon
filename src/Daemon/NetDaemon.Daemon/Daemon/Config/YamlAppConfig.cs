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
    internal class YamlAppConfig
    {
        private readonly IEnumerable<Type> _types;
        private readonly YamlStream _yamlStream;
        private readonly IYamlConfig _yamlConfig;
        private readonly string _yamlFilePath;
        private readonly IAppInstantiator _appInstantiator;

        public YamlAppConfig(IEnumerable<Type> types, TextReader reader, IYamlConfig yamlConfig, string yamlFilePath,
            IAppInstantiator appInstantiator)
        {
            _types = types;
            _yamlStream = new YamlStream();
            _yamlStream.Load(reader);

            _yamlConfig = yamlConfig;
            _yamlFilePath = yamlFilePath;
            _appInstantiator = appInstantiator;
        }

        [SuppressMessage("", "CA1508")]
        [SuppressMessage("", "CA1065")]
        public IEnumerable<ApplicationContext> GetInstances()
        {
            var instances = new List<ApplicationContext>();
            // For each app instance defined in the yaml config
            foreach (KeyValuePair<YamlNode, YamlNode> app in (YamlMappingNode)_yamlStream.Documents[0].RootNode)
            {
                string appId = null!;
                try
                {
                    if (app.Key.NodeType != YamlNodeType.Scalar ||
                        app.Value.NodeType != YamlNodeType.Mapping)
                    {
                        continue;
                    }

                    appId = ((YamlScalarNode)app.Key).Value!;
         
                    // Get the class
                    string? appClass = GetTypeNameFromClassConfig((YamlMappingNode)app.Value);
                    Type? appType = _types.FirstOrDefault(n => n.FullName?.ToLowerInvariant() == appClass);

                    if (appType != null)
                    {
                        var appContext = InstanceAndSetPropertyConfig(appType, (YamlMappingNode)app.Value, appId);
                        
                        instances.Add(appContext);
                    }
                }
                catch (Exception e)
                {
                    throw new NetDaemonException($"Error instancing application {appId}", e);
                }
            }

            return instances;
        }

        public ApplicationContext InstanceAndSetPropertyConfig(
            Type netDaemonAppType,
            YamlMappingNode appNode,
            string appId)
        {
            _ = appNode ??
                throw new NetDaemonArgumentNullException(nameof(appNode));

            var appContext = _appInstantiator.Instantiate(netDaemonAppType, appId);

            SetPropertyConfig(appNode, appContext);
            
            return appContext;
        }

        public void SetPropertyConfig(YamlMappingNode appNode, ApplicationContext applicationContext)
        {
            var appInstance = applicationContext.ApplicationInstance;

            foreach (KeyValuePair<YamlNode, YamlNode> entry in appNode.Children)
            {
                string? scalarPropertyName = ((YamlScalarNode)entry.Key).Value;
                try
                {
                    // Just continue to next configuration if null or class declaration
                    if (scalarPropertyName == null) continue;
                    if (scalarPropertyName == "class") continue;

                    var prop = appInstance.GetType().GetYamlProperty(scalarPropertyName) ??
                               throw new MissingMemberException(
                                   $"{scalarPropertyName} is missing from the type {appInstance.GetType()}");

                    var instance = InstanceProperty(appInstance, prop.PropertyType, entry.Value, applicationContext);

                    prop.SetValue(appInstance, instance);
                }
                catch (Exception e)
                {
                    throw new NetDaemonException($"Failed to set value {scalarPropertyName} for app {applicationContext.Id}", e);
                }
            }
        }

        [SuppressMessage("", "CA1508")] // Weird bug that this should not warn!
        private object? InstanceProperty(object? parent, Type instanceType, YamlNode node, ApplicationContext applicationContext)
        {
            switch (node.NodeType)
            {
                case YamlNodeType.Scalar:
                {
                    var scalarNode = (YamlScalarNode) node;
                    ReplaceSecretIfExists(scalarNode);
                    return ((YamlScalarNode) node).ToObject(instanceType, applicationContext);
                }
                case YamlNodeType.Sequence when !instanceType.IsGenericType ||
                                                instanceType.GetGenericTypeDefinition() != typeof(IEnumerable<>):
                    return null;
                case YamlNodeType.Sequence:
                {
                    var list = CreateSequenceInstance(parent, instanceType, node, applicationContext);

                    return list;
                }
                case YamlNodeType.Mapping:
                {
                    var instance = CreateMappingInstance(instanceType, node, applicationContext);

                    return instance;
                }
                default:
                    return null;
            }
        }

        private object? CreateMappingInstance(Type instanceType, YamlNode node, ApplicationContext applicationContext)
        {
            var instance = Activator.CreateInstance(instanceType);

            foreach (KeyValuePair<YamlNode, YamlNode> entry in ((YamlMappingNode) node).Children)
            {
                var scalarPropertyName = ((YamlScalarNode) entry.Key).Value;
                // Just continue to next configuration if null or class declaration
                if (scalarPropertyName == null) continue;

                var childProp = instanceType.GetYamlProperty(scalarPropertyName) ??
                                throw new MissingMemberException(
                                    $"{scalarPropertyName} is missing from the type {instanceType}");

                var valueType = entry.Value.NodeType;
                object? result = null;

                switch (valueType)
                {
                    case YamlNodeType.Sequence:
                        result = InstanceProperty(instance, childProp.PropertyType, (YamlSequenceNode) entry.Value, applicationContext);

                        break;

                    case YamlNodeType.Scalar:
                        result = InstanceProperty(instance, childProp.PropertyType,
                            (YamlScalarNode) entry.Value, applicationContext);
                        break;

                    case YamlNodeType.Mapping:
                        result = CreateMappingInstance(childProp.PropertyType, entry.Value, applicationContext);
                        break;
                }

                childProp.SetValue(instance, result);
            }

            return instance;
        }

        [SuppressMessage("", "CA1508")]
        private IList CreateSequenceInstance(object? parent, Type instanceType, YamlNode node, ApplicationContext applicationContext)
        {
            Type listType = instanceType.GetGenericArguments()[0] ??
                            throw new NetDaemonNullReferenceException(
                                $"The property {instanceType.Name} of Class {parent?.GetType().Name} is not compatible with configuration");

            IList list = listType.CreateListOfPropertyType() ??
                         throw new NetDaemonNullReferenceException(
                             "Failed to create list type, please check {prop.Name} of Class {app.GetType().Name}");

            foreach (YamlNode item in ((YamlSequenceNode) node).Children)
            {
                var instance = InstanceProperty(null, listType, item, applicationContext) ??
                               throw new NotSupportedException(
                                   $"The class {parent?.GetType().Name} has wrong type in items");

                list.Add(instance);
            }

            return list;
        }

        private void ReplaceSecretIfExists(YamlScalarNode scalarNode)
        {
            if (scalarNode.Tag != "!secret" && scalarNode.Value != null)
                return;

            var secretReplacement =
                _yamlConfig.GetSecretFromPath(scalarNode.Value!, Path.GetDirectoryName(_yamlFilePath)!);

            scalarNode.Value = secretReplacement ??
                               throw new NetDaemonException($"{scalarNode.Value!} not found in secrets.yaml");
        }

        private static string? GetTypeNameFromClassConfig(YamlMappingNode appNode)
        {
            KeyValuePair<YamlNode, YamlNode> classChild = appNode.Children.FirstOrDefault(n =>
                ((YamlScalarNode) n.Key).Value?.ToLowerInvariant() == "class");

            if (classChild.Value.NodeType != YamlNodeType.Scalar)
            {
                return null;
            }

            var scalarNode = (YamlScalarNode) classChild.Value;
            return scalarNode.Value?.ToLowerInvariant();
        }
    }
}