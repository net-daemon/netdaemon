using HiveMQtt.Client;
using HiveMQtt.MQTT5.Types;
using NetDaemon.Extensions.MqttEntityManager;

namespace NetDaemon.HassClient.Tests.ExtensionsTest.MqttEntityManagerTests.TestHelpers;

/// <summary>
/// These are helpers that set up mock mqtt factories and connection, and allow a message to be sent through
/// the mock client and capture the message that would have been published
/// </summary>
internal sealed class MockMqttMessageSenderSetup
{
    public IAssuredMqttConnection Connection { get; private set; } = null!;
    public Mock<IHiveMQClient> MqttClient { get; private set; } = null!;
    public MessageSender MessageSender { get; private set; } = null!;
    public MQTT5PublishMessage LastPublishedMessage { get; set; } = null!;

    public MockMqttMessageSenderSetup()
    {
        SetupMockMqtt();
        SetupMessageSender();
        SetupMessageReceiver();
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public void SetupMessageReceiver()
    {
        // Ensure that when the MQTT Client is called its published message is saved
        MqttClient.Setup(m => m.PublishAsync(It.IsAny<MQTT5PublishMessage>(), It.IsAny<CancellationToken>()))
            .Callback<MQTT5PublishMessage>(message =>
            {
                LastPublishedMessage = message;
            });
    }

    /// <summary>
    /// Get a mocked MQTT client, factor and connection
    /// </summary>
    /// <returns></returns>
    private async Task SetupMockMqtt()
    {
        // var logger = new Mock<ILogger<AssuredHiveMqttConnection>>();
        // var mqttClient = new Mock<IHiveMQClient>();
        // var mqttFactory = new Mock<IMqttClientFactory>();
        //
        // mqttFactory.Setup(f => f.GetClient())
        //     .Returns(mqttClient.Object)
        //     .Verifiable(Times.Once);
        //
        // Connection = new AssuredHiveMqttConnection(logger.Object, mqttFactory.Object);
        // MqttClient = mqttClient;
        //
        // // Remember to ensure that the connection has hooked up our mock client
        // Assert.Equal(mqttClient.Object, await Connection.GetClientAsync());
    }

    private void SetupMessageSender()
    {
        var logger = new Mock<ILogger<MessageSender>>().Object;
        MessageSender = new MessageSender(logger, Connection);
    }
}
