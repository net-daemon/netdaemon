using NetDaemon.Extensions.MqttEntityManager;

namespace NetDaemon.HassClient.Tests.ExtensionsTest.MqttEntityManagerTests.TestHelpers;

internal class MockMqttEntityManagerHelper
{
    public IMessageSender MessageSender { get; }
    public IMessageSubscriber MessageSubscriber { get; }
    public IOptions<MqttConfiguration> Options { get; }

    public string? CapturedTopic { get; private set; }
    public string? CapturedPayload { get; private set; }
    public bool? CapturedRetain { get; private set; }
    public MqttQualityOfServiceLevel? CapturedQos { get; private set; }

    public MockMqttEntityManagerHelper()
    {
        var mockSender = new Mock<IMessageSender>();
        var mockSubscriber = new Mock<IMessageSubscriber>();
        var mockConfig = new Mock<IOptions<MqttConfiguration>>();

        MessageSender = mockSender.Object;
        MessageSubscriber = mockSubscriber.Object;
        Options = mockConfig.Object;

        mockConfig.Setup(f => f.Value).Returns(new MqttConfiguration { DiscoveryPrefix = "HomeAssistant" });

        mockSender.Setup(f => f.SendMessageAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<MqttQualityOfServiceLevel>()))
            .Callback((string topic, string payload, bool retain, MqttQualityOfServiceLevel qos) =>
            {
                CapturedTopic = topic;
                CapturedPayload = payload;
                CapturedRetain = retain;
                CapturedQos = qos;
            })
            .Returns(Task.CompletedTask);
    }
}
