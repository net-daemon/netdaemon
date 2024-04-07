namespace NetDaemon.Client.HomeAssistant.Extensions;

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
            .SendCommandAndReturnResponseRawAsync
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
    ///     Get all labels from Home Assistant
    /// </summary>
    /// <param name="connection">connected Home Assistant instance</param>
    /// <param name="cancelToken">cancellation token</param>
    public static async Task<IReadOnlyCollection<HassLabel>?> GetLabelsAsync(this IHomeAssistantConnection connection,
        CancellationToken cancelToken)
    {
        return await connection
            .SendCommandAndReturnResponseAsync<SimpleCommand, IReadOnlyCollection<HassLabel>>
                (new SimpleCommand("config/label_registry/list"), cancelToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Get all floors from Home Assistant
    /// </summary>
    /// <param name="connection">connected Home Assistant instance</param>
    /// <param name="cancelToken">cancellation token</param>
    public static async Task<IReadOnlyCollection<HassFloor>?> GetFloorsAsync(this IHomeAssistantConnection connection,
        CancellationToken cancelToken)
    {
        return await connection
            .SendCommandAndReturnResponseAsync<SimpleCommand, IReadOnlyCollection<HassFloor>>
                (new SimpleCommand("config/floor_registry/list"), cancelToken).ConfigureAwait(false);
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
            .SendCommandAsync
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
    ///     Get all configuration from Home Assistant
    /// </summary>
    /// <param name="connection">connected Home Assistant instance</param>
    /// <param name="domain"></param>
    /// <param name="service"></param>
    /// <param name="serviceData"></param>
    /// <param name="serviceTarget">The target of service call</param>
    /// <param name="cancelToken">cancellation token</param>
    public static async Task<HassServiceResult?> CallServiceWithResponseAsync(
        this IHomeAssistantConnection connection,
        string domain,
        string service,
        object? serviceData = null,
        HassTarget? serviceTarget = null,
        CancellationToken? cancelToken = null
    )
    {
        var response = await connection
            .SendCommandAndReturnResponseAsync<CallExecuteScriptCommand, HassServiceResult>
            (
                new()
                {
                   Sequence = new object[]
                   {
                       new
                       {
                           service = $"{domain}.{service}",
                           data = serviceData,
                           target = serviceTarget,
                           response_variable = "service_result"
                       },
                       new
                       {
                           stop = "done",
                           response_variable = "service_result"
                       }
                   }
                },
                cancelToken ?? CancellationToken.None).ConfigureAwait(false);
        return response;
    }

    /// <summary>
    ///     Pings the connected Home Assistant instance and expects a pong
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
                .SendCommandAsync
                    (new SimpleCommand("ping"), cancelToken).ConfigureAwait(false);

            await resultEvent.ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            return false;
        }

        return true;
    }

    public static async Task<HassMessage> SubscribeToTriggerAsync(this IHomeAssistantConnection connection, object trigger, CancellationToken cancelToken)
    {
        var triggerCommand = new SubscribeTriggerCommand(trigger);

        var msg = await connection.SendCommandAndReturnHassMessageResponseAsync
                      (triggerCommand, cancelToken).ConfigureAwait(false) ??
                  throw new NullReferenceException("Unexpected null return from command");
        return msg;
    }

    public static async Task UnsubscribeEventsAsync(this IHomeAssistantConnection connection,
        int id, CancellationToken cancelToken)
    {
        var triggerCommand = new UnsubscribeEventsCommand(id);

        _ = await connection.SendCommandAndReturnHassMessageResponseAsync
                (triggerCommand, cancelToken).ConfigureAwait(false) ??
            throw new NullReferenceException("Unexpected null return from command");
    }
}
