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
        var mqttClientOptionsFactory = new Mock<IMqttClientOptionsFactory>();
        var mqttConfigurationOptions = new Mock<IOptions<MqttConfiguration>>();

        ConfigureMockOptions(mqttConfigurationOptions);

        mqttClientOptionsFactory.Setup(f => f.CreateClientOptions(It.Is<MqttConfiguration>(o => o.Host == "localhost" && o.UserName == "id")))
            .Returns(new ManagedMqttClientOptions())
            .Verifiable(Times.Once);

        var conn = new AssuredMqttConnection(logger.Object, mqttClientOptionsFactory.Object, mqttFactory, mqttConfigurationOptions.Object);
        var returnedClient = await conn.GetClientAsync();

        returnedClient.Should().Be(mqttClient.Object);

        mqttClientOptionsFactory.VerifyAll();
        mqttConfigurationOptions.VerifyAll();
    }

    private static void ConfigureMockOptions(Mock<IOptions<MqttConfiguration>> mockOptions, Action<MqttConfiguration>? configuration = null)
    {
        var mqttConfiguration = new MqttConfiguration
        {
            Host = "localhost",
            UserName = "id"
        };

        configuration?.Invoke(mqttConfiguration);

        mockOptions.SetupGet(o => o.Value)
            .Returns(() => mqttConfiguration)
            .Verifiable(Times.Once);
    }
}
