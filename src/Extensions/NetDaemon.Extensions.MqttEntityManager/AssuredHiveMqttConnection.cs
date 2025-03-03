using HiveMQtt.Client;
using HiveMQtt.Client.Events;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.ReasonCodes;
using Microsoft.Extensions.Logging;

namespace NetDaemon.Extensions.MqttEntityManager;

/// <summary>
/// Represents a robust MQTT connection that ensures a client is connected to a HiveMQ broker.
/// This class manages the lifecycle of the MQTT client, including creating the client,
/// connecting it to the broker, and handling disconnection scenarios.
/// </summary>
/// <remarks>
/// This implementation internally uses a semaphore to ensure thread-safe connection management.
/// </remarks>
internal sealed class AssuredHiveMqttConnection(
    ILogger<AssuredHiveMqttConnection> logger,
    IMqttClientFactory clientFactory) : IAssuredMqttConnection, IDisposable, IAsyncDisposable
{
    private readonly SemaphoreSlim _subscriptionSetupLock = new (1);
    private bool _disposed;
    private IHiveMQClient? _client;

    /// <summary>
    /// Retrieves an instance of <see cref="IHiveMQClient"/> connected to the MQTT broker.
    /// If a connection is not already established, this method ensures it creates one, sets up a message receiver,
    /// and connects the client.
    /// <para>Note that this method is thread-safe / re-entrant.</para>
    /// </summary>
    /// <returns>
    /// A connected <see cref="IHiveMQClient"/>.
    /// </returns>
    /// <exception cref="Exception">Thrown when the connection to the MQTT broker fails.</exception>
    public async Task<IHiveMQClient> GetClientAsync()
    {
        await _subscriptionSetupLock.WaitAsync();
        try
        {
            if (_client == null || !_client.IsConnected())
            {
                await BuildSubscribeAndConnectAsync();
            }

            return _client!;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to connect to MQTT broker");
            throw;
        }
        finally
        {
            _subscriptionSetupLock.Release();
        }
    }

    /// <summary>
    /// Establishes a new connection to the MQTT broker, subscribes to specified topics,
    /// and ensures the readiness of the MQTT client for communication.
    /// This method disposes of any existing client and safely initialises a new one.
    /// <para>Handles connection lifecycle, including applying relevant event handlers
    /// for message reception when the client is a <see cref="HiveMQClient"/>.</para>
    /// </summary>
    /// <exception cref="Exception">Thrown if the client cannot connect to the MQTT broker or if subscription fails.</exception>
    private async Task BuildSubscribeAndConnectAsync()
    {
        if (_client != null)
            await DisconnectAndDisposeClientAsync(); // _client is now disposed

        logger.LogDebug("MQTTClient connecting...");
        _client = clientFactory.GetClient();

        // https://github.com/hivemq/hivemq-mqtt-client-dotnet/issues/230
        // Workaround, which can't be unit-tested - only apply event handler if the object
        // is definitely a HiveMqttClient
        if (_client is HiveMQClient hiveClient)
            hiveClient.OnMessageReceived += OnMessageReceived;

        var result = await _client.ConnectAsync().ConfigureAwait(false);
        if (result.ReasonCode == ConnAckReasonCode.Success)
            logger.LogDebug("MQTTClient connected");
        else
            logger.LogError("MQTTClient connection failed: {Reason}", result.ReasonString);
    }

    /// <summary>
    /// Disconnects and disposes the active MQTT client instance, ensuring resource clean-up and disconnection from the broker.
    /// Removes any attached event handlers and releases the client object for garbage collection.
    /// </summary>
    private async Task DisconnectAndDisposeClientAsync()
    {
        logger.LogDebug("MQTTClient disconnecting...");

        // https://github.com/hivemq/hivemq-mqtt-client-dotnet/issues/230
        // Workaround, which can't be unit-tested - only apply event handler if the object
        // is definitely a HiveMqttClient
        if (_client is HiveMQClient hiveClient)
            hiveClient.OnMessageReceived -= OnMessageReceived;

        var disconnectOptions = new DisconnectOptions { ReasonCode = DisconnectReasonCode.DisconnectWithWillMessage };
        await _client!.DisconnectAsync(disconnectOptions).ConfigureAwait(false);

        _client.Dispose();
        _client = null;
    }

    private void OnMessageReceived(object? sender, OnMessageReceivedEventArgs e)
    {
        logger.LogDebug("MQTTClient received a message from {Topic}", e.PublishMessage.Topic);

        // TODO: Call into message subscriber...
    }

    #region IDisposable Support

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _subscriptionSetupLock.Dispose();
        }

        _disposed = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;

        logger.LogDebug("MQTTClient disposing");

        if (_client != null)
        {
            await DisconnectAndDisposeClientAsync().ConfigureAwait(false);
        }

        _subscriptionSetupLock.Dispose();

        GC.SuppressFinalize(this);
    }

    #endregion
}
