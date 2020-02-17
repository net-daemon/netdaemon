using JoySoftware.HomeAssistant.NetDaemon.Common;
using JoySoftware.HomeAssistant.NetDaemon.DaemonRunner.Service.Config;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

[assembly: InternalsVisibleTo("NetDaemon.Daemon.Tests")]
namespace JoySoftware.HomeAssistant.NetDaemon.DaemonRunner.Service
{
    public interface IDaemonAppConfig
    {
        Task InstanceFromDaemonAppConfigs(IEnumerable<Type> netDaemonApps, string codeFolder);
    }
    public static class TaskExtensions
    {
        public static async Task InvokeAsync(this MethodInfo mi, object obj, params object[] parameters)
        {
            dynamic awaitable = mi.Invoke(obj, parameters);
            await awaitable;
            //return awaitable.GetAwaiter().GetResult();
        }
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
            // First check if we have any secrets.yaml files

            // await LoadSecretsSettings(codeFolder);

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
                    await netDaemonApp.StartUpAsync(_daemon);

                    _logger.LogInformation($"Loading App ({netDaemonAppType.Name})");
                    foreach (var method in netDaemonAppType.GetMethods())
                    {
                        foreach (var attr in method.GetCustomAttributes(false))
                        {
                            switch (attr)
                            {
                                case HomeAssistantServiceCallAttribute hasstServiceCallAttribute:
                                    if (!CheckIfServiceCallSignatureIsOk(method))
                                        continue;
                                    dynamic serviceData = new FluentExpandoObject();
                                    serviceData.method = method.Name;
                                    serviceData.@class = netDaemonAppType.Name;
                                    _daemon.CallService("netdaemon", "register_service", serviceData);

                                    _daemon.ListenServiceCall("netdaemon", $"{serviceData.@class}_{serviceData.method}",
                                        async (data) =>
                                        {
                                            try
                                            {
                                                var expObject = data as ExpandoObject;
                                                await method.InvokeAsync(netDaemonApp, expObject);
                                            }
                                            catch (Exception e)
                                            {
                                                _logger.LogError(e, "Failed to invoke the ServiceCall funcition");
                                            }
                                        });

                                    break;
                                case HomeAssistantStateChangedAttribute hassStateChangedAttribute:

                                    if (!CheckIfStateChangedSignatureIsOk(method))
                                        continue;

                                    _daemon.ListenState(hassStateChangedAttribute.EntityId,
                                    async (entityId, to, from) =>
                                    {
                                        try
                                        {
                                            if (hassStateChangedAttribute.To != null)
                                                if ((dynamic)hassStateChangedAttribute.To != to?.State)
                                                    return;

                                            if (hassStateChangedAttribute.From != null)
                                                if ((dynamic)hassStateChangedAttribute.From != from?.State)
                                                    return;

                                            // If we don´t accept all changes in the state change
                                            // and we do not have a state change so return
                                            if (to?.State == from?.State && !hassStateChangedAttribute.AllChanges)
                                                return;

                                            await method.InvokeAsync(netDaemonApp, entityId, to, from);
                                        }
                                        catch (Exception e)
                                        {
                                            _logger.LogError(e, "Failed to invoke the ServiceCall funcition");
                                        }

                                    });

                                    break;
                            }
                        }
                    }
                    await netDaemonApp.InitializeAsync();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Failed to initialize app {netDaemonAppType.Name}");
                }
            }
        }

        private bool CheckIfServiceCallSignatureIsOk(MethodInfo method)
        {
            if (method.ReturnType != typeof(Task))
            {
                _logger.LogWarning($"{method.Name} has not correct return type, expected Task");
                return false;
            }

            var parameters = method.GetParameters();

            if (parameters == null || (parameters != null && parameters.Length != 1))
            {
                _logger.LogWarning($"{method.Name} has not correct number of parameters");
                return false;
            }

            var dynParam = parameters![0];
            if (dynParam.CustomAttributes.Count() == 1 && dynParam.CustomAttributes.First().AttributeType == typeof(DynamicAttribute))
                return true;

            return false;
        }

        private bool CheckIfStateChangedSignatureIsOk(MethodInfo method)
        {
            if (method.ReturnType != typeof(Task))
            {
                _logger.LogWarning($"{method.Name} has not correct return type, expected Task");
                return false;
            }

            var parameters = method.GetParameters();

            if (parameters == null || (parameters != null && parameters.Length != 3))
            {
                _logger.LogWarning($"{method.Name} has not correct number of parameters");
                return false;
            }

            if (parameters![0].ParameterType != typeof(string))
            {
                _logger.LogWarning($"{method.Name} first parameter exepected to be string for entityId");
                return false;
            }
            if (parameters![1].ParameterType != typeof(EntityState))
            {
                _logger.LogWarning($"{method.Name} second parameter exepected to be EntityState for toState");
                return false;
            }
            if (parameters![2].ParameterType != typeof(EntityState))
            {
                _logger.LogWarning($"{method.Name} first parameter exepected to be EntityState for fromState");
                return false;
            }
            return true;
        }
    }
}