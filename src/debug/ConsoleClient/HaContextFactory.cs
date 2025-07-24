using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Client;
using NetDaemon.Client.Settings;
using NetDaemon.HassModel;

public static class HaContextFactory
{
    public static async Task<IHaContext> CreateHaContextAsync()
    {
        var connection = await CreateConnection();
        return await CreateHaContextAsync(connection);
    }

    public static async Task<IHaContext> CreateHaContextAsync(IHomeAssistantConnection connection)
    {
        var collection = new ServiceCollection();
        collection.AddLogging();
        collection.AddSingleton(connection);
        collection.AddScopedHaContext();
        var serviceProvider = collection.BuildServiceProvider().CreateScope().ServiceProvider;

        var cacheManager = serviceProvider.GetRequiredService<ICacheManager>();
        await cacheManager.InitializeAsync(connection, CancellationToken.None);

        return serviceProvider.GetRequiredService<IHaContext>();
    }

    private static async Task<IHomeAssistantConnection> CreateConnection()
    {
        var config = GetConfigurationRoot([]);

        var homeassistantSettings = new HomeAssistantSettings();

        config.GetSection("HomeAssistant").Bind(homeassistantSettings);

        var connection = await HomeAssistantClientConnector.ConnectClientAsync(
            host: homeassistantSettings.Host, homeassistantSettings.Port, homeassistantSettings.Ssl, homeassistantSettings.Token,
            "api/websocket",
            CancellationToken.None);
        return connection;
    }

    private static IConfigurationRoot GetConfigurationRoot(string[] args)
    {
        var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        var builder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{env}.json", optional: true)
            .AddUserSecrets(typeof(HaContextFactory).Assembly, optional: true)

            // finally override with Environment vars or commandline
            .AddEnvironmentVariables()
            .AddCommandLine(args, new Dictionary<string, string>
            {
                ["-host"] = "HomeAssistant:Host",
                ["-port"] = "HomeAssistant:Port",
                ["-ssl"] = "HomeAssistant:Ssl",
                ["-token"] = "HomeAssistant:Token",
                ["-bypass-cert"] = "HomeAssistant:InsecureBypassCertificateErrors",
            });

        return builder.Build();
    }
}
