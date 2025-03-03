using HiveMQtt.Client;
using HiveMQtt.Client.Events;
using HiveMQtt.Client.Options;
using HiveMQtt.Client.Results;
using HiveMQtt.MQTT5.Types;

namespace NetDaemon.Extensions.MqttEntityManager;

/// <summary>
/// Adaptor pattern to implement a <see cref="IHiveMQClient"/> that can be mocked and its callers tested.
/// Only implements the properties and methods that we utilise in this library - expand this list if you need to.
/// </summary>
/// <remarks>
/// This of course needs to be in step with the IHiveMQClient source, so if we update the HiveMQ Nuget package
/// then the contents here may need to be changed.
/// </remarks>
internal interface IHiveMqClientWrapper : IDisposable
{
    // Properties
    bool IsConnected();

    // Methods
    Task<IConnectResult> ConnectAsync();
    Task<bool> DisconnectAsync(DisconnectOptions options);
    Task<PublishResult> PublishAsync(MQTT5PublishMessage message, CancellationToken cancellationToken = default);

    // Events

    event EventHandler<OnMessageReceivedEventArgs>? OnMessageReceived;
}
