namespace NetDaemon.Client;

public interface IHomeAssistantConnection : IHomeAssistantApiManager, IAsyncDisposable
{
    /// <summary>
    ///     Subscribe to all events and return a IObservable getting the events
    /// </summary>
    /// <param name="eventType">The type of event to subscribe to, null if all events. If users want non optimized separate subscriptions on all events, provide a "*" as input.</param>
    /// <param name="cancelToken">token to cancel operation</param>
    /// <summary>
    ///     If eventType is null or empty the HomeAssistant connection optimizes and use same
    ///     observable for all subscriptions on all events.
    ///     Users can still provide a "*" as input for eventType and have separated subscriptions
    ///     for all events.
    /// </summary>
    Task<IObservable<HassEvent>> SubscribeToHomeAssistantEventsAsync(string? eventType, CancellationToken cancelToken);
    
    /// <summary>
    ///     Sends a command message to Home Assistant without handling the result
    /// </summary>
    /// <param name="command">Command message to send</param>
    /// <param name="cancelToken">token to cancel operation</param>
    /// <typeparam name="T">Type of command</typeparam>
    Task SendCommandAsync<T>(T command, CancellationToken cancelToken) where T : CommandMessage;

    /// <summary>
    ///     Sends a command message to Home Assistant and return the result
    /// </summary>
    /// <param name="command">Command message to send</param>
    /// <param name="cancelToken">token to cancel operation</param>
    /// <typeparam name="T">Type of command</typeparam>
    /// <typeparam name="TResult">The result of the command</typeparam>
    Task<TResult?> SendCommandAndReturnResponseAsync<T, TResult>(T command, CancellationToken cancelToken)
        where T : CommandMessage;

    /// <summary>
    ///     Sends a command message to Home Assistant without handling the result
    /// </summary>
    /// <param name="command">Command message to send</param>
    /// <param name="cancelToken">token to cancel operation</param>
    /// <typeparam name="T">Type of command</typeparam>
    Task<JsonElement?> SendCommandAndReturnResponseRawAsync<T>(T command, CancellationToken cancelToken)
        where T : CommandMessage;

    Task<HassMessage?> SendCommandAndReturnHassMessageResponseAsync<T>(T command, CancellationToken cancelToken)
        where T : CommandMessage;
    
    /// <summary>
    ///     Return if connection to HomeAssistant is closed for any reason
    /// </summary>
    /// <param name="cancelToken">The token to cancel the processing of events</param>
    Task WaitForConnectionToCloseAsync(CancellationToken cancelToken);
}

/// <summary>
///     Access to all raw Home Assistant messages
/// </summary>
public interface IHomeAssistantHassMessages
{
    /// <summary>
    ///     Allows subscription on all Home Assistant Messages
    /// </summary>
    /// <remark>
    ///     This requires that "ProcessHomeAssistantEvents" task is running
    /// </remark>
    IObservable<HassMessage> OnHassMessage { get; }
}