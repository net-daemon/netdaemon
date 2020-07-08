using System.IO;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetDaemon.Service.Infrastructure.Configuration;
using NetDaemon.Service.Support;
using Serilog;

namespace NetDaemon.Service.Extensions
{
    public static class HostBuilderExtensions
    {
        // We preserve the static logger so that we can access it statically and early on in the application lifecycle.
        private const bool PreserveStaticLogger = true;
        private const string AppSettingsFile = "daemon_config.json";
        
        public static IHostBuilder UseNetDaemon(this IHostBuilder builder)
        {
            var configFile = BuildConfigFilePath();

            return builder
                .UseNetDaemonSerilog()
                .ConfigureAppConfiguration(configurationBuilder =>
                {
                    if (File.Exists(configFile))
                        configurationBuilder.AddJsonFile(AppSettingsFile);
                    else
                        CreateSampleConfigFile();

                    configurationBuilder.AddEnvironmentVariables();
                })
                .ConfigureServices(services =>
                {
                    services.AddNetDaemon();
                    services.AddSingleton(provider =>
                    {
                        var configuration = provider.GetService<IConfiguration>();
                        var factory = new HostConfigFactory(configuration);

                        return factory.Create();
                    });
                });
        }

        private static IHostBuilder UseNetDaemonSerilog(this IHostBuilder builder)
        {
            return builder.UseSerilog(
                (context, configuration) => SeriLogConfigurator.Configure(configuration),
                PreserveStaticLogger
            );
        }


        private static string BuildConfigFilePath()
        {
            var path = GetExecutingFolder();
            return Path.Combine(path!, AppSettingsFile);
        }

        private static void CreateSampleConfigFile()
        {
            var path = GetExecutingFolder();
            var exampleFilePath = Path.Combine(path!, "daemon_config_example.json");
            if (File.Exists(exampleFilePath))
                return;

            using (var outputFile = new StreamWriter(exampleFilePath))
            {
                var options = new JsonSerializerOptions {WriteIndented = true, IgnoreNullValues = true};
                outputFile.WriteLine(JsonSerializer.Serialize(new HostConfig(), options));
            }
        }

        private static string? GetExecutingFolder()
        {
            var filenameForExecutingAssembly = Assembly.GetExecutingAssembly().Location;
            return Path.GetDirectoryName(filenameForExecutingAssembly);
        }
    }
}