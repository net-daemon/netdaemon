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
                    services.Configure<HomeAssistantSettings>(context.Configuration.GetSection("HomeAssistant"));
                    services.Configure<NetDaemonSettings>(context.Configuration.GetSection("NetDaemon"));
                    services.AddSingleton<IYamlConfig, YamlConfigProvider>();
                    services.AddSingleton<ICodeGenerationHandler, CodeGenerationHandler>();
                    services.AddSingleton<ICodeGenerator, CodeGenerator>();
                    services.AddSingleton<IYamlConfigReader, YamlConfigReader>();
                    services.AddSingleton<IIoWrapper, IoWrapper>();

                    RegisterNetDaemonAssembly(services);
                })
                .ConfigureWebHostDefaults(webbuilder =>
                {
                    webbuilder.UseKestrel(_ => { });
                    webbuilder.UseStartup<ApiStartup>();
                });
        }

        public static IHostBuilder UseDefaultNetDaemonLogging(this IHostBuilder hostBuilder)
        {
            return hostBuilder.UseSerilog((context, loggerConfiguration) => SerilogConfigurator.Configure(loggerConfiguration, context.HostingEnvironment));
        }

        public static void CleanupNetDaemon()
        {
            Log.CloseAndFlush();
        }

        private static void RegisterNetDaemonAssembly(IServiceCollection services)
        {
            if (UseLocalAssemblyLoading())
                services.AddSingleton<IDaemonAppCompiler, LocalDaemonAppCompiler>();
            else
                services.AddSingleton<IDaemonAppCompiler, DaemonAppCompiler>();
        }

        /// <summary>
        ///     Returns true if local loading of assemblies should be preferred.
        ///     This is typically when running in container. When running in dev
        ///     you want the local loading
        /// </summary>
        private static bool UseLocalAssemblyLoading()
        {
            var appSource = Environment.GetEnvironmentVariable("NETDAEMON__APPSOURCE");

            if (string.IsNullOrEmpty(appSource))
                return true;

            return appSource.EndsWith(".csproj", true, CultureInfo.InvariantCulture)
                || appSource.EndsWith(".dll", true, CultureInfo.InvariantCulture);
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