using NetDaemon.Extensions.MqttEntityManager;
using NetDaemon.HassClient.Tests.ExtensionsTest.MqttEntityManagerTests.TestHelpers;
using Newtonsoft.Json.Linq;

namespace NetDaemon.HassClient.Tests.ExtensionsTest.MqttEntityManagerTests;

/// <summary>
/// Note that there are so many tests relating to <see cref="MqttEntityManager"/> that they are broken into
/// multiple test classes, all MqttEntityManager[OPERATION]Tests
/// </summary>
public class MqttEntityManagerCreateTests
{
    [Fact]
    public async Task CreateSetsCorrectTopic()
    {
        var mocks = new MockMqttEntityManagerHelper();

        var entityManager = new MqttEntityManager(mocks.MessageSender, mocks.MessageSubscriber, mocks.Options);

        await entityManager.CreateAsync("sensor.test");

        mocks.CapturedTopic.Should().Be("HomeAssistant/sensor/test/config");
    }

    [Fact]
    public async Task CreateSetsDefaultQos()
    {
        var mocks = new MockMqttEntityManagerHelper();

        var entityManager = new MqttEntityManager(mocks.MessageSender, mocks.MessageSubscriber, mocks.Options);

        await entityManager.CreateAsync("sensor.test");

        mocks.CapturedQos.Should().Be(MqttQualityOfServiceLevel.AtMostOnceDelivery);
    }

    [Fact]
    public async Task CreatePayloadSetsName()
    {
        var mocks = new MockMqttEntityManagerHelper();

        var entityManager = new MqttEntityManager(mocks.MessageSender, mocks.MessageSubscriber, mocks.Options);

        await entityManager.CreateAsync("sensor.test");

        var parsedPayload = JsonDocument.Parse(mocks.CapturedPayload!);
        parsedPayload.RootElement.GetProperty("name").ToString().Should().Be("test");
    }

    [Fact]
    public async Task CreatePayloadSetsObjectId()
    {
        var mocks = new MockMqttEntityManagerHelper();

        var entityManager = new MqttEntityManager(mocks.MessageSender, mocks.MessageSubscriber, mocks.Options);

        await entityManager.CreateAsync("sensor.test");

        var parsedPayload = JsonDocument.Parse(mocks.CapturedPayload!);
        parsedPayload.RootElement.GetProperty("object_id").ToString().Should().Be("test");
    }

    [Fact]
    public async Task CreatePayloadSetsUniqueId()
    {
        var mocks = new MockMqttEntityManagerHelper();

        var entityManager = new MqttEntityManager(mocks.MessageSender, mocks.MessageSubscriber, mocks.Options);

        await entityManager.CreateAsync("sensor.test");

        var parsedPayload = JsonDocument.Parse(mocks.CapturedPayload!);
        parsedPayload.RootElement.GetProperty("unique_id").ToString().Should().Be("HomeAssistant_sensor_test_config");
    }

    [Fact]
    public async Task CreatePayloadSetsCommandTopic()
    {
        var mocks = new MockMqttEntityManagerHelper();

        var entityManager = new MqttEntityManager(mocks.MessageSender, mocks.MessageSubscriber, mocks.Options);

        await entityManager.CreateAsync("sensor.test");

        var parsedPayload = JsonDocument.Parse(mocks.CapturedPayload!);
        parsedPayload.RootElement.GetProperty("command_topic").ToString().Should().Be("HomeAssistant/sensor/test/set");
    }

    [Fact]
    public async Task CreatePayloadSetsStateTopic()
    {
        var mocks = new MockMqttEntityManagerHelper();

        var entityManager = new MqttEntityManager(mocks.MessageSender, mocks.MessageSubscriber, mocks.Options);

        await entityManager.CreateAsync("sensor.test");

        var parsedPayload = JsonDocument.Parse(mocks.CapturedPayload!);
        parsedPayload.RootElement.GetProperty("state_topic").ToString().Should().Be("HomeAssistant/sensor/test/state");
    }

    [Fact]
    public async Task CreatePayloadSetsJsonAttributesTopic()
    {
        var mocks = new MockMqttEntityManagerHelper();

        var entityManager = new MqttEntityManager(mocks.MessageSender, mocks.MessageSubscriber, mocks.Options);

        await entityManager.CreateAsync("sensor.test");

        var parsedPayload = JsonDocument.Parse(mocks.CapturedPayload!);
        parsedPayload.RootElement.GetProperty("json_attributes_topic").ToString().Should().Be("HomeAssistant/sensor/test/attributes");
    }

    [Fact]
    public async Task CreateCanSetAdditionalOptions()
    {
        var mocks = new MockMqttEntityManagerHelper();

        var entityManager = new MqttEntityManager(mocks.MessageSender, mocks.MessageSubscriber, mocks.Options);

        await entityManager.CreateAsync("sensor.test", options: null, additionalConfig: new { myOption = "new value" });

        var parsedPayload = JsonDocument.Parse(mocks.CapturedPayload!);
        parsedPayload.RootElement.GetProperty("myOption").ToString().Should().Be("new value");
    }

    [Fact]
    public async Task CreateCanOverrideDefaultSettings()
    {
        var mocks = new MockMqttEntityManagerHelper();

        var entityManager = new MqttEntityManager(mocks.MessageSender, mocks.MessageSubscriber, mocks.Options);

        await entityManager.CreateAsync("sensor.test", options: null, additionalConfig: new { unique_id = "overridden" });

        var parsedPayload = JsonDocument.Parse(mocks.CapturedPayload!);
        parsedPayload.RootElement.GetProperty("unique_id").ToString().Should().Be("overridden");
    }

    [Fact]
    public async Task CreateWithNoOptionsDoesNotSetAdditionalFeatures()
    {
        var mocks = new MockMqttEntityManagerHelper();

        var entityManager = new MqttEntityManager(mocks.MessageSender, mocks.MessageSubscriber, mocks.Options);

        await entityManager.CreateAsync("sensor.test", options: null);

        var parsedTokens = JToken.Parse(mocks.CapturedPayload!);
        parsedTokens.Should().NotContain("tnc");
    }

    [Fact]
    public async Task CreateWithDeviceClassSetsDeviceClass()
    {
        var mocks = new MockMqttEntityManagerHelper();

        var entityManager = new MqttEntityManager(mocks.MessageSender, mocks.MessageSubscriber, mocks.Options);

        await entityManager.CreateAsync("sensor.test", options: new EntityCreationOptions { DeviceClass = "temperature" });

        var parsedPayload = JsonDocument.Parse(mocks.CapturedPayload!);
        parsedPayload.RootElement.GetProperty("device_class").ToString().Should().Be("temperature");
    }

    [Fact]
    public async Task CreateWithPayloadAvailableSetsAvailabilityTopics()
    {
        var mocks = new MockMqttEntityManagerHelper();

        var entityManager = new MqttEntityManager(mocks.MessageSender, mocks.MessageSubscriber, mocks.Options);

        await entityManager.CreateAsync("sensor.test", options: new EntityCreationOptions { PayloadAvailable = "UP" });

        var parsedPayload = JsonDocument.Parse(mocks.CapturedPayload!);
        parsedPayload.RootElement.GetProperty("availability_topic").ToString().Should().Be("HomeAssistant/sensor/test/availability");
        parsedPayload.RootElement.GetProperty("payload_available").ToString().Should().Be("UP");
    }

    [Fact]
    public async Task CreateWithPayloadNotAvailableSetsAvailabilityTopics()
    {
        var mocks = new MockMqttEntityManagerHelper();

        var entityManager = new MqttEntityManager(mocks.MessageSender, mocks.MessageSubscriber, mocks.Options);

        await entityManager.CreateAsync("sensor.test", options: new EntityCreationOptions { PayloadNotAvailable = "DOWN" });

        var parsedPayload = JsonDocument.Parse(mocks.CapturedPayload!);
        parsedPayload.RootElement.GetProperty("availability_topic").ToString().Should().Be("HomeAssistant/sensor/test/availability");
        parsedPayload.RootElement.GetProperty("payload_not_available").ToString().Should().Be("DOWN");
    }

    [Fact]
    public async Task CreateWithPayloadOnSetsPayloadOn()
    {
        var mocks = new MockMqttEntityManagerHelper();

        var entityManager = new MqttEntityManager(mocks.MessageSender, mocks.MessageSubscriber, mocks.Options);

        await entityManager.CreateAsync("sensor.test", options: new EntityCreationOptions { PayloadOn = "ON" });

        var parsedPayload = JsonDocument.Parse(mocks.CapturedPayload!);
        parsedPayload.RootElement.GetProperty("payload_on").ToString().Should().Be("ON");
    }

    [Fact]
    public async Task CreateWithPayloadOnSetsPayloadOff()
    {
        var mocks = new MockMqttEntityManagerHelper();

        var entityManager = new MqttEntityManager(mocks.MessageSender, mocks.MessageSubscriber, mocks.Options);

        await entityManager.CreateAsync("sensor.test", options: new EntityCreationOptions { PayloadOff = "OFF" });

        var parsedPayload = JsonDocument.Parse(mocks.CapturedPayload!);
        parsedPayload.RootElement.GetProperty("payload_off").ToString().Should().Be("OFF");
    }

    [Fact]
    public async Task CreateCanOverrideName()
    {
        var mocks = new MockMqttEntityManagerHelper();

        var entityManager = new MqttEntityManager(mocks.MessageSender, mocks.MessageSubscriber, mocks.Options);

        await entityManager.CreateAsync("sensor.test", options: new EntityCreationOptions { Name = "custom_name" });

        var parsedPayload = JsonDocument.Parse(mocks.CapturedPayload!);
        parsedPayload.RootElement.GetProperty("name").ToString().Should().Be("custom_name");
    }

    [Fact]
    public async Task CreateCanOverrideUniqueId()
    {
        var mocks = new MockMqttEntityManagerHelper();

        var entityManager = new MqttEntityManager(mocks.MessageSender, mocks.MessageSubscriber, mocks.Options);

        await entityManager.CreateAsync("sensor.test", options: new EntityCreationOptions { UniqueId = "unique" });

        var parsedPayload = JsonDocument.Parse(mocks.CapturedPayload!);
        parsedPayload.RootElement.GetProperty("unique_id").ToString().Should().Be("unique");
    }

    [Fact]
    public async Task CreateSetsRetainFlagTrueByDefault()
    {
        var mocks = new MockMqttEntityManagerHelper();

        var entityManager = new MqttEntityManager(mocks.MessageSender, mocks.MessageSubscriber, mocks.Options);

        await entityManager.CreateAsync("sensor.test");

        mocks.CapturedRetain.Should().BeTrue();
    }

    [Fact]
    public async Task CreateCanOverrideRetainFlag()
    {
        var mocks = new MockMqttEntityManagerHelper();

        var entityManager = new MqttEntityManager(mocks.MessageSender, mocks.MessageSubscriber, mocks.Options);

        await entityManager.CreateAsync("sensor.test", options: new EntityCreationOptions { Persist = false });

        mocks.CapturedRetain.Should().BeFalse();
    }
}
