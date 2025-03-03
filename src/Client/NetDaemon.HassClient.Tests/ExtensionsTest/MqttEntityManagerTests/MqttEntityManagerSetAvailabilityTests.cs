using NetDaemon.Extensions.MqttEntityManager;
using NetDaemon.HassClient.Tests.ExtensionsTest.MqttEntityManagerTests.TestHelpers;

namespace NetDaemon.HassClient.Tests.ExtensionsTest.MqttEntityManagerTests;

/// <summary>
/// Note that there are so many tests relating to <see cref="MqttEntityManager"/> that they are broken into
/// multiple test classes, all MqttEntityManager[OPERATION]Tests
/// </summary>
public class MqttEntityManagerSetAvailabilityTests
{
    [Fact]
    public async Task SetAvailabilitySetsCorrectTopic()
    {
        var mocks = new MockMqttEntityManagerHelper();

        var entityManager = new MqttEntityManager(mocks.MessageSender, mocks.MessageSubscriber, mocks.Options);

        await entityManager.SetAvailabilityAsync("sensor.test", "down");

        mocks.CapturedTopic.Should().Be("HomeAssistant/sensor/test/availability");
    }

    [Fact]
    public async Task SetAvailabilitySetsCorrectPayload()
    {
        var mocks = new MockMqttEntityManagerHelper();

        var entityManager = new MqttEntityManager(mocks.MessageSender, mocks.MessageSubscriber, mocks.Options);

        await entityManager.SetAvailabilityAsync("sensor.test", "down");

        mocks.CapturedPayload.Should().Be("down");
    }

    [Fact]
    public async Task SetAvailabilitySetsRetainFlag()
    {
        var mocks = new MockMqttEntityManagerHelper();

        var entityManager = new MqttEntityManager(mocks.MessageSender, mocks.MessageSubscriber, mocks.Options);

        await entityManager.SetAvailabilityAsync("sensor.test", "down");

        mocks.CapturedRetain.Should().BeTrue();
    }
}
