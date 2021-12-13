using NetDaemon.HassClient.Debug;

await Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.Configure<HomeAssistantSettings>(context.Configuration.GetSection("HomeAssistant"));
        services.AddHostedService<DebugService>();
        services.AddHomeAssistantClient();
    })
    .Build()
    .RunAsync()
    .ConfigureAwait(false);