using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using NetDaemon.Common;
using NetDaemon.Common.Exceptions;
using NetDaemon.Daemon.Config;

[assembly: InternalsVisibleTo("NetDaemon.Daemon.Tests")]

namespace NetDaemon.Daemon
{
    public sealed class CodeManager : IInstanceDaemonApp
    {
        private readonly ILogger _logger;
        private readonly IYamlConfig _yamlConfig;
        private readonly IEnumerable<Type> _loadedDaemonApps;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="daemonAppTypes">App compiled app types</param>
        /// <param name="logger">ILogger instance to use</param>
        /// <param name="yamlConfig"></param>
        public CodeManager(IEnumerable<Type> daemonAppTypes, ILogger logger, IYamlConfig yamlConfig)
        {
            _logger = logger;
            _loadedDaemonApps = daemonAppTypes;
            _yamlConfig = yamlConfig;
        }

        [SuppressMessage("", "CA1065")] public int Count => _loadedDaemonApps?.Count() ?? throw new NetDaemonNullReferenceException("_loadedDaemonApps cannot be null");

        // Internal for testing
        public IEnumerable<Type> DaemonAppTypes => _loadedDaemonApps!;

        public IEnumerable<ApplicationContext> InstanceDaemonApps(IServiceProvider serviceProvider)
        {
            var result = new List<ApplicationContext>(50);

            // Get all yaml config file paths
            var allConfigFilePaths = _yamlConfig.GetAllConfigFilePaths().ToArray();

            if (_loadedDaemonApps?.Any() != true && allConfigFilePaths.Length == 0)
            {
                _logger.LogWarning("No yaml configuration files or loaded apps found");
                return result;
            }

            var appInstantiator = new AppInstantiator(serviceProvider);

            foreach (string file in allConfigFilePaths)
            {
                try
                {
                    using var fileReader = File.OpenText(file);
                    var yamlAppConfig = new YamlAppConfig(_loadedDaemonApps, fileReader, _yamlConfig, file, appInstantiator);

                    foreach (var appInstance in yamlAppConfig.GetInstances())
                    {
                        result.Add(appInstance);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogTrace(e, "Error instance the app from the file {file}", file);
                    _logger.LogError("Error instance the app from the file {file}, use trace flag for details", file);
                    throw new NetDaemonException($"Error instance the app from the file {file}", e);
                }
            }

            var appTypesWithoutYamlConfig = _loadedDaemonApps
                .Where(appType => !result.Select(x => x.ApplicationInstance.GetType()).Contains(appType));

            foreach (var appType in appTypesWithoutYamlConfig)
            {
                var appId = appType.GetCustomAttribute<NetDaemonAppAttribute>()?.Id ?? appType.Name;

                result.Add(appInstantiator.Instantiate(appType, appId));
            }

            return result;
        }
    }
}