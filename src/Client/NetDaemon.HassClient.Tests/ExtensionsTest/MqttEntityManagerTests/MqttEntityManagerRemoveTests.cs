using NetDaemon.Extensions.MqttEntityManager;
using NetDaemon.HassClient.Tests.ExtensionsTest.MqttEntityManagerTests.TestHelpers;

namespace NetDaemon.HassClient.Tests.ExtensionsTest.MqttEntityManagerTests;

/// <summary>
/// Note that there are so many tests relating to <see cref="MqttEntityManager"/> that they are broken into
/// multiple test classes, all MqttEntityManager[OPERATION]Tests
/// </summary>
public class MqttEntityManagerRemoveTests
{
    [Fact]
    public async Task RemoveSetsCorrectTopic()
    {
        var mocks = new MockMqttEntityManagerHelper();

        var entityManager = new MqttEntityManager(mocks.MessageSender, mocks.MessageSubscriber, mocks.Options);

        await entityManager.RemoveAsync("sensor.test");

        mocks.CapturedTopic.Should().Be("HomeAssistant/sensor/test/config");
    }

    [Fact]
    public async Task RemoveHasEmptyPayload()
    {
        var mocks = new MockMqttEntityManagerHelper();

        var entityManager = new MqttEntityManager(mocks.MessageSender, mocks.MessageSubscriber, mocks.Options);

        await entityManager.RemoveAsync("sensor.test");

        mocks.CapturedPayload.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveUnsetsRetainFlag()
    {
        var mocks = new MockMqttEntityManagerHelper();

        var entityManager = new MqttEntityManager(mocks.MessageSender, mocks.MessageSubscriber, mocks.Options);

        await entityManager.RemoveAsync("sensor.test");

        mocks.CapturedRetain.Should().BeFalse();
    }

}
