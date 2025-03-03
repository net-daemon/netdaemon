using NetDaemon.Extensions.MqttEntityManager;
using NetDaemon.HassClient.Tests.ExtensionsTest.MqttEntityManagerTests.TestHelpers;

namespace NetDaemon.HassClient.Tests.ExtensionsTest.MqttEntityManagerTests;

/// <summary>
/// Note that there are so many tests relating to <see cref="MqttEntityManager"/> that they are broken into
/// multiple test classes, all MqttEntityManager[OPERATION]Tests
/// </summary>
public class MqttEntityManagerSetAttributesTests
{
    [Fact]
    public async Task SetAttributesSetsCorrectTopic()
    {
        var mocks = new MockMqttEntityManagerHelper();

        var entityManager = new MqttEntityManager(mocks.MessageSender, mocks.MessageSubscriber, mocks.Options);

        await entityManager.SetAttributesAsync("sensor.test", new { a1 = "one", a2 = 2 });

        mocks.CapturedTopic.Should().Be("HomeAssistant/sensor/test/attributes");
    }

    [Fact]
    public async Task SetAttributesCanSetSimplePayload()
    {
        var mocks = new MockMqttEntityManagerHelper();

        var entityManager = new MqttEntityManager(mocks.MessageSender, mocks.MessageSubscriber, mocks.Options);

        await entityManager.SetAttributesAsync("sensor.test", new { a1 = "one", a2 = 2 });

        var parsedPayload = JsonDocument.Parse(mocks.CapturedPayload!);
        parsedPayload.RootElement.GetProperty("a1").GetString().Should().Be("one");
        parsedPayload.RootElement.GetProperty("a2").GetInt32().Should().Be(2);
    }

    [Fact]
    public async Task SetAttributesCanSetComplexPayload()
    {
        var mocks = new MockMqttEntityManagerHelper();

        var entityManager = new MqttEntityManager(mocks.MessageSender, mocks.MessageSubscriber, mocks.Options);

        var complexObject = new
        {
            a1 = "one", a2 = 2, a3 = new
            {
                a31 = "three", a32 = 3
            }
        };
        await entityManager.SetAttributesAsync("sensor.test", complexObject);

        var parsedPayload = JsonDocument.Parse(mocks.CapturedPayload!);
        parsedPayload.RootElement.GetProperty("a1").GetString().Should().Be("one");
        parsedPayload.RootElement.GetProperty("a2").GetInt32().Should().Be(2);
        parsedPayload.RootElement.GetProperty("a3").GetProperty("a31").GetString().Should().Be("three");
        parsedPayload.RootElement.GetProperty("a3").GetProperty("a32").GetInt32().Should().Be(3);
    }

    [Fact]
    public async Task SetAttributesSetsRetainFlag()
    {
        var mocks = new MockMqttEntityManagerHelper();

        var entityManager = new MqttEntityManager(mocks.MessageSender, mocks.MessageSubscriber, mocks.Options);

        await entityManager.SetAttributesAsync("sensor.test", new { a1 = "one", a2 = 2 });

        mocks.CapturedRetain.Should().BeTrue();
    }
}
