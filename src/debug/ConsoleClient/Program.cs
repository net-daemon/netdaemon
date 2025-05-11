using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Client;
using NetDaemon.Client.Extensions;
using NetDaemon.HassModel;

var haContext = await CreateHaContext(new HaSettings());

var state = haContext.GetState("sun.sun")?.State;
Console.WriteLine(state);

haContext.Entity("input_button.test_button").StateAllChanges().Subscribe(s => Console.WriteLine($"Pressed {s.New?.State}"));


while (!Console.KeyAvailable)
{
    await Task.Delay(1000);
}

async Task<IHaContext> CreateHaContext(HaSettings haSettings)
{
    var collection = new ServiceCollection();
    collection.AddHomeAssistantClient();
    collection.AddScopedHaContext();

    var serviceProvider = collection.BuildServiceProvider().CreateScope().ServiceProvider;

    var runner = serviceProvider.GetRequiredService<IHomeAssistantRunner>();

    var connectedTask = runner.OnConnect.Take(1).ToTask();

    var _ = runner.RunAsync(haSettings.Host, haSettings.Port, haSettings.Ssl, haSettings.Token,
        "api/websocket",
        TimeSpan.FromSeconds(30),
        CancellationToken.None);

    await connectedTask.ConfigureAwait(false);

    var cacheManager = serviceProvider.GetRequiredService<ICacheManager>();
    await cacheManager.InitializeAsync(CancellationToken.None);

    return serviceProvider.GetRequiredService<IHaContext>();
}
