using System.Globalization;
using Microsoft.Extensions.Configuration;
using NetDaemon.Client.Settings;

#pragma warning disable CA1303

var configurationRoot = GetConfigurationRoot();
var haSettings = configurationRoot.GetSection("HomeAssistant").Get<HomeAssistantSettings>() ?? new HomeAssistantSettings();
var generationSettings = configurationRoot.GetSection("CodeGeneration").Get<CodeGenerationSettings>() ?? new CodeGenerationSettings();

if (args.Any(arg => arg.ToLower(CultureInfo.InvariantCulture) == "-help"))
{
    ShowUsage();
    return 0;
}

//This is used as a command line switch rather than a configuration key. There is no key value following it to interpret.
generationSettings.GenerateOneFilePerEntity = args.Any(arg => arg.ToLower(CultureInfo.InvariantCulture) == "-fpe");

var controller = new Controller(generationSettings, haSettings);
await controller.RunAsync().ConfigureAwait(false);

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

        // Also look in the current directory which will typically be the project folder
        // of the users code and have a appsettings.development.json with the correct HA connection settings
        .SetBasePath(Environment.CurrentDirectory)
        .AddJsonFile("appsettings.json", true, true)
        .AddJsonFile("appsettings.development.json", true, true)

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

void ShowUsage()
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
}