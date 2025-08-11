namespace NetDaemon.HassModel;

/// <summary>
/// Factory class to create an instance of <see cref="IHaContext"/> for use in console applications or scripts.
/// </summary>
public static class HaContextFactory
{
    /// <summary>
    /// Creates a new instance of <see cref="IHaContext"/> using the provided WebSocket URL and token.
    /// </summary>
    /// <remarks>Do not use this in NetDaemon apps. Apps should use UseNetDaemonRuntime and resolve IHaContext via dependency injection</remarks>
    /// <param name="homeAssistantWebsocketUrl">The Websocket Url to HomeAssistant, eg: ws://localhost:8123/api/websocket</param>
    /// <param name="token">The long lived accestoken for Home Assitant</param>
    /// <returns>An instance of IHaContext</returns>
    public static async Task<IHaContext> CreateAsync(string homeAssistantWebsocketUrl, string token)
    {
        var connection = await HomeAssistantClientConnector.ConnectClientAsync(homeAssistantWebsocketUrl, token, CancellationToken.None);

        var collection = new ServiceCollection();
        collection.AddLogging(builder => builder.AddConsole());
        collection.AddSingleton(connection);
        collection.AddSingleton<IHomeAssistantConnectionProvider>(new ConnectionProvider(connection));

        collection.AddScopedHaContext();
        var serviceProvider = collection.BuildServiceProvider().CreateScope().ServiceProvider;

        var cacheManager = serviceProvider.GetRequiredService<ICacheManager>();
        await cacheManager.InitializeAsync(connection, CancellationToken.None);

        return serviceProvider.GetRequiredService<IHaContext>();
    }

    /// <summary>
    /// ConnectionProvider to provide the current connection without reconnect logic
    /// </summary>
    class ConnectionProvider(IHomeAssistantConnection connection) : IHomeAssistantConnectionProvider
    {
        public IHomeAssistantConnection CurrentConnection => connection;
    }
}
