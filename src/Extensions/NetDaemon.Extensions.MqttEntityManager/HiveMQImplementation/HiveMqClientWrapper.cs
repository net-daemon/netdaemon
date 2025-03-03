using HiveMQtt.Client;
using HiveMQtt.Client.Events;
using HiveMQtt.Client.Options;
using HiveMQtt.Client.Results;
using HiveMQtt.MQTT5.Types;

namespace NetDaemon.Extensions.MqttEntityManager;

/// <summary>
/// A wrapper class for the HiveMQ .NET client providing simplified and testable access to HiveMQ operations.
/// This class implements the <see cref="IHiveMqClientWrapper"/> interface and manages the lifecycle
/// and communication of the underlying <see cref="IHiveMQClient"/> instance.
/// </summary>
internal class HiveMqClientWrapper : IHiveMqClientWrapper
{
    private readonly IHiveMQClient _innerClient;

    public bool IsConnected() => _innerClient.IsConnected();

    public Task<bool> DisconnectAsync(DisconnectOptions options) => _innerClient.DisconnectAsync(options);

    public void Dispose() => _innerClient.Dispose();

    public Task<PublishResult> PublishAsync(MQTT5PublishMessage message, CancellationToken cancellationToken = default) =>
        _innerClient.PublishAsync(message, cancellationToken);

    public event EventHandler<OnMessageReceivedEventArgs>? OnMessageReceived;

    public HiveMqClientWrapper(IHiveMQClient innerClient)
    {
        _innerClient = innerClient;
        _innerClient.OnMessageReceived += (sender, args) => OnMessageReceived?.Invoke(sender, args);
    }

    public async Task<IConnectResult> ConnectAsync()
    {
        var result = await _innerClient.ConnectAsync();
        return new ConnectResultWrapper(result);
    }
}
