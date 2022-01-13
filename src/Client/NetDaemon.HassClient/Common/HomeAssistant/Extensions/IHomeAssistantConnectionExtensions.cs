namespace NetDaemon.Client.Common.HomeAssistant.Extensions;

/// <summary>
///     HomeAssistantConnection extensions
/// </summary>
public static class HomeAssistantConnectionExtensions
{
    /// <summary>
    ///     Get all states from all entities from Home Assistant
    /// </summary>
    /// <param name="connection">connected Home Assistant instance</param>
    /// <param name="cancelToken">cancellation token</param>
    public static async Task<IReadOnlyCollection<HassState>?> GetStatesAsync(this IHomeAssistantConnection connection,
        CancellationToken cancelToken)
    {
        return await connection
            .SendCommandAndReturnResponseAsync<SimpleCommand, IReadOnlyCollection<HassState>>
                (new SimpleCommand("get_states"), cancelToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Get all services from Home Assistant
    /// </summary>
    /// <param name="connection">connected Home Assistant instance</param>
    /// <param name="cancelToken">cancellation token</param>
    public static async Task<JsonElement?> GetServicesAsync(this IHomeAssistantConnection connection,
        CancellationToken cancelToken)
    {
        return await connection
            .SendCommandAndReturnResponseRawAsync<SimpleCommand>
                (new SimpleCommand("get_services"), cancelToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Get all areas from Home Assistant
    /// </summary>
    /// <param name="connection">connected Home Assistant instance</param>
    /// <param name="cancelToken">cancellation token</param>
    public static async Task<IReadOnlyCollection<HassArea>?> GetAreasAsync(this IHomeAssistantConnection connection,
        CancellationToken cancelToken)
    {
        return await connection
            .SendCommandAndReturnResponseAsync<SimpleCommand, IReadOnlyCollection<HassArea>>
                (new SimpleCommand("config/area_registry/list"), cancelToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Get all devices from Home Assistant
    /// </summary>
    /// <param name="connection">connected Home Assistant instance</param>
    /// <param name="cancelToken">cancellation token</param>
    public static async Task<IReadOnlyCollection<HassDevice>?> GetDevicesAsync(this IHomeAssistantConnection connection,
        CancellationToken cancelToken)
    {
        return await connection
            .SendCommandAndReturnResponseAsync<SimpleCommand, IReadOnlyCollection<HassDevice>>
                (new SimpleCommand("config/device_registry/list"), cancelToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Get all entities from Home Assistant
    /// </summary>
    /// <param name="connection">connected Home Assistant instance</param>
    /// <param name="cancelToken">cancellation token</param>
    public static async Task<IReadOnlyCollection<HassEntity>?> GetEntitiesAsync(
        this IHomeAssistantConnection connection, CancellationToken cancelToken)
    {
        return await connection
            .SendCommandAndReturnResponseAsync<SimpleCommand, IReadOnlyCollection<HassEntity>>
                (new SimpleCommand("config/entity_registry/list"), cancelToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Get all configuration from Home Assistant
    /// </summary>
    /// <param name="connection">connected Home Assistant instance</param>
    /// <param name="cancelToken">cancellation token</param>
    public static async Task<HassConfig> GetConfigAsync(this IHomeAssistantConnection connection,
        CancellationToken cancelToken)
    {
        return await connection
                   .SendCommandAndReturnResponseAsync<SimpleCommand, HassConfig>
                       (new SimpleCommand("get_config"), cancelToken).ConfigureAwait(false) ??
               throw new NullReferenceException("Unexpected null return from command");
    }

    /// <summary>
    ///     Get all configuration from Home Assistant
    /// </summary>
    /// <param name="connection">connected Home Assistant instance</param>
    /// <param name="domain"></param>
    /// <param name="service"></param>
    /// <param name="serviceData"></param>
    /// <param name="target">The target of service call</param>
    /// <param name="cancelToken">cancellation token</param>
    public static async Task CallServiceAsync(
        this IHomeAssistantConnection connection,
        string domain,
        string service,
        object? serviceData = null,
        HassTarget? target = null,
        CancellationToken? cancelToken = null
    )
    {
        await connection
            .SendCommandAndReturnResponseAsync<CallServiceCommand, object?>
            (
                new CallServiceCommand
                {
                    Domain = domain,
                    Service = service,
                    ServiceData = serviceData,
                    Target = target
                },
                cancelToken ?? CancellationToken.None).ConfigureAwait(false);
    }

    /// <summary>
    ///     Pings the connected Home Assistant instance and expect a pong
    /// </summary>
    /// <param name="connection">connected Home Assistant instance</param>
    /// <param name="timeout">Timeout to wait for pong back</param>
    /// <param name="cancelToken">cancellation token</param>
    public static async Task<bool> PingAsync(this IHomeAssistantConnection connection, TimeSpan timeout,
        CancellationToken cancelToken)
    {
        var allHassMessages = connection as IHomeAssistantHassMessages
                              ?? throw new InvalidCastException("Unexpected failure to cast");
        try
        {
            var resultEvent = allHassMessages.OnHassMessage
                .Where(n => n.Type == "pong")
                .Timeout(timeout, Observable.Return(default(HassMessage?)))
                .FirstAsync()
                .ToTask(cancelToken);

            await connection
                .SendCommandAsync<SimpleCommand>
                    (new SimpleCommand("ping"), cancelToken).ConfigureAwait(false);

            await resultEvent.ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            return false;
        }

        return true;
    }
}