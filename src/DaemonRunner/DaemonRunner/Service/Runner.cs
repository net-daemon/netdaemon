using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NetDaemon.Service.Extensions;
using NetDaemon.Service.Support;
using Serilog;

namespace NetDaemon.Service
{
    public static class Runner
    {
        private const string _hassioConfigPath = "/data/options.json";

        public static IHostBuilder CreateHostBuilder(string[] args) => 
            Host.CreateDefaultBuilder(args)
                .UseNetDaemon();
        
        public static async Task Run(string[] args)
        {
            try
            {
                Log.Logger = SeriLogConfigurator.GetConfiguration().CreateLogger();

                if (File.Exists(_hassioConfigPath))
                {
                    try
                    {
                        var hassAddOnSettings = await JsonSerializer.DeserializeAsync<HassioConfig>(
                            File.OpenRead(_hassioConfigPath)).ConfigureAwait(false);
                        if (hassAddOnSettings.LogLevel is object)
                        {
                            SeriLogConfigurator.SetMinimumLogLevel(hassAddOnSettings.LogLevel);
                        }
                        if (hassAddOnSettings.GenerateEntitiesOnStart is object)
                        {
                            Environment.SetEnvironmentVariable("HASS_GEN_ENTITIES", hassAddOnSettings.GenerateEntitiesOnStart.ToString());
                        }
                        if (hassAddOnSettings.LogMessages is object && hassAddOnSettings.LogMessages == true)
                        {
                            Environment.SetEnvironmentVariable("HASSCLIENT_MSGLOGLEVEL", "Default");
                        }
                        if (hassAddOnSettings.ProjectFolder is object &&
                            string.IsNullOrEmpty(hassAddOnSettings.ProjectFolder) == false)
                        {
                            Environment.SetEnvironmentVariable("HASS_RUN_PROJECT_FOLDER", hassAddOnSettings.ProjectFolder);
                        }

                        // We are in Hassio so hard code the path
                        Environment.SetEnvironmentVariable("HASS_DAEMONAPPFOLDER", "/config/netdaemon");
                    }
                    catch (Exception e)
                    {
                        Log.Fatal(e, "Failed to read the Home Assistant Add-on config");
                    }
                }
                else
                {
                    var envLogLevel = Environment.GetEnvironmentVariable("HASS_LOG_LEVEL");
                    if (!string.IsNullOrEmpty(envLogLevel))
                    {
                        SeriLogConfigurator.SetMinimumLogLevel(envLogLevel);
                    }
                }

                await CreateHostBuilder(args).Build().RunAsync();
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
    }
}