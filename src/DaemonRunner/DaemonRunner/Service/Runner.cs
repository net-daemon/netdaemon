using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetDaemon.Service.Configuration;
using NetDaemon.Service.Extensions;
using NetDaemon.Service.Support;
using Serilog;

namespace NetDaemon.Service
{
    public static class Runner
    {
        private const string _hassioConfigPath = "/data/options.json";

        public static async Task Run(string[] args)
        {
            try
            {
                Log.Logger = SerilogConfigurator.Configure().CreateLogger();

                if (File.Exists(_hassioConfigPath))
                    await ReadHassioConfig();

                await Host.CreateDefaultBuilder(args)
                    .UseSerilog(Log.Logger)
                    .ConfigureServices((context, services) =>
                    {
                        services.Configure<HomeAssistantSettings>(context.Configuration.GetSection("HomeAssistant"));
                        services.Configure<NetDaemonSettings>(context.Configuration.GetSection("NetDaemon"));

                        services.AddNetDaemon();
                    })
                    .Build()
                    .RunAsync();

            }
            catch (Exception e)
            {
                Log.Fatal(e, "Failed to start host...");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static async Task ReadHassioConfig()
        {
            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-3.1#environment-variables
            try
            {
                var hassAddOnSettings = await JsonSerializer.DeserializeAsync<HassioConfig>(File.OpenRead(_hassioConfigPath)).ConfigureAwait(false);
                
                if (hassAddOnSettings.LogLevel is object)
                    SerilogConfigurator.SetMinimumLogLevel(hassAddOnSettings.LogLevel);

                if (hassAddOnSettings.GenerateEntitiesOnStart is object)
                    Environment.SetEnvironmentVariable("NetDaemon__GenerateEntities", hassAddOnSettings.GenerateEntitiesOnStart.ToString());

                // I couldn't find any usages.
                //if (hassAddOnSettings.LogMessages is object && hassAddOnSettings.LogMessages == true)
                //    Environment.SetEnvironmentVariable("HASSCLIENT_MSGLOGLEVEL", "Default");

                //if (hassAddOnSettings.ProjectFolder is object && string.IsNullOrEmpty(hassAddOnSettings.ProjectFolder) == false)
                //    Environment.SetEnvironmentVariable("HASS_RUN_PROJECT_FOLDER", hassAddOnSettings.ProjectFolder);

                // We are in Hassio so hard code the path
                Environment.SetEnvironmentVariable("NetDaemon__AppFolder", "/config/netdaemon");
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Failed to read the Home Assistant Add-on config");
            }
        }
    }
}