using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NetDaemon;
using Serilog;

namespace Service
{
    internal class Program
    {
        private const string HassioConfigPath = "/data/options.json";

        public static async Task Main(string[] args)
        {
            try
            {
                Log.Logger = SerilogConfigurator.Configure().CreateLogger();

                if (File.Exists(HassioConfigPath))
                    await ReadHassioConfig();

                await Host.CreateDefaultBuilder(args)
                    .UseSerilog(Log.Logger)
                    .UseNetDaemon()
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
            try
            {
                var hassAddOnSettings = await JsonSerializer.DeserializeAsync<HassioConfig>(File.OpenRead(HassioConfigPath)).ConfigureAwait(false);

                if (hassAddOnSettings.LogLevel is object)
                    SerilogConfigurator.SetMinimumLogLevel(hassAddOnSettings.LogLevel);

                if (hassAddOnSettings.GenerateEntitiesOnStart is object)
                    Environment.SetEnvironmentVariable("NetDaemon__GenerateEntities", hassAddOnSettings.GenerateEntitiesOnStart.ToString());

                if (hassAddOnSettings.LogMessages is object && hassAddOnSettings.LogMessages == true)
                    Environment.SetEnvironmentVariable("HASSCLIENT_MSGLOGLEVEL", "Default");

                if (hassAddOnSettings.ProjectFolder is object && string.IsNullOrEmpty(hassAddOnSettings.ProjectFolder) == false)
                    Environment.SetEnvironmentVariable("HASS_RUN_PROJECT_FOLDER", hassAddOnSettings.ProjectFolder);

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