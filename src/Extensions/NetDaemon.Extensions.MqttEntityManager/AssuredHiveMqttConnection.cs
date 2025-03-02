using HiveMQtt.Client;
using HiveMQtt.Client.Events;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.ReasonCodes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet.Extensions.ManagedClient;

namespace NetDaemon.Extensions.MqttEntityManager;

internal class AssuredHiveMqttConnection(
    ILogger<AssuredHiveMqttConnection> logger,
    IOptions<MqttConfiguration> mqttConfig) : IAssuredMqttConnection, IDisposable, IAsyncDisposable
{
    private readonly SemaphoreSlim _subscriptionSetupLock = new (1);
    private bool _disposed;
    private HiveMQClient? _client;

    public async Task<IManagedMqttClient> GetClientAsync()
    {
        await _subscriptionSetupLock.WaitAsync();
        try
        {
            if (_client == null || !_client.IsConnected())
            {
                await BuildSubscribeAndConnectAsync(mqttConfig.Value);
            }

            return null; // TODO: change interface
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

    private async Task BuildSubscribeAndConnectAsync(MqttConfiguration options)
    {
        if (_client != null)
            await DisconnectAndDisposeClientAsync(); // _client is now disposed

        logger.LogDebug("MQTTClient connecting...");
        _client = BuildClient(options.Host, options.Port, options.UserName, options.Password);
        _client.OnMessageReceived += OnMessageReceived;

        var result = await _client.ConnectAsync().ConfigureAwait(false);
        if (result.ReasonCode == ConnAckReasonCode.Success)
            logger.LogDebug("MQTTClient connected");
        else
            logger.LogError("MQTTClient connection failed: {Reason}", result.ReasonString);
    }

    private async Task DisconnectAndDisposeClientAsync()
    {
        logger.LogDebug("MQTTClient disconnecting...");

        var disconnectOptions = new DisconnectOptions { ReasonCode = DisconnectReasonCode.DisconnectWithWillMessage };
        await _client!.DisconnectAsync(disconnectOptions).ConfigureAwait(false);

        _client.Dispose();
        _client = null;
    }

    private HiveMQClient BuildClient(string host, int port, string? username, string? password)
    {
        logger.LogDebug("MQTTClient connecting to {Host}:{Port}", host, port);

        var options = new HiveMQClientOptionsBuilder()
            .WithBroker(host)
            .WithPort(port)
            .WithAutomaticReconnect(true);

        if (!string.IsNullOrEmpty(username))
            options = options.WithUserName(username);
        if (!string.IsNullOrEmpty(password))
            options = options.WithPassword(password);

        return new HiveMQClient(options.Build());
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

    protected virtual void Dispose(bool disposing)
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
