using HiveMQtt.MQTT5.Types;
using NetDaemon.Extensions.MqttEntityManager;

namespace NetDaemon.HassClient.Tests.ExtensionsTest.MqttEntityManagerTests;

public class MessageSenderTests
{
    /// <summary>
    /// Helper class to set up the mock AssuredMqttConnection and expose its captured message
    /// </summary>
    private class MockAssuredMqttConnectionSetup
    {
        public MQTT5PublishMessage? CapturedMessage { get; private set; }
        public IAssuredMqttConnection AssuredMqttConnection { get; private set; }

        public MockAssuredMqttConnectionSetup()
        {
            var mqttClient = new Mock<IHiveMqClientWrapper>();
            var assuredConnection = new Mock<IAssuredMqttConnection>();

            assuredConnection.Setup(f => f.GetClientAsync())
                .Returns(Task.FromResult(mqttClient.Object));

            CapturedMessage = null!;
            mqttClient.Setup(f => f.PublishAsync(
                    It.IsAny<MQTT5PublishMessage>(), It.IsAny<CancellationToken>()))
                .Callback((MQTT5PublishMessage message, CancellationToken token) =>
                {
                    CapturedMessage = message;
                });

            AssuredMqttConnection = assuredConnection.Object;
        }
    }

    [Fact]
    public async Task TopicAndPayloadAreSet()
    {
        var logger = new Mock<ILogger<MessageSender>>().Object;
        var mocks = new MockAssuredMqttConnectionSetup();

        var messageSender = new MessageSender(logger, mocks.AssuredMqttConnection);

        await messageSender.SendMessageAsync("topic", "payload", true, MqttQualityOfServiceLevel.AtMostOnceDelivery);

        mocks.CapturedMessage.Should().NotBeNull();
        mocks.CapturedMessage.Topic.Should().Be("topic");
        mocks.CapturedMessage.PayloadAsString.Should().Be("payload");
    }

    [Fact]
    public async Task RetainFlagCanBeSetTrue()
    {
        var logger = new Mock<ILogger<MessageSender>>().Object;
        var mocks = new MockAssuredMqttConnectionSetup();

        var messageSender = new MessageSender(logger, mocks.AssuredMqttConnection);

        await messageSender.SendMessageAsync("topic", "payload", true, MqttQualityOfServiceLevel.AtMostOnceDelivery);

        mocks.CapturedMessage.Should().NotBeNull();
        mocks.CapturedMessage.Retain.Should().BeTrue();
    }

    [Fact]
    public async Task RetainFlagCanBeSetFalse()
    {
        var logger = new Mock<ILogger<MessageSender>>().Object;
        var mocks = new MockAssuredMqttConnectionSetup();

        var messageSender = new MessageSender(logger, mocks.AssuredMqttConnection);

        await messageSender.SendMessageAsync("topic", "payload", false, MqttQualityOfServiceLevel.AtMostOnceDelivery);

        mocks.CapturedMessage.Should().NotBeNull();
        mocks.CapturedMessage.Retain.Should().BeFalse();
    }

    [Fact]
    public async Task CanSetQosLevel()
    {
        var logger = new Mock<ILogger<MessageSender>>().Object;
        var mocks = new MockAssuredMqttConnectionSetup();

        var messageSender = new MessageSender(logger, mocks.AssuredMqttConnection);

        await messageSender.SendMessageAsync("topic", "payload", true, MqttQualityOfServiceLevel.ExactlyOnceDelivery);

        mocks.CapturedMessage.Should().NotBeNull();
        mocks.CapturedMessage.QoS.Should().Be(QualityOfService.ExactlyOnceDelivery);    // Note mapping from public enum to internal enum
    }

    [Fact]
    public async Task CanSetPersist()
    {
        var logger = new Mock<ILogger<MessageSender>>().Object;
        var mocks = new MockAssuredMqttConnectionSetup();

        var messageSender = new MessageSender(logger, mocks.AssuredMqttConnection);

        await messageSender.SendMessageAsync("topic", "payload", true, MqttQualityOfServiceLevel.ExactlyOnceDelivery);

        mocks.CapturedMessage.Should().NotBeNull();
        mocks.CapturedMessage.Retain.Should().BeTrue();
    }

    [Fact]
    public async Task CanUnsetPersist()
    {
        var logger = new Mock<ILogger<MessageSender>>().Object;
        var mocks = new MockAssuredMqttConnectionSetup();

        var messageSender = new MessageSender(logger, mocks.AssuredMqttConnection);

        await messageSender.SendMessageAsync("topic", "payload", false, MqttQualityOfServiceLevel.ExactlyOnceDelivery);

        mocks.CapturedMessage.Should().NotBeNull();
        mocks.CapturedMessage.Retain.Should().BeFalse();
    }
}
