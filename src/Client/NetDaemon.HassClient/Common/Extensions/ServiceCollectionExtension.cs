namespace NetDaemon.Client.Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHomeAssistantClient(this IServiceCollection services)
    {
        services.AddSingleton<HomeAssistantClient>()
            .AddSingleton<IHomeAssistantClient>(s => s.GetRequiredService<HomeAssistantClient>())
            .AddSingleton<HomeAssistantRunner>()
            .AddSingleton<IHomeAssistantRunner>(s => s.GetRequiredService<HomeAssistantRunner>())
            .AddSingleton<HomeAssistantApiManager>()
            .AddSingleton<IHomeAssistantApiManager>(s => s.GetRequiredService<HomeAssistantApiManager>())
            .AddTransient(s => s.GetRequiredService<HomeAssistantRunner>().CurrentConnection!)
            .AddTransient(s => s.GetRequiredService<HomeAssistantRunner>().CurrentConnection!.OnHomeAssistantEvent)
            .AddWebSocketFactory()
            .AddPipelineFactory()
            .AddConnectionFactory()
            .AddHttpClientAndFactory();
        return services;
    }

    private static IServiceCollection AddWebSocketFactory(this IServiceCollection services)
    {
        services.AddSingleton<WebSocketClientFactory>();
        services.AddSingleton<IWebSocketClientFactory>(s => s.GetRequiredService<WebSocketClientFactory>());
        return services;
    }

    private static IServiceCollection AddPipelineFactory(this IServiceCollection services)
    {
        services.AddSingleton<WebSocketClientTransportPipelineFactory>();
        services.AddSingleton<IWebSocketClientTransportPipelineFactory>(s =>
            s.GetRequiredService<WebSocketClientTransportPipelineFactory>());
        return services;
    }

    private static IServiceCollection AddConnectionFactory(this IServiceCollection services)
    {
        services.AddSingleton<HomeAssistantConnectionFactory>();
        services.AddSingleton<IHomeAssistantConnectionFactory>(s =>
            s.GetRequiredService<HomeAssistantConnectionFactory>());
        return services;
    }

    private static IServiceCollection AddHttpClientAndFactory(this IServiceCollection services)
    {
        services.AddSingleton(s => s.GetRequiredService<IHttpClientFactory>().CreateClient());
        services.AddHttpClient<IHomeAssistantApiManager, HomeAssistantApiManager>()
            .ConfigurePrimaryHttpMessageHandler(ConfigureHttpMessageHandler);
        return services;
    }

    private static HttpMessageHandler ConfigureHttpMessageHandler(IServiceProvider provider)
    {
        var handler = provider.GetService<HttpMessageHandler>();
        return handler ?? HttpHelper.CreateHttpMessageHandler();
    }
}