using NetDaemon.Extensions.MqttEntityManager;
using NetDaemon.HassClient.Tests.ExtensionsTest.MqttEntityManagerTests.TestHelpers;

namespace NetDaemon.HassClient.Tests.ExtensionsTest.MqttEntityManagerTests;

/// <summary>
/// Note that there are so many tests relating to <see cref="MqttEntityManager"/> that they are broken into
/// multiple test classes, all MqttEntityManager[OPERATION]Tests
/// </summary>
public class MqttEntityManagerSetStateTests
{
    [Fact]
    public async Task SetStateSetsCorrectTopic()
    {
        var mocks = new MockMqttEntityManagerHelper();

        var entityManager = new MqttEntityManager(mocks.MessageSender, mocks.MessageSubscriber, mocks.Options);

        await entityManager.SetStateAsync("sensor.test", "down");

        mocks.CapturedTopic.Should().Be("HomeAssistant/sensor/test/state");
    }

    [Fact]
    public async Task SetStateSetsPayload()
    {
        var mocks = new MockMqttEntityManagerHelper();

        var entityManager = new MqttEntityManager(mocks.MessageSender, mocks.MessageSubscriber, mocks.Options);

        await entityManager.SetStateAsync("sensor.test", "down");

        mocks.CapturedPayload.Should().Be("down");
    }

    [Fact]
    public async Task SetStateSetsRetainFlag()
    {
        var mocks = new MockMqttEntityManagerHelper();

        var entityManager = new MqttEntityManager(mocks.MessageSender, mocks.MessageSubscriber, mocks.Options);

        await entityManager.SetStateAsync("sensor.test", "down");

        mocks.CapturedRetain.Should().BeTrue();
    }
}
