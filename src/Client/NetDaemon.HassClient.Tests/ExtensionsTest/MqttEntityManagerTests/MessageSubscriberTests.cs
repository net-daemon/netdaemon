using NetDaemon.Extensions.MqttEntityManager;
using NetDaemon.HassClient.Tests.ExtensionsTest.MqttEntityManagerTests.TestHelpers;

namespace NetDaemon.HassClient.Tests.ExtensionsTest.MqttEntityManagerTests;

public class MessageSubscriberTests
{
    [Fact]
    public async Task SubscribeStoresTopicFilter()
    {
        var connection = new FakeAssuredMqttConnection();
        var subscriber = new MessageSubscriber(new Mock<ILogger<MessageSubscriber>>().Object, connection);

        await subscriber.SubscribeTopicAsync("homeassistant/domain/sensor/set");

        connection.Subscriptions.Should().ContainSingle();
        connection.Subscriptions[0].Topic.Should().Be("homeassistant/domain/sensor/set");
    }

    [Fact]
    public async Task ReceivedMessageIsForwardedToMatchingSubscriber()
    {
        var connection = new FakeAssuredMqttConnection();
        var subscriber = new MessageSubscriber(new Mock<ILogger<MessageSubscriber>>().Object, connection);
        var observable = await subscriber.SubscribeTopicAsync("homeassistant/domain/sensor/set");
        var received = observable.FirstAsync().ToTask();

        await connection.ReceiveAsync("homeassistant/domain/sensor/set", "on");

        (await received.WaitAsync(TimeSpan.FromSeconds(1))).Should().Be("on");
    }

    [Fact]
    public async Task ReceivedMessageIsIgnoredForUnmatchedTopic()
    {
        var connection = new FakeAssuredMqttConnection();
        var subscriber = new MessageSubscriber(new Mock<ILogger<MessageSubscriber>>().Object, connection);
        var observable = await subscriber.SubscribeTopicAsync("homeassistant/domain/sensor/set");
        var received = new List<string>();
        using var subscription = observable.Subscribe(received.Add);

        await connection.ReceiveAsync("homeassistant/domain/other/set", "on");
        await Task.Delay(50);

        received.Should().BeEmpty();
    }
}
