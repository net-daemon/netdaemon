
using NetDaemon.AppModel;
using NetDaemon.Client;
using NetDaemon.Client.HomeAssistant.Extensions;

namespace Apps;

[NetDaemonApp]
public sealed class HelperApp(IHomeAssistantConnection conn) : IAsyncInitializable
{
    private readonly IHomeAssistantConnection _conn = conn;

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await _conn.CreateInputNumberHelperAsync(
                name: "MyNumberHelper",
                min: 0,
                max: 100.0,
                step: 1.2,
                initial: 10.0,
                unitOfMeasurement: "ml",
                mode: "slider",
                cancellationToken
                );
    }
}
