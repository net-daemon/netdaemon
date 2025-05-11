using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Client;
using NetDaemon.Client.Extensions;
using NetDaemon.Client.Settings;
using NetDaemon.HassModel;
using NetDaemon.Runtime.Internal;

var collection = new ServiceCollection();

collection.Configure<HomeAssistantSettings>(s =>
    {
        s.Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiI4MDhlZjQ3NWRlOTU0YWJmYTYwNTRkZDc2YzRkZmJjNiIsImlhdCI6MTc0NjkxMTkxOSwiZXhwIjoyMDYyMjcxOTE5fQ.KSwYw1IER965EUcN2_7XPPgVikeIli-mTH8XreveWvA";
        s.Host = "localhost";
        s.Port = 8123;
        s.Ssl = false;
    });

collection.AddHomeAssistantClient();
collection.AddScopedHaContext();
collection.AddTransient<NetDaemonRuntimeLight>();

var serviceProvider = collection.BuildServiceProvider().CreateScope().ServiceProvider;

var runner = serviceProvider.GetRequiredService<IHomeAssistantRunner>();


var runtime = serviceProvider.GetRequiredService<NetDaemonRuntimeLight>();
runtime.Start(CancellationToken.None);
await runtime.WaitForInitializationAsync();


//await StartAsync(runner, CancellationToken.None).ConfigureAwait(false);

//await serviceProvider.GetRequiredService<ICacheManager>().InitializeAsync(CancellationToken.None);
var haContext = serviceProvider.GetRequiredService<IHaContext>();

var state = haContext.GetState("sun.sun")?.State;
Console.WriteLine(state);

haContext.Entity("input_button.test_button").StateAllChanges().Subscribe(s => Console.WriteLine($"Pressed {s.New?.State}"));

Console.ReadLine();


async Task StartAsync(IHomeAssistantRunner homeAssistantRunner, CancellationToken stoppingToken)
{
    var connectedTask = homeAssistantRunner.OnConnect.Take(1).ToTask();

    homeAssistantRunner.RunAsync(
        "localhost",
        8123,
        false,
        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiI4MDhlZjQ3NWRlOTU0YWJmYTYwNTRkZDc2YzRkZmJjNiIsImlhdCI6MTc0NjkxMTkxOSwiZXhwIjoyMDYyMjcxOTE5fQ.KSwYw1IER965EUcN2_7XPPgVikeIli-mTH8XreveWvA",
        "api/websocket",
        TimeSpan.FromSeconds(30),
        stoppingToken);

    await connectedTask.ConfigureAwait(false);
}
