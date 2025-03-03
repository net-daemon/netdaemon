using HiveMQtt.Client.Results;
using HiveMQtt.MQTT5.ReasonCodes;

namespace NetDaemon.Extensions.MqttEntityManager;

/// <summary>
/// A wrapper for the <see cref="ConnectResult"/> class that provides a more flexible interface
/// by implementing the <see cref="IConnectResult"/> interface (specifically for allowing mocking)
/// </summary>
/// <remarks>
/// This class serves as an adapter for the <see cref="ConnectResult"/> object, allowing it to be
/// used in scenarios where the <see cref="IConnectResult"/> interface is required.
/// </remarks>
internal class ConnectResultWrapper : IConnectResult
{
    public ConnAckReasonCode ReasonCode => _connectResult.ReasonCode;
    public bool SessionPresent => _connectResult.SessionPresent;
    public string? ReasonString  => _connectResult.ReasonString;

    private readonly ConnectResult _connectResult;

    public ConnectResultWrapper(ConnectResult connectResult)
    {
        _connectResult = connectResult;
    }
}
