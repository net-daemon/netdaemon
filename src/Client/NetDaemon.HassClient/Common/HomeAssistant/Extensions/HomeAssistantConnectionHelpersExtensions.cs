namespace NetDaemon.Client.HomeAssistant.Extensions;

public static class HomeAssistantConnectionHelpersExtensions
{
    public static async Task<InputBooleanHelper?> CreateInputBooleanHelperAsync(
        this IHomeAssistantConnection connection,
        string name, CancellationToken cancelToken)
    {
        return await connection.SendCommandAndReturnResponseAsync<CreateInputBooleanHelperCommand, InputBooleanHelper?>(
            new CreateInputBooleanHelperCommand
            {
                Name = name
            }, cancelToken).ConfigureAwait(false);
    }

    public static async Task DeleteInputBooleanHelperAsync(
        this IHomeAssistantConnection connection,
        string inputBooleanId, CancellationToken cancelToken)
    {
        await connection.SendCommandAndReturnResponseAsync<DeleteInputBooleanHelperCommand, object?>(
            new DeleteInputBooleanHelperCommand
            {
                InputBooleanId = inputBooleanId
            }, cancelToken).ConfigureAwait(false);
    }

    public static async Task<IReadOnlyCollection< InputBooleanHelper>> ListInputBooleanHelpersAsync(
        this IHomeAssistantConnection connection, CancellationToken cancelToken)
    {
        return await connection.SendCommandAndReturnResponseAsync<ListInputBooleanHelperCommand, IReadOnlyCollection< InputBooleanHelper>>(
            new ListInputBooleanHelperCommand(), cancelToken) ?? Array.Empty<InputBooleanHelper>();
    }
}
