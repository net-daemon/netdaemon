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

        _ = hassAddOnSettings?.AppSource ??
            throw new NullReferenceException("AppSource cannot be null");

        if (hassAddOnSettings.AppSource.StartsWith("/") || hassAddOnSettings.AppSource[1] == ':')
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