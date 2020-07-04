using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using NetDaemon.Common;
using NetDaemon.Daemon.Config;

[assembly: InternalsVisibleTo("NetDaemon.Daemon.Tests")]

namespace NetDaemon.Daemon
{
    public sealed class CodeManager : IInstanceDaemonApp
    {
        private readonly string _codeFolder;
        private readonly ILogger _logger;
        private readonly YamlConfig _yamlConfig;
        private IEnumerable<Type>? _loadedDaemonApps;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="codeFolder">Path to code folder</param>
        /// <param name="daemonAppTypes">App compiled app types</param>
        /// <param name="logger">ILogger instance to use</param>
        public CodeManager(string codeFolder, IEnumerable<Type> daemonAppTypes, ILogger logger)
        {
            _logger = logger;
            _codeFolder = codeFolder;
            _loadedDaemonApps = daemonAppTypes;
            _yamlConfig = new YamlConfig(codeFolder);
        }

        public int Count => _loadedDaemonApps.Count();

        // Internal for testing
        public IEnumerable<Type> DaemonAppTypes => _loadedDaemonApps!;

        public IEnumerable<INetDaemonAppBase> InstanceDaemonApps()
        {
            var result = new List<INetDaemonAppBase>(50);

            // No loaded, just return an empty list
            if (_loadedDaemonApps is null || _loadedDaemonApps.Count() == 0)
                return result;

            // Get all yaml config file paths
            var allConfigFilePaths = _yamlConfig.GetAllConfigFilePaths();

            if (allConfigFilePaths.Count() == 0)
            {
                _logger.LogWarning("No yaml configuration files found, please add files to [netdaemonfolder]/apps");
                return result;
            }

            foreach (string file in allConfigFilePaths)
            {
                var yamlAppConfig = new YamlAppConfig(_loadedDaemonApps, File.OpenText(file), _yamlConfig, file);

                foreach (var appInstance in yamlAppConfig.Instances)
                {
                    result.Add(appInstance);
                }
            }
            return result;
        }
    }
}