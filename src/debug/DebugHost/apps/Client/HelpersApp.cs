
using NetDaemon.AppModel;
using NetDaemon.Client;
using NetDaemon.Client.HomeAssistant.Extensions;

namespace Apps;

[NetDaemonApp]
public sealed class HelperApp(IHomeAssistantConnection conn) : IAsyncInitializable
{
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await conn.CreateInputNumberHelperAsync(
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
