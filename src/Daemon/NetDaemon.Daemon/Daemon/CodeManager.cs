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

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="daemonAppTypes">App compiled app types</param>
        /// <param name="logger">ILogger instance to use</param>
        /// <param name="yamlConfig"></param>
        public CodeManager(IEnumerable<Type> daemonAppTypes, ILogger logger, IYamlConfig yamlConfig)
        {
            _logger = logger;
            DaemonAppTypes = daemonAppTypes;
            _yamlConfig = yamlConfig;
        }

        [SuppressMessage("", "CA1065")]
        public int Count => DaemonAppTypes.Count();

        // Internal for testing
        internal IEnumerable<Type> DaemonAppTypes { get; }

        public IEnumerable<ApplicationContext> InstanceDaemonApps(IServiceProvider serviceProvider)
        {
            if (!DaemonAppTypes.Any() && !_yamlConfig.GetAllConfigs().Any())
            {
                _logger.LogWarning("No yaml configuration files or loaded apps found");
                yield break;
            }

            var appInstantiator = new AppInstantiator(serviceProvider, _logger);
            var yamlAppConfigProvider = new YamlAppConfigProvider(_yamlConfig, _logger);

            foreach (Type appType in DaemonAppTypes)
            {
                var appConfigs = yamlAppConfigProvider.GetConfigs(appType).ToArray();

                if (appConfigs.Length == 0)
                {
                    var appId = appType.GetCustomAttribute<NetDaemonAppAttribute>()?.Id ?? appType.Name;

                    yield return appInstantiator.Instantiate(appType, appId);
                }

                foreach (var appConfig in appConfigs)
                {
                    var appContext = appInstantiator.Instantiate(appType, appConfig.AppId);

                    appConfig.SetPropertyConfig(appContext);

                    yield return appContext;
                }
            }
        }
    }
}