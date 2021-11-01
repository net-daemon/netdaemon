using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using JoySoftware.HomeAssistant.Client;
using Microsoft.Extensions.Configuration;
using NetDaemon.Common.Configuration;
using NetDaemon.Mapping;
using NetDaemon.HassModel.CodeGenerator;

#pragma warning disable CA1303

var configurationRoot = GetConfigurationRoot();
var haSettings = configurationRoot.GetSection("HomeAssistant")?.Get<HomeAssistantSettings>() ?? new HomeAssistantSettings();
var generationSettings = configurationRoot.GetSection("CodeGeneration").Get<CodeGenerationSettings>() ?? new CodeGenerationSettings();

if (args?.Length > 0)
{
    foreach (var arg in args)
    {
        if (arg.ToLower(CultureInfo.InvariantCulture) == "-help")
        {
            Console.WriteLine(@"
    Usage: nd-codegen [options] -ns namespace -o outfile
    Options:
        -host       : Host of the netdaemon instance
        -port       : Port of the NetDaemon instance
        -ssl        : true if NetDaemon instance use ssl
        -token      : A long lived HomeAssistant token

    These settings is valid when installed codegen as global dotnet tool.
            ");
            return 0;
        }
    }
}

Console.WriteLine($"Connecting to Home Assistant at {haSettings.Host}:{haSettings.Port}");
await using var client = new HassClient();

var connected = await client.ConnectAsync(haSettings.Host, haSettings.Port, haSettings.Ssl, haSettings.Token, false).ConfigureAwait(false);

if (!connected)
{
    Console.Error.WriteLine("Failed to Connect to Home Assistant");
    return -1;
}

var services = await client.GetServices().ConfigureAwait(false);
var states = await client.GetAllStates().ConfigureAwait(false);

var code = new Generator().GenerateCodeRx(generationSettings.Namespace, states.Select(s => s.Map()).ToList(), services);
File.WriteAllText(generationSettings.OutputFile, code);

Console.WriteLine("Code Generated successfully!");
Console.WriteLine(Path.GetFullPath(generationSettings.OutputFile));

return 0;

IConfigurationRoot GetConfigurationRoot()
{
    var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
    var builder = new ConfigurationBuilder()
        // default path is the folder of the currently execting root assembley
        .AddJsonFile("appsettings.json", true, true)
        .AddJsonFile($"appsettings.{env}.json", true, true)
        
        // Also look in the current directory which wil typically be the project folder
        // of the users code and have a appsettings.development.json with the correct HA connection settings
        .SetBasePath(Environment.CurrentDirectory)
        .AddJsonFile("appsettings.json", true, true)
        .AddJsonFile($"appsettings.development.json", true, true)
        
        // finally override with Environment vars or commandline
        .AddEnvironmentVariables()
        .AddCommandLine(args, new Dictionary<string, string>()
        {
            ["-o"] = "CodeGeneration:OutputFile",
            ["-ns"] = "CodeGeneration:Namespace",
            ["-host"] = "HomeAssistant:Host",
            ["-port"] = "HomeAssistant:Port",
            ["-ssl"] = "HomeAssistant:Ssl",
            ["-token"] = "HomeAssistant:Token",
        });

    return builder.Build();
}
