using MQTTnet.Extensions.ManagedClient;
using NetDaemon.Extensions.MqttEntityManager;
using NetDaemon.Extensions.MqttEntityManager.Helpers;

namespace NetDaemon.HassClient.Tests.ExtensionsTest.MqttEntityManagerTests;

public class AssuredMqttConnectionTests
{
    [Fact]
    public async Task CanGetClient()
    {
        var logger = new Mock<ILogger<AssuredMqttConnection>>();

        var mqttClient = new Mock<IManagedMqttClient>();
        var mqttFactory = new MqttFactoryWrapper(mqttClient.Object);

        var conn = new AssuredMqttConnection(logger.Object, mqttFactory, GetMockOptions());
        var returnedClient = await conn.GetClientAsync();

        returnedClient.Should().Be(mqttClient.Object);
    }

    private static IOptions<MqttConfiguration> GetMockOptions()
    {
        var options = new Mock<IOptions<MqttConfiguration>>();

        options.Setup(o => o.Value)
            .Returns(() =>  new MqttConfiguration
            {
                Host = "localhost", UserName = "id"
            });

        return options.Object;
    }
}
