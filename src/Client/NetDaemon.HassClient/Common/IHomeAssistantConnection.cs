using NetDaemon.Client.Common.HomeAssistant.Model;

namespace NetDaemon.Client;

public interface IHomeAssistantConnection : IHomeAssistantApiManager, IAsyncDisposable
{
    /// <summary>
    ///     Allows subscription on all events
    /// </summary>
    /// <remark>
    ///     This requires that "ProcessHomeAssistantEvents" task is running
    /// </remark>
    IObservable<HassEvent> OnHomeAssistantEvent { get; }

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

    Task<IObservable<HassMessage>> SubscribeToTriggerAsync<T>(
        T trigger, CancellationToken cancelToken) where T: TriggerBase;
    
    /// <summary>
    ///     Start processing Home Assistant events
    /// </summary>
    /// <param name="cancelToken">The token to cancel the processing of events</param>
    Task ProcessHomeAssistantEventsAsync(CancellationToken cancelToken);
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