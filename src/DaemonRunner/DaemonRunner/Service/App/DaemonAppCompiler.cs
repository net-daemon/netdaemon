using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetDaemon.Common;
using NetDaemon.Common.Configuration;
using NetDaemon.Common.Exceptions;
using NetDaemon.Infrastructure.Extensions;

namespace NetDaemon.Service.App
{
    public class DaemonAppCompiler : IDaemonAppCompiler
    {
        private readonly ILogger<DaemonAppCompiler> _logger;

        private readonly string? _sourceFolder;
        public DaemonAppCompiler(ILogger<DaemonAppCompiler> logger, IOptions<NetDaemonSettings> netDaemonSettings)
        {
            _ = netDaemonSettings ??
               throw new NetDaemonArgumentNullException(nameof(netDaemonSettings));
            _logger = logger;
            NetDaemonSettings = netDaemonSettings;
            _sourceFolder = netDaemonSettings.Value.GetAppSourceDirectory();
        }

        public IOptions<NetDaemonSettings> NetDaemonSettings { get; }

        public IEnumerable<Type> GetApps()
        {
            _logger.LogDebug("Loading dynamically compiled apps...");
            var assembly = Load();
            var apps = assembly.GetAppClasses().ToList();

            if (!apps.Any())
                _logger.LogWarning("No .cs files found, please add files to {SourceFolder}", _sourceFolder);
            else
                _logger.LogDebug("Found total of {NumberOfApps} apps", apps.Count());

            return apps;
        }

        private Assembly? _generatedAssemby;

        public IServiceCollection RegisterDynamicServices(IServiceCollection serviceCollection)
        {
            var assemby = Load();
            var methods = assemby?.GetTypes().SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public))
                .Where(m => m.GetCustomAttribute<ServiceCollectionExtensionAttribute>() != null);

            foreach (var methodInfo in methods ?? Array.Empty<MethodInfo>())
            {
                methodInfo.Invoke(null, new object?[]{ serviceCollection });
            }

            return serviceCollection;
        }
 
        public Assembly? Load()
        {
            return _generatedAssemby ??= DaemonCompiler.GetCompiledAppAssembly(out _, _sourceFolder!, _logger);
        }
    }
    
}