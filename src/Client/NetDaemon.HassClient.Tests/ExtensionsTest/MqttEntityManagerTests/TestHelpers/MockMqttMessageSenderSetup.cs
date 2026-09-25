using MQTTnet;
using MQTTnet.Packets;
using NetDaemon.Extensions.MqttEntityManager;

namespace NetDaemon.HassClient.Tests.ExtensionsTest.MqttEntityManagerTests.TestHelpers;

/// <summary>
/// Test helper that captures MQTT messages sent by the entity manager.
/// </summary>
internal sealed class MockMqttMessageSenderSetup
{
    public FakeAssuredMqttConnection Connection { get; } = new();
    public MessageSender MessageSender { get; private set; } = null!;
    public MqttApplicationMessage LastPublishedMessage => Connection.LastPublishedMessage;

    public MockMqttMessageSenderSetup()
    {
        SetupMessageSender();
    }

    private void SetupMessageSender()
    {
        var logger = new Mock<ILogger<MessageSender>>().Object;
        MessageSender = new MessageSender(logger, Connection);
    }
}

internal sealed class FakeAssuredMqttConnection : IAssuredMqttConnection
{
    private readonly List<MqttTopicFilter> _subscriptions = [];

    public event Func<MqttApplicationMessageReceivedEventArgs, Task>? ApplicationMessageReceivedAsync;

    public IReadOnlyList<MqttTopicFilter> Subscriptions => _subscriptions;

    public MqttApplicationMessage LastPublishedMessage { get; private set; } = null!;

    public Task PublishAsync(MqttApplicationMessage message)
    {
        LastPublishedMessage = message;
        return Task.CompletedTask;
    }

    public Task SubscribeAsync(MqttTopicFilter topicFilter)
    {
        _subscriptions.Add(topicFilter);
        return Task.CompletedTask;
    }

    public Task ReceiveAsync(string topic, string payload)
    {
        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(Encoding.UTF8.GetBytes(payload))
            .Build();

        var publishPacket = new MqttPublishPacket
        {
            Topic = topic,
            PayloadSegment = new ArraySegment<byte>(Encoding.UTF8.GetBytes(payload))
        };

        var eventArgs = new MqttApplicationMessageReceivedEventArgs(
            "test",
            message,
            publishPacket,
            (_, _) => Task.CompletedTask);

        return ApplicationMessageReceivedAsync?.Invoke(eventArgs) ?? Task.CompletedTask;
    }
}
