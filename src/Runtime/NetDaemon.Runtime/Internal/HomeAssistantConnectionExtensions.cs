using NetDaemon.Runtime.Internal.Model;

namespace NetDaemon.Runtime.Internal;

internal static class HomeAssistantConnectionExtensions
{
    public static async Task<InputBooleanHelper?> CreateInputBooleanHelperAsync(
        this IHomeAssistantConnection connection,
        string name, CancellationToken cancelToken)
    {
        return await connection.SendCommandAndReturnResponseAsync<CreateInputBooleanHelperCommand, InputBooleanHelper?>(
            new CreateInputBooleanHelperCommand
            {
                Name = name
            }, cancelToken);
    }
}
