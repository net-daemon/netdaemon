using HiveMQtt.MQTT5.ReasonCodes;

namespace NetDaemon.Extensions.MqttEntityManager;

/// <summary>
/// Represents the result of a connection attempt to an MQTT broker.
/// </summary>
/// <remarks>
/// This is provided because the HiveMQ ConnectResult does not implement an interface and does not have a
/// public constructor. There are many more properties in the referenced ConnectResult so, if we need them,
/// add them to this file.
/// </remarks>
internal interface IConnectResult
{
    ConnAckReasonCode ReasonCode { get; }
    string? ReasonString { get; }
    bool SessionPresent { get; }
}
