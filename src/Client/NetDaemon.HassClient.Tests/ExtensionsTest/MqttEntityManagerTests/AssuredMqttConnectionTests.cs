using HiveMQtt.MQTT5.ReasonCodes;
using NetDaemon.Extensions.MqttEntityManager;

namespace NetDaemon.HassClient.Tests.ExtensionsTest.MqttEntityManagerTests;

public class AssuredMqttConnectionTests
{
    [Fact]
    public async Task CanGetClient()
    {
        var logger = new Mock<ILogger<AssuredHiveMqttConnection>>();

        var (mqttClient, mqttFactory) = BuildClientAndFactory();
        SetExpectedConnectResult(mqttClient, ConnAckReasonCode.Success, true);

        var conn = new AssuredHiveMqttConnection(logger.Object, mqttFactory.Object);

        // Act by calling GetClientAsync
        var returnedClient = await conn.GetClientAsync();

        // Verify the returned client is correct
        returnedClient.Should().Be(mqttClient.Object);

        // Verify all expected calls were made
        mqttFactory.Verify(f => f.GetClient(), Times.Once);
        mqttClient.Verify(client => client.ConnectAsync(), Times.Once);
    }

    /// <summary>
    /// Note that this may seem counter-intuitive but remember that the HiveMQ client will auto-reconnect.
    /// So the behaviour should be that, even if initial connect fails, we still return a client and assume
    /// that at some point in the future it will successfully connect.
    /// </summary>
    [Fact]
    public async Task CanGetClientEvenIfConnectFails()
    {
        var logger = new Mock<ILogger<AssuredHiveMqttConnection>>();

        var (mqttClient, mqttFactory) = BuildClientAndFactory();
        SetExpectedConnectResult(mqttClient, ConnAckReasonCode.ServerUnavailable, false);

        var conn = new AssuredHiveMqttConnection(logger.Object, mqttFactory.Object);

        // Act by calling GetClientAsync
        var returnedClient = await conn.GetClientAsync();

        // Verify the returned client is correct
        returnedClient.Should().Be(mqttClient.Object);

        // Verify all expected calls were made
        mqttFactory.Verify(f => f.GetClient(), Times.Once);
        mqttClient.Verify(client => client.ConnectAsync(), Times.Once);
    }

    [Fact]
    public async Task RequestingClientTwiceReturnsSameInstance()
    {
        var logger = new Mock<ILogger<AssuredHiveMqttConnection>>();

        var (mqttClient, mqttFactory) = BuildClientAndFactory();
        SetExpectedConnectResult(mqttClient, ConnAckReasonCode.Success, true);

        var conn = new AssuredHiveMqttConnection(logger.Object, mqttFactory.Object);

        // Setup - get the client, first time
        var originalClient = await conn.GetClientAsync();

        // Ensure that IsConnected returns true
        mqttClient.Setup(r => r.IsConnected())
            .Returns(true);

        // Act by calling GetClientAsync again
        var secondRequest = await conn.GetClientAsync();

        // Verify: Note that we can't simply test that the second result is the same as the first because
        // our mock factory is explicitly injecting the same item.
        // So the test is that we do *not* call into the factory GetClient for a second time,
        // nor do we try to reconnect.

        mqttFactory.Verify(f => f.GetClient(), Times.Once);
        mqttClient.Verify(client => client.ConnectAsync(), Times.Once);
    }

    private static (Mock<IHiveMqClientWrapper> mockClient, Mock<IMqttClientFactory> mockFactory) BuildClientAndFactory()
    {
        var mqttClient = new Mock<IHiveMqClientWrapper>();
        var mqttFactory = new Mock<IMqttClientFactory>();
        mqttFactory.Setup(f => f.GetClient())
            .Returns(mqttClient.Object)
            .Verifiable();

        return (mqttClient, mqttFactory);
    }

    private static void SetExpectedConnectResult(Mock<IHiveMqClientWrapper> mockClient, ConnAckReasonCode reasonCode, bool sessionPresent)
    {
        var mockConnectResult = new Mock<IConnectResult>();
        mockConnectResult.Setup(r => r.ReasonCode).Returns(reasonCode);
        mockConnectResult.Setup(r => r.SessionPresent).Returns(sessionPresent);

        mockClient.Setup(client => client.ConnectAsync())
            .ReturnsAsync(mockConnectResult.Object)
            .Verifiable();
    }
}
