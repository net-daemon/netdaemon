namespace NetDaemon.Client.HomeAssistant.Extensions;

public static class HomeAssistantConnectionHelpersExtensions
{
    public static async Task<InputBooleanHelper?> CreateInputBooleanHelperAsync(
        this IHomeAssistantConnection connection,
        string name, CancellationToken cancelToken)
    {
        cancelToken.ThrowIfCancellationRequested();
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
        cancelToken.ThrowIfCancellationRequested();
        await connection.SendCommandAndReturnResponseAsync<DeleteInputBooleanHelperCommand, object?>(
            new DeleteInputBooleanHelperCommand
            {
                InputBooleanId = inputBooleanId
            }, cancelToken).ConfigureAwait(false);
    }

    public static async Task<IReadOnlyCollection< InputBooleanHelper>> ListInputBooleanHelpersAsync(
        this IHomeAssistantConnection connection, CancellationToken cancelToken)
    {
        cancelToken.ThrowIfCancellationRequested();
        return await connection.SendCommandAndReturnResponseAsync<ListInputBooleanHelperCommand, IReadOnlyCollection< InputBooleanHelper>>(
            new ListInputBooleanHelperCommand(), cancelToken) ?? [];
    }

    public static async Task<InputNumberHelper?> CreateInputNumberHelperAsync(
        this IHomeAssistantConnection connection,
        string name, double min, double max, double? step, double? initial, string? unitOfMeasurement, string? mode, CancellationToken cancelToken)
    {
        cancelToken.ThrowIfCancellationRequested();
        return await connection.SendCommandAndReturnResponseAsync<CreateInputNumberHelperCommand, InputNumberHelper?>(
            new CreateInputNumberHelperCommand
            {
                Name = name,
                Min = min,
                Max = max,
                Step = step,
                Initial = initial,
                UnitOfMeasurement = unitOfMeasurement,
                Mode = mode
            }, cancelToken).ConfigureAwait(false);
    }

    public static async Task DeleteInputNumberHelperAsync(
        this IHomeAssistantConnection connection,
        string inputNumberId, CancellationToken cancelToken)
    {
        cancelToken.ThrowIfCancellationRequested();
        await connection.SendCommandAndReturnResponseAsync<DeleteInputNumberHelperCommand, object?>(
            new DeleteInputNumberHelperCommand
            {
                InputNumberId = inputNumberId
            }, cancelToken).ConfigureAwait(false);
    }

    public static async Task<IReadOnlyCollection< InputNumberHelper>> ListInputNumberHelpersAsync(
        this IHomeAssistantConnection connection, CancellationToken cancelToken)
    {
        cancelToken.ThrowIfCancellationRequested();
        return await connection.SendCommandAndReturnResponseAsync<ListInputNumberHelperCommand, IReadOnlyCollection< InputNumberHelper>>(
            new ListInputNumberHelperCommand(), cancelToken) ?? [];
    }
}
