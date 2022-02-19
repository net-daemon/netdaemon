using MQTTnet;
using MQTTnet.Client.Publishing;
using MQTTnet.Extensions.ManagedClient;
using NetDaemon.Extensions.MqttEntityManager;
using NetDaemon.Extensions.MqttEntityManager.Helpers;

namespace NetDaemon.HassClient.Tests.ExtensionsTest.MqttEntityManagerTests.TestHelpers;

/// <summary>
/// These are helpers that set up mock mqtt factories and connection, and allow a message to be sent through
/// the mock client and capture the message that would have been published
/// </summary>
internal class MockMqttMessageSenderSetup
{
    public AssuredMqttConnection Connection { get; private set; } = null!;
    public Mock<IManagedMqttClient> MqttClient { get; private set; } = null!;
    public MqttFactoryWrapper MqttFactory { get; private set; } = null!;
    public MessageSender MessageSender { get; private set; } = null!;
    public MqttApplicationMessage LastPublishedMessage { get; set; } = null!;

    public MockMqttMessageSenderSetup()
    {
        SetupMockMqtt();
        SetupMessageSender();
        SetResponseCode(MqttClientPublishReasonCode.Success);
    }

    public void SetResponseCode(MqttClientPublishReasonCode code)
    {
        var publishResult = new MqttClientPublishResult() { ReasonCode = code };
        
        // Ensure that when the MQTT Client is called, it's published message is saved and that it returns
        // the specified response code
        MqttClient.Setup(m => m.PublishAsync(It.IsAny<MqttApplicationMessage>(), It.IsAny<CancellationToken>()))
            .Callback<MqttApplicationMessage, CancellationToken>((message, token) =>
            {
                LastPublishedMessage = message;
            })
            .ReturnsAsync(publishResult);
    }

    /// <summary>
    /// Get a mocked MQTT client, factor and connection
    /// </summary>
    /// <returns></returns>
    private void SetupMockMqtt()
    {
        var options = new Mock<IOptions<MqttConfiguration>>();

        options.Setup(o => o.Value)
            .Returns(() => new MqttConfiguration
            {
                Host = "localhost", UserName = "id"
            });

        MqttClient = new Mock<IManagedMqttClient>();
        MqttFactory = new MqttFactoryWrapper(MqttClient.Object);

        Connection = new AssuredMqttConnection(new Mock<ILogger<AssuredMqttConnection>>().Object, MqttFactory,
            options.Object);
    }
    
    private void SetupMessageSender()
    {
        var logger = new Mock<ILogger<MessageSender>>().Object;
        MessageSender = new MessageSender(logger, Connection);
        
    }
}