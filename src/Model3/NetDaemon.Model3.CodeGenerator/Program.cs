using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JoySoftware.HomeAssistant.Client;
using Microsoft.Extensions.Configuration;
using NetDaemon.Common.Configuration;
using NetDaemon.Mapping;
using NetDaemon.Model3.CodeGenerator;

#pragma warning disable CA1303

var configurationRoot = GetConfigurationRoot();
var haSettings = configurationRoot.GetSection("HomeAssistant").Get<HomeAssistantSettings>();
var generationSettings = configurationRoot.GetSection("CodeGeneration").Get<CodeGenerationSettings>();

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
return 0;

IConfigurationRoot GetConfigurationRoot()
{
    var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
    var builder = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", true, true)
        .AddJsonFile($"appsettings.{env}.json", true, true)
        .AddEnvironmentVariables()
        .AddCommandLine(args, new Dictionary<string, string>()
        {
            ["-o"] = "CodeGeneration:OutputFile",
            ["-ns"] = "CodeGeneration:Namespace",
            ["-host"] = "HomeAssistant:Host",
            ["-port"] = "HomeAssistant:Post",
            ["-ssl"] = "HomeAssistant:Ssl",
            ["-token"] = "HomeAssistant:Token",
        });

    return builder.Build();
}