using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NetDaemon;
using Serilog;
using Service;

const string HassioConfigPath = "/data/options.json";

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

/// <summary>
///     Reads the Home Assistant (hassio) configuration file
/// </summary>
/// <returns></returns>
static async Task ReadHassioConfig()
{
    try
    {
        var hassAddOnSettings = await JsonSerializer.DeserializeAsync<HassioConfig>(File.OpenRead(HassioConfigPath)).ConfigureAwait(false);

        if (hassAddOnSettings?.LogLevel is not null)
            SerilogConfigurator.SetMinimumLogLevel(hassAddOnSettings.LogLevel);

        if (hassAddOnSettings?.GenerateEntitiesOnStart is not null)
            Environment.SetEnvironmentVariable("NETDAEMON__GENERATEENTITIES", hassAddOnSettings.GenerateEntitiesOnStart.ToString());

        if (hassAddOnSettings?.LogMessages is not null && hassAddOnSettings.LogMessages == true)
            Environment.SetEnvironmentVariable("HASSCLIENT_MSGLOGLEVEL", "Default");

        if (hassAddOnSettings?.ProjectFolder is not null && string.IsNullOrEmpty(hassAddOnSettings.ProjectFolder) == false)
            Environment.SetEnvironmentVariable("NETDAEMON__PROJECTFOLDER", hassAddOnSettings.ProjectFolder);

        // We are in Hassio so hard code the path
        Environment.SetEnvironmentVariable("NETDAEMON__SOURCEFOLDER", "/config/netdaemon");
    }
    catch (Exception e)
    {
        Log.Fatal(e, "Failed to read the Home Assistant Add-on config");
    }
}