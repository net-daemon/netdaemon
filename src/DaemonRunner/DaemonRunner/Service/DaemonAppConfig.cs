using JoySoftware.HomeAssistant.NetDaemon.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace JoySoftware.HomeAssistant.NetDaemon.DaemonRunner.Service
{
    public interface IDaemonAppConfig
    {
        Task InstanceFromDaemonAppConfigs(IEnumerable<Type> netDaemonApps, string codeFolder);
    }

    public static class ConfigStringExtensions
    {
        public static string ToPythonStyle(this string str)
        {
            var build = new StringBuilder(str.Length);
            bool isStart = true;
            foreach (char c in str)
            {
                if (char.IsUpper(c) && !isStart)
                    build.Append("_");
                else
                    isStart = false;
                build.Append(char.ToLower(c));
            }
            return build.ToString();
        }

        public static string ToCamelCase(this string str)
        {
            var build = new StringBuilder();
            bool nextIsUpper = false;
            bool isFirstCharacter = true;
            foreach (char c in str)
            {
                if (c == '_')
                {
                    nextIsUpper = true;
                    continue;
                }
                   
                build.Append(nextIsUpper || isFirstCharacter ? char.ToUpper(c) : c);
                nextIsUpper = false;
                isFirstCharacter = false;
            }
            var returnString = build.ToString();
           
            return build.ToString();
        }
    }
    public static class PropertyInfoExtensions
    {
        public static IList? CreateListOfPropertyType(this Type listType)
        {
            Type gen = typeof(List<>).MakeGenericType(listType!);
            object? list = Activator.CreateInstance(gen);

            return list as IList;
        }
    }

    public static class YamlExtensions
    {
        public static object? ToObject(this YamlScalarNode node, Type valueType)
        {
            Type? underlyingNullableType = Nullable.GetUnderlyingType(valueType);
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
                    if (int.TryParse(node.Value, NumberStyles.Number,
                        CultureInfo.InvariantCulture, out int i32Value))
                    {
                        return i32Value;
                    }
                    break;

                case "Int64":
                    if (long.TryParse(node.Value, NumberStyles.Number,
                        CultureInfo.InvariantCulture, out long i64Value))
                    {
                        return i64Value;
                    }
                    break;

                case "Decimal":
                    if (decimal.TryParse(node.Value, NumberStyles.Number,
                        CultureInfo.InvariantCulture, out decimal decimalValue))
                    {
                        return decimalValue;
                    }
                    break;

                case "Float":
                    if (decimal.TryParse(node.Value, NumberStyles.Number,
                        CultureInfo.InvariantCulture, out decimal floatValue))
                    {
                        return floatValue;
                    }
                    break;

                case "Double":
                    if (double.TryParse(node.Value, NumberStyles.Number,
                        CultureInfo.InvariantCulture, out double doubleValue))
                    {
                        return doubleValue;
                    }
                    break;

                case "Boolean":
                    if (bool.TryParse(node.Value, out bool boolValue))
                    {
                        return boolValue;
                    }
                    break;
            }
            return null;
        }
    }

    public class AppInfo
    {
        public string AppId { get; set; }
        public string SourcePath { get; set; }
        public Type AppType { get; set; }

        public YamlMappingNode YamlConfig { get; set; }
    }

    public class DaemonAppConfig : IDaemonAppConfig
    {
        private readonly INetDaemon _daemon;
        private readonly ILogger _logger;
        private string? _fileInProcess;

        public DaemonAppConfig(INetDaemon daemonHost, ILogger logger)
        {
            _daemon = daemonHost;
            _logger = logger;
        }

        public async Task InstanceFromDaemonAppConfigs(IEnumerable<Type> netDaemonApps, string codeFolder)
        {
            var yamlConfigs = new Dictionary<string, List<YamlMappingNode>>(10);
            foreach (string file in Directory.EnumerateFiles(codeFolder, "*.yaml", SearchOption.AllDirectories))
            {
                try
                {
                    _fileInProcess = file;

                    var yaml = new YamlStream();
                    yaml.Load(File.OpenText(file));

                    // For each app instance defined in the yaml config
                    foreach (KeyValuePair<YamlNode, YamlNode> app in (YamlMappingNode)yaml.Documents[0].RootNode)
                    {
                        if (app.Key.NodeType != YamlNodeType.Scalar ||
                            app.Value.NodeType != YamlNodeType.Mapping)
                        {
                            continue;
                        }

                        string? appId = ((YamlScalarNode)app.Key).Value;
                        // Get the class

                        string? appClass = GetTypeNameFromClassConfig((YamlMappingNode)app.Value);
                        Type appType = netDaemonApps.Where(n => n.Name.ToLowerInvariant() == appClass)
                                                       .FirstOrDefault();

                        if (appType != null)
                        {
                            await InstanceAndSetPropertyConfig(appType, ((YamlMappingNode)app.Value));
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, $"Failed to load yaml file  {_fileInProcess}");
                    // do not throw, just keep going with next file
                }
            }
        }

        private string? GetTypeNameFromClassConfig(YamlMappingNode appNode)
        {
            KeyValuePair<YamlNode, YamlNode> classChild = appNode.Children.Where(n =>
                                   ((YamlScalarNode)n.Key)?.Value?.ToLowerInvariant() == "class").FirstOrDefault();

            if (classChild.Key == null || classChild.Value == null)
            {
                _logger.LogWarning($"Failure, Class configuration is not correct in file {_fileInProcess}");
                return null;
            }

            if (classChild.Value.NodeType != YamlNodeType.Scalar)
            {
                _logger.LogWarning($"Failure, Class configuration is not correct in file {_fileInProcess}");
                return null;
            }
            return ((YamlScalarNode)classChild.Value)?.Value?.ToLowerInvariant();
        }

        public async Task InstanceAndSetPropertyConfig(Type netDaemonAppType, YamlMappingNode appNode)
        {
            var netDaemonApp = (NetDaemonApp?)Activator.CreateInstance(netDaemonAppType);

            if (netDaemonApp == null)
            {
                _logger.LogError($"Failed to create instance for the app {netDaemonAppType.Name}");
                return;
            }

            foreach (KeyValuePair<YamlNode, YamlNode> entry in appNode.Children)
            {
                string? key = ((YamlScalarNode)entry.Key).Value;

                if (key == null)
                {
                    return;
                }

                if (key.ToLowerInvariant() == "class")
                {
                    continue; // Ignore the class
                }

                YamlNodeType valueType = entry.Value.NodeType;

                System.Reflection.PropertyInfo? prop = netDaemonAppType.GetProperty(key);

                if (prop == null)
                {
                    // Lets try convert from python style to CamelCase
                    prop = netDaemonAppType.GetProperty(key.ToCamelCase());
                    if (prop == null)
                    {
                        _logger.LogWarning($"No property on class {netDaemonAppType.Name} matches {key}");
                        continue;
                    }
                    
                }

                switch (valueType)
                {
                    case YamlNodeType.Sequence:
                        var seq = (YamlSequenceNode)entry.Value;

                        if (prop.PropertyType.IsGenericType && prop.PropertyType?.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                        {
                            //var list = (IList) Activator.CreateInstance(prop.DeclaringType);
                            Type? listType = prop.PropertyType?.GetGenericArguments()[0];
                            IList? list = listType.CreateListOfPropertyType();
                            foreach (YamlNode item in seq.Children)
                            {
                                if (item.NodeType != YamlNodeType.Scalar)
                                {
                                    _logger.LogWarning($"The class {netDaemonAppType.Name} and property {key} can only accept ");
                                    return;
                                }
                                object? value = ((YamlScalarNode)item).ToObject(listType);

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
                        object? scalarValue = sc.ToObject(prop.PropertyType);
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
                    _logger.LogInformation($"Loading App ({netDaemonAppType.Name})");
                    await netDaemonApp.StartUpAsync(_daemon);
                    await netDaemonApp.InitializeAsync();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Failed to initialize app {netDaemonAppType.Name}");
                }
            }
        }
    }
}