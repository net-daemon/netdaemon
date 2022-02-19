using MQTTnet.Client.Publishing;
using MQTTnet.Protocol;
using NetDaemon.Extensions.MqttEntityManager.Exceptions;
using NetDaemon.HassClient.Tests.ExtensionsTest.MqttEntityManagerTests.TestHelpers;

namespace NetDaemon.HassClient.Tests.ExtensionsTest.MqttEntityManagerTests;

public class MessageSenderTests
{
    [Fact]
    public async Task TopicAndPayloadAreSet()
    {
        var mqttSetup = new MockMqttMessageSenderSetup();

        await mqttSetup.MessageSender.SendMessageAsync("topic", "payload", true, MqttQualityOfServiceLevel.AtMostOnce);
        var publishedMessage = mqttSetup.LastPublishedMessage;

        var payloadAsText = System.Text.Encoding.Default.GetString(publishedMessage.Payload);

        publishedMessage.Topic.Should().Be("topic");
        payloadAsText.Should().Be("payload");
    }

    [Fact]
    public async Task RetainFlagCanBeSetTrue()
    {
        var mqttSetup = new MockMqttMessageSenderSetup();

        await mqttSetup.MessageSender.SendMessageAsync("topic", "payload", true, MqttQualityOfServiceLevel.AtMostOnce);
        var publishedMessage = mqttSetup.LastPublishedMessage;

        publishedMessage.Retain.Should().BeTrue();
    }

    [Fact]
    public async Task RetainFlagCanBeSetFalse()
    {
        var mqttSetup = new MockMqttMessageSenderSetup();

        await mqttSetup.MessageSender.SendMessageAsync("topic", "payload", false, MqttQualityOfServiceLevel.AtMostOnce);
        var publishedMessage = mqttSetup.LastPublishedMessage;

        publishedMessage.Retain.Should().BeFalse();
    }

    [Fact]
    public async Task FailedSendThrows()
    {
        var mqttSetup = new MockMqttMessageSenderSetup();
        mqttSetup.SetResponseCode(MqttClientPublishReasonCode.UnspecifiedError);

        Func<Task> f = async () =>
        {
            await mqttSetup.MessageSender.SendMessageAsync("topic", "payload", true,
                MqttQualityOfServiceLevel.AtMostOnce);
        };

        await f.Should().ThrowAsync<MqttPublishException>();
    }

    [Fact]
    public async Task CanSetQosLevel()
    {
        var mqttSetup = new MockMqttMessageSenderSetup();

        await mqttSetup.MessageSender.SendMessageAsync("topic", "payload", true, MqttQualityOfServiceLevel.ExactlyOnce);
        var publishedMessage = mqttSetup.LastPublishedMessage;

        publishedMessage.QualityOfServiceLevel.Should().Be(MqttQualityOfServiceLevel.ExactlyOnce);
    }
    
    [Fact]
    public async Task CanSetPersist()
    {
        var mqttSetup = new MockMqttMessageSenderSetup();

        await mqttSetup.MessageSender.SendMessageAsync("topic", "payload", true, MqttQualityOfServiceLevel.ExactlyOnce);
        var publishedMessage = mqttSetup.LastPublishedMessage;

        publishedMessage.Retain.Should().BeTrue();
    }
    
    [Fact]
    public async Task CanUnsetPersist()
    {
        var mqttSetup = new MockMqttMessageSenderSetup();

        await mqttSetup.MessageSender.SendMessageAsync("topic", "payload", false, MqttQualityOfServiceLevel.ExactlyOnce);
        var publishedMessage = mqttSetup.LastPublishedMessage;

        publishedMessage.Retain.Should().BeFalse();
    }
}