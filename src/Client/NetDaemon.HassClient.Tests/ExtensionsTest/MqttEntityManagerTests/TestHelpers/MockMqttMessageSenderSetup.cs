using MQTTnet;
using MQTTnet.Extensions.ManagedClient;
using NetDaemon.Extensions.MqttEntityManager;
using NetDaemon.Extensions.MqttEntityManager.Helpers;

namespace NetDaemon.HassClient.Tests.ExtensionsTest.MqttEntityManagerTests.TestHelpers;

/// <summary>
/// These are helpers that set up mock mqtt factories and connection, and allow a message to be sent through
/// the mock client and capture the message that would have been published
/// </summary>
internal sealed class MockMqttMessageSenderSetup
{
    public AssuredMqttConnection Connection { get; private set; } = null!;
    public Mock<IManagedMqttClient> MqttClient { get; private set; } = null!;
    public Mock<IMqttClientOptionsFactory> MqttClientOptionsFactory { get; private set; } = null!;

    public MqttFactoryWrapper MqttFactory { get; private set; } = null!;
    public MessageSender MessageSender { get; private set; } = null!;
    public MqttApplicationMessage LastPublishedMessage { get; set; } = null!;

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
        MqttClient.Setup(m => m.EnqueueAsync(It.IsAny<MqttApplicationMessage>()))
            .Callback<MqttApplicationMessage>((message) =>
            {
                LastPublishedMessage = message;
            });
    }

    /// <summary>
    /// Get a mocked MQTT client, factor and connection
    /// </summary>
    /// <returns></returns>
    private void SetupMockMqtt()
    {
        var mqttConfiguration = new MqttConfiguration
        {
            Host = "localhost",
            UserName = "id"
        };

        var options = new Mock<IOptions<MqttConfiguration>>();

        options.Setup(o => o.Value)
            .Returns(() => mqttConfiguration);

        MqttClient = new Mock<IManagedMqttClient>();
        MqttClientOptionsFactory = new Mock<IMqttClientOptionsFactory>();
        MqttFactory = new MqttFactoryWrapper(MqttClient.Object);

        MqttClientOptionsFactory
            .Setup(o => o.CreateClientOptions(mqttConfiguration))
            .Returns(new ManagedMqttClientOptions());

        Connection = new AssuredMqttConnection(
            new Mock<ILogger<AssuredMqttConnection>>().Object,
            MqttClientOptionsFactory.Object,
            MqttFactory,
            options.Object);
    }

    private void SetupMessageSender()
    {
        var logger = new Mock<ILogger<MessageSender>>().Object;
        MessageSender = new MessageSender(logger, Connection);

    }
}
