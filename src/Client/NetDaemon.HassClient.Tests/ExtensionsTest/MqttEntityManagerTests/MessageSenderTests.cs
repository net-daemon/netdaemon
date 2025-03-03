//
// using NetDaemon.Extensions.MqttEntityManager;
// using NetDaemon.HassClient.Tests.ExtensionsTest.MqttEntityManagerTests.TestHelpers;
//
// namespace NetDaemon.HassClient.Tests.ExtensionsTest.MqttEntityManagerTests;
//
// public class MessageSenderTests
// {
//     [Fact]
//     public async Task TopicAndPayloadAreSet()
//     {
//         var mqttSetup = new MockMqttMessageSenderSetup();
//
//         await mqttSetup.MessageSender.SendMessageAsync("topic", "payload", true, MqttQualityOfServiceLevel.AtMostOnceDelivery);
//         var publishedMessage = mqttSetup.LastPublishedMessage;
//
//         var payloadAsText = System.Text.Encoding.Default.GetString(publishedMessage.PayloadSegment.Array ?? []);
//
//         publishedMessage.Topic.Should().Be("topic");
//         payloadAsText.Should().Be("payload");
//     }
//
//     [Fact]
//     public async Task RetainFlagCanBeSetTrue()
//     {
//         var mqttSetup = new MockMqttMessageSenderSetup();
//
//         await mqttSetup.MessageSender.SendMessageAsync("topic", "payload", true, MqttQualityOfServiceLevel.AtMostOnceDelivery);
//         var publishedMessage = mqttSetup.LastPublishedMessage;
//
//         publishedMessage.Retain.Should().BeTrue();
//     }
//
//     [Fact]
//     public async Task RetainFlagCanBeSetFalse()
//     {
//         var mqttSetup = new MockMqttMessageSenderSetup();
//
//         await mqttSetup.MessageSender.SendMessageAsync("topic", "payload", false, MqttQualityOfServiceLevel.AtMostOnceDelivery);
//         var publishedMessage = mqttSetup.LastPublishedMessage;
//
//         publishedMessage.Retain.Should().BeFalse();
//     }
//
//     [Fact]
//     public async Task CanSetQosLevel()
//     {
//         var mqttSetup = new MockMqttMessageSenderSetup();
//
//         await mqttSetup.MessageSender.SendMessageAsync("topic", "payload", true, MqttQualityOfServiceLevel.ExactlyOnceDelivery);
//         var publishedMessage = mqttSetup.LastPublishedMessage;
//
//         publishedMessage.QualityOfServiceLevel.Should().Be(MqttQualityOfServiceLevel.ExactlyOnceDelivery);
//     }
//
//     [Fact]
//     public async Task CanSetPersist()
//     {
//         var mqttSetup = new MockMqttMessageSenderSetup();
//
//         await mqttSetup.MessageSender.SendMessageAsync("topic", "payload", true, MqttQualityOfServiceLevel.ExactlyOnceDelivery);
//         var publishedMessage = mqttSetup.LastPublishedMessage;
//
//         publishedMessage.Retain.Should().BeTrue();
//     }
//
//     [Fact]
//     public async Task CanUnsetPersist()
//     {
//         var mqttSetup = new MockMqttMessageSenderSetup();
//
//         await mqttSetup.MessageSender.SendMessageAsync("topic", "payload", false, MqttQualityOfServiceLevel.ExactlyOnceDelivery);
//         var publishedMessage = mqttSetup.LastPublishedMessage;
//
//         publishedMessage.Retain.Should().BeFalse();
//     }
// }
