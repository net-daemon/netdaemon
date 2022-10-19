using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NetDaemon.Client;
using NetDaemon.Client.Extensions;
using NetDaemon.Client.HomeAssistant.Extensions;
using NetDaemon.Client.HomeAssistant.Model;
using NetDaemon.Client.Settings;

namespace NetDaemon.HassModel.CodeGenerator;

internal static class HaRepositry
{
    public record HaData(IReadOnlyCollection<HassState> states, JsonElement? servicesMetaData);

    public static async Task<HaData> GetHaData(HomeAssistantSettings homeAssistantSettings)
    {
        Console.WriteLine($"Connecting to Home Assistant at {homeAssistantSettings.Host}:{homeAssistantSettings.Port}");

        var client = GetHaClient(homeAssistantSettings);

        var connection = await client.ConnectAsync(
            homeAssistantSettings.Host, 
            homeAssistantSettings.Port,
            homeAssistantSettings.Ssl,
            homeAssistantSettings.Token,
            CancellationToken.None).ConfigureAwait(false);
        
        await using (connection.ConfigureAwait(false))
        {
            var services = await connection.GetServicesAsync(CancellationToken.None).ConfigureAwait(false);
            var states = await connection.GetStatesAsync(CancellationToken.None).ConfigureAwait(false);

            return new HaData(states!, services);
        }
    }

    private static IHomeAssistantClient GetHaClient(HomeAssistantSettings homeAssistantSettings)
    {
        // We need some trickery to get the HomeAssistantClient because it is currently
        // only available via DI
        
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(Options.Create(homeAssistantSettings));
        serviceCollection.AddHomeAssistantClient();
        return serviceCollection.BuildServiceProvider().GetRequiredService<IHomeAssistantClient>();
    }
}