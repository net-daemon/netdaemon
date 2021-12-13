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
            var apps = assembly?.GetAppClasses().ToList();

            if (apps?.Any() == false)
                _logger.LogWarning("No .cs files found, please add files to {SourceFolder}", _sourceFolder);
            else
                _logger.LogDebug("Found total of {NumberOfApps} apps", apps?.Count);

            return apps!;
        }

        private Assembly? _generatedAssembly;

        public IServiceCollection RegisterDynamicServices(IServiceCollection serviceCollection)
        {
            var assembly = Load();
            var methods = assembly?.GetTypes().SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public))
                .Where(m => m.GetCustomAttribute<ServiceCollectionExtensionAttribute>() != null).ToArray() ?? Array.Empty<MethodInfo>();

            if (methods.Any(m => m.GetParameters().Length != 1 && m.GetParameters()[0].GetType() != typeof(IServiceProvider)))
            {
                throw new InvalidOperationException("Methods with [ServiceCollectionExtension] Attribute should have exactly one parameter of type IServiceCollection");
            }
    
            foreach (var methodInfo in methods )
            {
                methodInfo.Invoke(null, new object?[]{ serviceCollection });
            }

            return serviceCollection;
        }

        public Assembly? Load()
        {
            return _generatedAssembly ??= DaemonCompiler.GetCompiledAppAssembly(out _, _sourceFolder!, _logger);
        }
    }
    
}