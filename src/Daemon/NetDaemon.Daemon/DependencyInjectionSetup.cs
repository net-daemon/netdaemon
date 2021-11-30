using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using JoySoftware.HomeAssistant.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetDaemon.Common;
using NetDaemon.Daemon;
using NetDaemon.Extensions.Scheduler;

namespace NetDaemon
{
    public static class DependencyInjectionSetup
    {
        public static IHostBuilder UseNetDaemonHostSingleton(this IHostBuilder hostBuilder)
        {
            if (hostBuilder == null) throw new ArgumentNullException(nameof(hostBuilder));

            return hostBuilder.ConfigureServices((_, services) => { AddNetDaemonServices(services); });
        }

        public static IServiceCollection AddNetDaemonServices(this IServiceCollection services)
        {
            // replace the transient IHassClient with a singleton
            services.AddSingleton<IHassClient, HassClient>();

            services.AddSingleton<NetDaemonHost>();
            services.AddSingleton<INetDaemonHost>(s => s.GetRequiredService<NetDaemonHost>());
            services.AddSingleton<INetDaemon>(s => s.GetRequiredService<NetDaemonHost>());

            // Provide services created by NetDaemonHost as separate services so apps only need to take a dependency on 
            // these interfaces and methods directly on INetDaemonHost can eventually be removed
            services.AddSingleton(s => s.GetRequiredService<NetDaemonHost>().HassEventsObservable);
            services.AddSingleton<ITextToSpeechService>(s => s.GetRequiredService<NetDaemonHost>().TextToSpeechService);
            services.AddNetDaemonScheduler();
            services.AddNetDaemonAppServices();
            services.AddNetDaemonAppsDiServices();

            return services;
        }
        
        /// <summary>
        /// Registers services from NetDaemon.App assembly
        /// </summary>
        public static IServiceCollection AddNetDaemonAppsDiServices(this IServiceCollection services)
        {
            // need rework
            var compiler = new AppCompiler();
            var apps = compiler.GetApps();
            
            foreach (var appType in apps)
            {
                services.AddSingleton(appType,provider =>
                {
                    var host = provider.GetRequiredService<NetDaemonHost>();
                    var app = host.GetNetApp(appType.Name);
                    
                    if (app?.GetType() == appType)
                    {
                        return app;
                    }

                    throw new Exception("app not found");
                });
                
                var configFn = appType.GetMethod("ConfigureServices", BindingFlags.Public | BindingFlags.Static);
                
                if (configFn != null)
                {
                    configFn.Invoke(null, new object?[]{services});
                }
            }

            return services;
        }

        class AppCompiler
        {
            public IEnumerable<Type> GetApps()
            {
                // _logger.LogDebug("Loading local assembly apps...");

                var assemblies = LoadAll();
                var apps = assemblies.SelectMany(x => GetAppClasses(x)).ToList();

                // if (apps.Count == 0)
                //     _logger.LogWarning("No local daemon apps found.");
                // else
                //     _logger.LogDebug("Found total of {NumberOfApps} apps", apps.Count);

                return apps;
            }
            
            private IEnumerable<Type> GetAppClasses(Assembly assembly)
            {
                return assembly.GetTypes()
                    .Where(type => type.IsClass && 
                                   !type.IsGenericType && 
                                   !type.IsAbstract && 
                                   (type.IsAssignableTo(typeof(INetDaemonAppBase)) || type.GetCustomAttribute<NetDaemonAppAttribute>() != null
                                   ));
            }

            private IEnumerable<Assembly> LoadAll()
            {
                var binFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location)!;
                var netDaemonDlls = Directory.GetFiles(binFolder, "NetDaemon.*.dll");

                var alreadyLoadedAssemblies = AssemblyLoadContext.Default.Assemblies
                    .Where(x => !x.IsDynamic)
                    .Select(x => x.Location)
                    .ToList();

                foreach (var netDaemonDllToLoadDynamically in netDaemonDlls.Except(alreadyLoadedAssemblies))
                {
                    // _logger.LogTrace("Loading {Dll} into AssemblyLoadContext", netDaemonDllToLoadDynamically);
                    AssemblyLoadContext.Default.LoadFromAssemblyPath(netDaemonDllToLoadDynamically);
                }

                return AssemblyLoadContext.Default.Assemblies;
            }
        }
    }
}