using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetDaemon.Common.Exceptions;
using YamlDotNet.RepresentationModel;

namespace NetDaemon.Daemon.Config
{
    internal class YamlAppConfigProvider
    {
        private readonly IYamlConfig _yamlConfig;
        private readonly ILogger _logger;
        private readonly List<(string AppFullTypeName, YamlAppConfigEntry Config)> _appConfigs = new();

        public YamlAppConfigProvider(IYamlConfig yamlConfig, ILogger logger)
        {
            _yamlConfig = yamlConfig;
            _logger = logger;

            LoadAppConfigs();
        }

        [SuppressMessage("", "CA1508")]
        [SuppressMessage("", "CA1065")]
        public IEnumerable<YamlAppConfigEntry> GetConfigs(Type appType)
        {
            var appTypeFullName = appType.FullName!.ToLowerInvariant();

            return _appConfigs.Where(x => x.AppFullTypeName == appTypeFullName).Select(x => x.Config);
        }

        private void LoadAppConfigs()
        {
            try
            {
                foreach (var yamlConfigInstance in _yamlConfig.GetAllConfigs())
                {
                    var yamlStream = yamlConfigInstance.GetYamlStream();

                    foreach (KeyValuePair<YamlNode, YamlNode> app in (YamlMappingNode)yamlStream.Documents[0].RootNode)
                    {
                        if (app.Key.NodeType != YamlNodeType.Scalar ||
                            app.Value.NodeType != YamlNodeType.Mapping)
                        {
                            continue;
                        }

                        var appValue = (YamlMappingNode) app.Value;

                        var appTypeFullName = GetTypeNameFromClassConfig(appValue);
                        if (appTypeFullName is null)
                        {
                            continue;
                        }

                        var appId = ((YamlScalarNode)app.Key).Value!;
                        var appConfiguration = new YamlAppConfigEntry(appId, appValue, yamlConfigInstance);

                        _appConfigs.Add((appTypeFullName, appConfiguration));
                    }
                }
            }
            catch (Exception e)
            {
                const string? message = "Error loading yaml app configs";

                _logger.LogTrace(e, $"{message}, use trace flag for details");
                _logger.LogError(message);

                throw new NetDaemonException(message, e);
            }
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