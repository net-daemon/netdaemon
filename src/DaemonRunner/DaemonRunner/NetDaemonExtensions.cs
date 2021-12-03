using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetDaemon.Common.Configuration;
using NetDaemon.Daemon.Config;
using NetDaemon.Service;
using NetDaemon.Service.App;
using Serilog;
using NetDaemon.Infrastructure.Config;
using NetDaemon.Common.Exceptions;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NetDaemon.Assemblies;
using NetDaemon.Daemon;
using NetDaemon.HassModel;
using NetDaemon.Service.App.CodeGeneration;

namespace NetDaemon
{
    public static class NetDaemonExtensions
    {
        private const string HassioConfigPath = "/data/options.json";

        public static IHostBuilder UseNetDaemon(this IHostBuilder hostBuilder)
        {
            _ = hostBuilder ??
               throw new NetDaemonArgumentNullException(nameof(hostBuilder));

            if (File.Exists(HassioConfigPath))
                ReadHassioConfig();

            return hostBuilder
                .ConfigureServices((context, services) =>
                {
                    var netDaemonSettings = context.Configuration.GetSection("NetDaemon");
                    services.Configure<HomeAssistantSettings>(context.Configuration.GetSection("HomeAssistant"));
                    services.Configure<NetDaemonSettings>(netDaemonSettings);
                    services.AddSingleton<NetDaemonSettings>(netDaemonSettings.Get<NetDaemonSettings>()); // temp fix for access config in compiler
                    services.AddSingleton<IYamlConfig, YamlConfigProvider>();
                    services.AddSingleton<ICodeGenerationHandler, CodeGenerationHandler>();
                    services.AddSingleton<ICodeGenerator, CodeGenerator>();
                    services.AddSingleton<IYamlConfigReader, YamlConfigReader>();
                    services.AddSingleton<IIoWrapper, IoWrapper>();
                    
                    // add app compiler
                    services.AddSingleton<IDaemonAppCompiler, DaemonAppCompiler>();
                })
                .ConfigureWebHostDefaults(webbuilder =>
                {
                    webbuilder.UseKestrel(_ => { });
                    webbuilder.UseStartup<ApiStartup>();
                })
                .UseNetDaemonHostSingleton()
                .UseNetDaemonAssemblyCompiler()
                .AddNetDaemonAppsDiServices()
                .UseAppScopedHaContext()
                ;
        }

        /// <summary>
        /// Registers services from each NetDaemon App with static ConfigureServices method
        /// </summary>
        public static IHostBuilder AddNetDaemonAppsDiServices(this IHostBuilder hostBuilder)
        {
            if (hostBuilder == null) throw new ArgumentNullException(nameof(hostBuilder));
        
            return hostBuilder.ConfigureServices((_, services) => { AddNetDaemonAppsDiServices(services); });
        }

        /// <summary>
        /// Registers services from each NetDaemon App with static ConfigureServices method
        /// </summary>
        public static IServiceCollection AddNetDaemonAppsDiServices(this IServiceCollection services)
        {
            var assembliesManager = services.GetNetDaemonAssemblyManager();
            assembliesManager.ConfigureAssemblies(assemblies =>
            {
                var servicesCompiler = new DaemonAppServicesCompiler(new NullLogger<DaemonAppServicesCompiler>());
                var appsCompiler = new DaemonAppCompiler(new NullLogger<DaemonAppCompiler>());
                var appServices = servicesCompiler.GetAppServices(assemblies);
                var apps = appsCompiler.GetApps(assemblies);

                IInstanceDaemonAppServiceCollection? codeServicesManager = new CodeServicesManager(appServices, apps, new NullLogger<CodeServicesManager>(), null);
                codeServicesManager.ConfigureServices(services);
            });

            return services;
        }

        public static IHostBuilder UseDefaultNetDaemonLogging(this IHostBuilder hostBuilder)
        {
            return hostBuilder.UseSerilog((context, loggerConfiguration) => SerilogConfigurator.Configure(loggerConfiguration, context.HostingEnvironment));
        }

        public static void CleanupNetDaemon()
        {
            Log.CloseAndFlush();
        }

        /// <summary>
        ///     Reads the Home Assistant (hassio) configuration file
        /// </summary>
        [SuppressMessage("", "CA1031")]
        private static void ReadHassioConfig()
        {
            try
            {
                var hassAddOnSettings = JsonSerializer.Deserialize<HassioConfig>(File.ReadAllBytes(HassioConfigPath));

                if (hassAddOnSettings?.LogLevel is not null)
                {
                    Environment.SetEnvironmentVariable("LOGGING__MINIMUMLEVEL", hassAddOnSettings.LogLevel);
                    SerilogConfigurator.SetMinimumLogLevel(hassAddOnSettings.LogLevel);
                }

                if (hassAddOnSettings?.GenerateEntitiesOnStart is not null)
                    Environment.SetEnvironmentVariable("NETDAEMON__GENERATEENTITIES", hassAddOnSettings.GenerateEntitiesOnStart.ToString());

                if (hassAddOnSettings?.LogMessages is not null && hassAddOnSettings.LogMessages == true)
                    Environment.SetEnvironmentVariable("HASSCLIENT_MSGLOGLEVEL", "Default");

                _ = hassAddOnSettings?.AppSource ??
                    throw new NetDaemonNullReferenceException("AppSource cannot be null");

                if (hassAddOnSettings.AppSource.StartsWith("/", true, CultureInfo.InvariantCulture) || hassAddOnSettings.AppSource[1] == ':')
                {
                    // Hard codede path
                    Environment.SetEnvironmentVariable("NETDAEMON__APPSOURCE", hassAddOnSettings.AppSource);
                }
                else
                {
                    // We are in Hassio so hard code the path
                    Environment.SetEnvironmentVariable("NETDAEMON__APPSOURCE", Path.Combine("/config/netdaemon", hassAddOnSettings.AppSource));
                }
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Failed to read the Home Assistant Add-on config");
            }
        }
    }
}