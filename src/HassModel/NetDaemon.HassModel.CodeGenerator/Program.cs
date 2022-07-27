﻿using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NetDaemon.Client;
using NetDaemon.Client.Extensions;
using NetDaemon.Client.HomeAssistant.Extensions;
using NetDaemon.Client.HomeAssistant.Model;
using NetDaemon.Client.Settings;

#pragma warning disable CA1303
#pragma warning disable CA2007

var configurationRoot = GetConfigurationRoot();
var haSettings = configurationRoot.GetSection("HomeAssistant")?.Get<HomeAssistantSettings>() ?? new HomeAssistantSettings();
var generationSettings = configurationRoot.GetSection("CodeGeneration").Get<CodeGenerationSettings>() ?? new CodeGenerationSettings();

if (args.Any(arg => arg.ToLower(CultureInfo.InvariantCulture) == "-help"))
{
    Console.WriteLine(@"
    Usage: nd-codegen [options] -ns namespace -o outfile
    Options:
        -f           : Name of folder to output files (folder name only)
        -fpe         : Create separate file per entity (ignores -o option)
        -host        : Host of the netdaemon instance
        -port        : Port of the NetDaemon instance
        -ssl         : true if NetDaemon instance use ssl
        -token       : A long lived HomeAssistant token
        -bypass-cert : Ignore certificate errors (insecure)

    These settings is valid when installed codegen as global dotnet tool.
            ");
    return 0;
}

//This is used as a command line switch rather than a configuration key. There is no key value following it to interpret.
generationSettings.GenerateOneFilePerEntity = args.Any(arg => arg.ToLower(CultureInfo.InvariantCulture) == "-fpe");

var (hassStates, hassServiceDomains) = await GetHaData(haSettings);

var dirPath = string.Empty;

if (!string.IsNullOrWhiteSpace(generationSettings.OutputFolder))
{
    if (!Directory.Exists($"{Directory.GetCurrentDirectory()}\\{generationSettings.OutputFolder}"))
        Directory.CreateDirectory(generationSettings.OutputFolder);

    dirPath = $"{Directory.GetCurrentDirectory()}\\{generationSettings.OutputFolder}\\";
}

if (generationSettings.GenerateOneFilePerEntity)
{
    Console.WriteLine("Generating separate file per entity");

    var entities = Generator.GenerateCodePerEntity(generationSettings, hassStates, hassServiceDomains).ToList();

    entities.ForEach(entity => File.WriteAllText($"{dirPath}{entity.GetClassName()}.cs", entity.ToFullString()));

    Console.WriteLine($"Generated {entities.Count} files.");
    Console.WriteLine(dirPath);
}
else
{
    Console.WriteLine("Generating single file for all entities");
    var code = Generator.GenerateCode(generationSettings, hassStates, hassServiceDomains);
    File.WriteAllText($"{dirPath}{generationSettings.OutputFile}", code);

    Console.WriteLine($"{dirPath}{generationSettings.OutputFile}");
}

Console.WriteLine();
Console.WriteLine("Code Generated successfully!");

return 0;

IConfigurationRoot GetConfigurationRoot()
{
    var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
    var builder = new ConfigurationBuilder()
        // default path is the folder of the currently executing root assembly
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
            ["-f"] = "CodeGeneration:OutputFolder",
            ["-ns"] = "CodeGeneration:Namespace",
            ["-host"] = "HomeAssistant:Host",
            ["-port"] = "HomeAssistant:Port",
            ["-ssl"] = "HomeAssistant:Ssl",
            ["-token"] = "HomeAssistant:Token",
            ["-bypass-cert"] = "HomeAssistant:InsecureBypassCertificateErrors",
        });

    return builder.Build();
}

async Task<(IReadOnlyCollection<HassState> states, IReadOnlyCollection<HassServiceDomain> serviceDmains)> GetHaData(HomeAssistantSettings homeAssistantSettings)
{
    Console.WriteLine($"Connecting to Home Assistant at {homeAssistantSettings.Host}:{homeAssistantSettings.Port}");

    var serviceCollection = new ServiceCollection();
    serviceCollection.AddSingleton(Options.Create(homeAssistantSettings));
    serviceCollection.AddHomeAssistantClient();
    var client = serviceCollection.BuildServiceProvider().GetRequiredService<IHomeAssistantClient>();

    await using var connection = await client.ConnectAsync(homeAssistantSettings.Host, homeAssistantSettings.Port, homeAssistantSettings.Ssl, homeAssistantSettings.Token, CancellationToken.None).ConfigureAwait(false);

    var services = await connection.GetServicesAsync(CancellationToken.None).ConfigureAwait(false);
    var serviceDmains = services!.Value.ToServicesResult();

    var states = await connection.GetStatesAsync(CancellationToken.None).ConfigureAwait(false);

    return (states!, serviceDmains);
}