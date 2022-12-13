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

    [Fact]
    public async Task GetClientAbortsAfterTimeout()
    {
        var timeout = TimeSpan.FromMilliseconds(5);
        var logger = new Mock<ILogger<AssuredMqttConnection>>();
        
        var mqttClient = new Mock<IManagedMqttClient>();
        var mqttFactory = new MqttFactoryWrapper(mqttClient.Object);

        mqttClient.Setup(m => m.StartAsync(It.IsAny<ManagedMqttClientOptions>()))
            .Callback(() => Thread.Sleep(TimeSpan.FromSeconds(1)));
        
        var conn = new AssuredMqttConnection(logger.Object, mqttFactory, GetMockOptions());
        var act = async () => { await conn.GetClientAsync(timeout); };
        
        await act.Should().ThrowAsync<TimeoutException>();
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