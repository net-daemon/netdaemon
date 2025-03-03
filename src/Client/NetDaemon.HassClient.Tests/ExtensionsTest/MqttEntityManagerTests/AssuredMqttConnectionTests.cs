using System.Reflection;
using HiveMQtt.Client;
using HiveMQtt.Client.Results;
using HiveMQtt.MQTT5.ReasonCodes;
using NetDaemon.Extensions.MqttEntityManager;

namespace NetDaemon.HassClient.Tests.ExtensionsTest.MqttEntityManagerTests;

public class AssuredMqttConnectionTests
{
    [Fact]
    public async Task CanGetClient()
    {
        // Mock logger
        var logger = new Mock<ILogger<AssuredHiveMqttConnection>>();

        // Mock the wrapper client (IHiveMqClientWrapper)
        var mqttClient = new Mock<IHiveMqClientWrapper>();

        // Mock the factory to return the wrapper client
        var mqttFactory = new Mock<IMqttClientFactory>();
        mqttFactory.Setup(f => f.GetClient())
            .Returns(mqttClient.Object)
            .Verifiable();

        // Mock the ConnectAsync behavior to return a mocked result
        var mockConnectResult = new Mock<IConnectResult>();
        mockConnectResult.Setup(r => r.ReasonCode).Returns(ConnAckReasonCode.Success);
        mockConnectResult.Setup(r => r.SessionPresent).Returns(false);

        mqttClient.Setup(client => client.ConnectAsync())
            .ReturnsAsync(mockConnectResult.Object)
            .Verifiable();

        // Create the AssuredHiveMqttConnection
        var conn = new AssuredHiveMqttConnection(logger.Object, mqttFactory.Object);

        // Act by calling GetClientAsync
        var returnedClient = await conn.GetClientAsync();

        // Verify the returned client is correct
        returnedClient.Should().Be(mqttClient.Object);

        // Verify all expected calls were made
        mqttFactory.Verify(f => f.GetClient(), Times.Once);
        mqttClient.Verify(client => client.ConnectAsync(), Times.Once);
    }
}
