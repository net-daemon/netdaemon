using JoySoftware.HomeAssistant.NetDaemon.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace JoySoftware.HomeAssistant.NetDaemon.DaemonRunner.Service
{
    public interface IDaemonAppConfig
    {
        Task InstanceFromDaemonAppConfig(IEnumerable<Type> netDaemonAppType, string currentFolder);
    }

    public static class PropertyInfoExtensions
    {
        public static IList? CreateListOfPropertyType(this Type listType)
        {
            Type gen = typeof(List<>).MakeGenericType(listType!);
            var list = Activator.CreateInstance(gen);

            return list as IList;
        }
    }

    public static class YamlExtensions
    {
        public static object? ToObject(this YamlScalarNode node, Type valueType)
        {
            var underlyingNullableType = Nullable.GetUnderlyingType(valueType);
            if (underlyingNullableType != null)
            {
                // It is nullable type
                valueType = underlyingNullableType;
            }
            switch (valueType.Name)
            {
                case "String":
                    return node.Value;

                case "Int32":
                    if (Int32.TryParse(node.Value, NumberStyles.Number,
                        CultureInfo.InvariantCulture, out var i32Value))
                    {
                        return i32Value;
                    }
                    break;

                case "Int64":
                    if (Int64.TryParse(node.Value, NumberStyles.Number,
                        CultureInfo.InvariantCulture, out var i64Value))
                    {
                        return i64Value;
                    }
                    break;

                case "Decimal":
                    if (Decimal.TryParse(node.Value, NumberStyles.Number,
                        CultureInfo.InvariantCulture, out var decimalValue))
                    {
                        return decimalValue;
                    }
                    break;

                case "Float":
                    if (Decimal.TryParse(node.Value, NumberStyles.Number,
                        CultureInfo.InvariantCulture, out var floatValue))
                    {
                        return floatValue;
                    }
                    break;

                case "Double":
                    if (Double.TryParse(node.Value, NumberStyles.Number,
                        CultureInfo.InvariantCulture, out var doubleValue))
                    {
                        return doubleValue;
                    }
                    break;

                case "Boolean":
                    if (bool.TryParse(node.Value, out var boolValue))
                    {
                        return boolValue;
                    }
                    break;
            }
            return null;
        }
    }

    public class DaemonAppConfig : IDaemonAppConfig
    {
        private readonly INetDaemon _daemon;
        private readonly ILogger _logger;

        public DaemonAppConfig(INetDaemon daemonHost, ILogger logger)
        {
            _daemon = daemonHost;
            _logger = logger;
        }

        public async Task InstanceFromDaemonAppConfig(IEnumerable<Type> netDaemonApps, string sourceFilePath)
        {
            var configPath = Path.ChangeExtension(sourceFilePath, "yaml");
            if (!File.Exists(configPath))
            {
                // No configuration file exists, just instance the classes without config
                foreach (var app in netDaemonApps)
                {
                    _logger.LogInformation($"Loading App ({app.Name}), no config found");

                    var daemonApp = (NetDaemonApp)Activator.CreateInstance(app);
                    if (daemonApp != null)
                    {
                        try
                        {
                            await daemonApp.StartUpAsync(_daemon);
                            await daemonApp.InitializeAsync();
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, $"Failed to initialize app {app.Name} in file {Path.GetFileName(sourceFilePath)}");
                        }
                    }
                }
                return;
            }

            //    // No config for app, instance it without config
            //    var netDaemonApp = Activator.CreateInstance(netDaemonAppType);

            //if (netDaemonApp == null)
            //    return null;

            var yaml = new YamlStream();

            yaml.Load(File.OpenText(configPath));

            var mapping =
            (YamlMappingNode)yaml.Documents[0].RootNode;

            foreach (var instance in mapping.Children)
            {
                if (instance.Value.NodeType != YamlNodeType.Mapping)
                {
                    _logger.LogWarning($"Unexpected configuration, expected class name for instance for file {Path.GetFileName(configPath)}");
                    return;
                }

                // Get the class
                var classChild = ((YamlMappingNode)instance.Value).Children.Where(n =>
                        ((YamlScalarNode)n.Key)?.Value?.ToLowerInvariant() == "class").FirstOrDefault();

                if (classChild.Key == null)
                {
                    _logger.LogWarning($"Failure, Class configuration is not correct in file {Path.GetFileName(sourceFilePath)}");
                    continue;
                }

                if (classChild.Value.NodeType != YamlNodeType.Scalar)
                {
                    _logger.LogWarning($"Failure, Class configuration is not correct in file {Path.GetFileName(sourceFilePath)}");
                    continue;
                }
                var appClass = ((YamlScalarNode)classChild.Value).Value.ToLowerInvariant(); ;

                var netDaemonAppType = netDaemonApps.FirstOrDefault(n => n.Name.ToLowerInvariant() == appClass);

                if (netDaemonAppType == null)
                {
                    _logger.LogWarning($"No class with name {((YamlScalarNode?)instance.Key)?.Value} exists in {Path.GetFileName(configPath)} ");
                    continue;
                }

                var netDaemonApp = (NetDaemonApp?)Activator.CreateInstance(netDaemonAppType);

                if (netDaemonApp == null)
                {
                    _logger.LogError($"Failed to create instance for the app {netDaemonAppType.Name}");
                    continue;
                }

                foreach (var entry in ((YamlMappingNode)instance.Value).Children)
                {
                    var key = ((YamlScalarNode)entry.Key).Value;

                    if (key == null)
                        return;

                    if (key.ToLowerInvariant() == "class")
                        continue; // Ignore the class

                    var valueType = entry.Value.NodeType;

                    var prop = netDaemonAppType.GetProperty(key);

                    if (prop == null)
                    {
                        _logger.LogWarning($"No property on class {netDaemonAppType.Name} matches {key}");
                        continue;
                    }

                    switch (valueType)
                    {
                        case YamlNodeType.Sequence:
                            var seq = (YamlSequenceNode)entry.Value;

                            if (prop.PropertyType.IsGenericType && prop.PropertyType?.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                            {
                                //var list = (IList) Activator.CreateInstance(prop.DeclaringType);
                                var listType = prop.PropertyType?.GetGenericArguments()[0];
                                var list = listType.CreateListOfPropertyType();
                                foreach (var item in seq.Children)
                                {
                                    if (item.NodeType != YamlNodeType.Scalar)
                                    {
                                        _logger.LogWarning($"The class {netDaemonAppType.Name} and property {key} can only accept ");
                                        return;
                                    }
                                    var value = ((YamlScalarNode)item).ToObject(listType);

                                    if (value != null)
                                    {
                                        list?.Add(value);
                                    }
                                    else
                                    {
                                        _logger.LogWarning($"The class {netDaemonAppType.Name} and property {key} has wrong type in items");
                                    }
                                }
                                // Bind the list to the property
                                prop.SetValue(netDaemonApp, list);
                            }

                            break;

                        case YamlNodeType.Scalar:
                            var sc = (YamlScalarNode)entry.Value;
                            var scalarValue = sc.ToObject(prop.PropertyType);
                            if (scalarValue != null)
                            {
                                // Bind the list to the property
                                prop.SetValue(netDaemonApp, scalarValue);
                            }
                            else
                            {
                                _logger.LogWarning($"The class {netDaemonAppType.Name} and property {key} unexpected value {sc.Value} is wrong type");
                                return;
                            }
                            break;

                        case YamlNodeType.Mapping:
                            var map = (YamlMappingNode)entry.Value;
                            break;
                    }
                }
                if (netDaemonApp != null)
                {
                    try
                    {
                        _logger.LogInformation($"Loading App ({appClass})");
                        await netDaemonApp.StartUpAsync(_daemon);
                        await netDaemonApp.InitializeAsync();
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, $"Failed to initialize app {netDaemonAppType.Name} in file {Path.GetFileName(sourceFilePath)}");
                    }
                }
            }
        }
    }
}