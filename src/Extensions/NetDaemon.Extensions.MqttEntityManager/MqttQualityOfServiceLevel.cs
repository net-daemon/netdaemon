namespace NetDaemon.Extensions.MqttEntityManager;

/// <summary>
/// Our internal representations of QOS levels rather than exposing the underlying concrete implementations.
/// Ultimately, these are MQTT standards so will always contain these values
/// </summary>
public enum MqttQualityOfServiceLevel
{
    /// <summary>
    /// The message is delivered according to the capabilities of the underlying network.
    /// The message arrives at the receiver either once or not at all.
    ///
    /// <para>
    /// AKA: Fire and forget.
    /// </para>
    /// </summary>
    AtMostOnceDelivery = 0x0,

    /// <summary>
    /// Ensures that the message arrives at the receiver at least once.
    /// </summary>
    AtLeastOnceDelivery = 0x1,

    /// <summary>
    /// The highest Quality of Service level, for use when neither loss nor duplication of messages are acceptable.
    /// </summary>
    ExactlyOnceDelivery = 0x2,
}
