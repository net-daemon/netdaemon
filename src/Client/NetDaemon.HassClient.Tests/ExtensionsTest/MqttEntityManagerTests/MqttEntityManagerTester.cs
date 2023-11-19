using NetDaemon.Extensions.MqttEntityManager;
using NetDaemon.HassClient.Tests.ExtensionsTest.MqttEntityManagerTests.TestHelpers;

namespace NetDaemon.HassClient.Tests.ExtensionsTest.MqttEntityManagerTests;

public class MqttEntityManagerTester
{
    [Fact]
    public async Task CreateSetsTopic()
    {
        var mqttSetup = new MockMqttMessageSenderSetup();
        var entityManager = new MqttEntityManager(mqttSetup.MessageSender, null!, GetOptions());

        await entityManager.CreateAsync("domain.sensor", new EntityCreationOptions());
        mqttSetup.LastPublishedMessage.Topic.Should().Be("homeassistant/domain/sensor/config");
    }

    [Fact]
    public async Task CreateWithNoOptionsSetsBaseConfig()
    {
        var mqttSetup = new MockMqttMessageSenderSetup();
        var entityManager = new MqttEntityManager(mqttSetup.MessageSender, null!, GetOptions());

        await entityManager.CreateAsync("domain.sensor");
        var payload = PayloadToDictionary(mqttSetup.LastPublishedMessage.PayloadSegment.Array ?? Array.Empty<byte>() );

        payload?.Count.Should().Be(6);
        payload?["name"].ToString().Should().Be("sensor");
        payload?["unique_id"].ToString().Should().Be("homeassistant_domain_sensor_config");
        payload?["object_id"].ToString().Should().Be("sensor");
        payload?["command_topic"].ToString().Should().Be("homeassistant/domain/sensor/set");
        payload?["state_topic"].ToString().Should().Be("homeassistant/domain/sensor/state");
        payload?["json_attributes_topic"].ToString().Should().Be("homeassistant/domain/sensor/attributes");
    }

    [Fact]
    public async Task CreateWithDefaultOptionsSetsBaseConfig()
    {
        var mqttSetup = new MockMqttMessageSenderSetup();
        var entityManager = new MqttEntityManager(mqttSetup.MessageSender, null!, GetOptions());

        await entityManager.CreateAsync("domain.sensor", new EntityCreationOptions());
        var payload = PayloadToDictionary(mqttSetup.LastPublishedMessage.PayloadSegment.Array ?? Array.Empty<byte>());

        payload?.Count.Should().Be(6);
        payload?["name"].ToString().Should().Be("sensor");
        payload?["unique_id"].ToString().Should().Be("homeassistant_domain_sensor_config");
        payload?["object_id"].ToString().Should().Be("sensor");
        payload?["command_topic"].ToString().Should().Be("homeassistant/domain/sensor/set");
        payload?["state_topic"].ToString().Should().Be("homeassistant/domain/sensor/state");
        payload?["json_attributes_topic"].ToString().Should().Be("homeassistant/domain/sensor/attributes");
    }

    [Fact]
    public async Task CreateCanSetUniqueId()
    {
        var mqttSetup = new MockMqttMessageSenderSetup();
        var entityManager = new MqttEntityManager(mqttSetup.MessageSender, null!, GetOptions());

        await entityManager.CreateAsync("domain.sensor", new EntityCreationOptions(UniqueId: "my_id"));
        var payload = PayloadToDictionary(mqttSetup.LastPublishedMessage.PayloadSegment.Array ?? Array.Empty<byte>());

        payload?["unique_id"].ToString().Should().Be("my_id");
    }

    [Fact]
    public async Task CreateSetsObjectId()
    {
        var mqttSetup = new MockMqttMessageSenderSetup();
        var entityManager = new MqttEntityManager(mqttSetup.MessageSender, null!, GetOptions());

        await entityManager.CreateAsync("domain.the_id");
        var payload = PayloadToDictionary(mqttSetup.LastPublishedMessage.PayloadSegment.Array ?? Array.Empty<byte>());

        payload?["object_id"].ToString().Should().Be("the_id");
    }

    [Fact]
    public async Task CreateCanSetDeviceClass()
    {
        var mqttSetup = new MockMqttMessageSenderSetup();
        var entityManager = new MqttEntityManager(mqttSetup.MessageSender, null!, GetOptions());

        await entityManager.CreateAsync("domain.sensor", new EntityCreationOptions(DeviceClass: "classy"));
        var payload = PayloadToDictionary(mqttSetup.LastPublishedMessage.PayloadSegment.Array ?? Array.Empty<byte>());

        payload?["device_class"].ToString().Should().Be("classy");
    }

    [Fact]
    public async Task CreateCanSetName()
    {
        var mqttSetup = new MockMqttMessageSenderSetup();
        var entityManager = new MqttEntityManager(mqttSetup.MessageSender, null!, GetOptions());

        await entityManager.CreateAsync("domain.sensor", new EntityCreationOptions(Name: "george"));
        var payload = PayloadToDictionary(mqttSetup.LastPublishedMessage.PayloadSegment.Array ?? Array.Empty<byte>());

        payload?["name"].ToString().Should().Be("george");
    }

    [Fact]
    public async Task CreateDefaultsToPersist()
    {
        var mqttSetup = new MockMqttMessageSenderSetup();
        var entityManager = new MqttEntityManager(mqttSetup.MessageSender, null!, GetOptions());

        await entityManager.CreateAsync("domain.sensor", new EntityCreationOptions());

        mqttSetup.LastPublishedMessage.Retain.Should().BeTrue();
    }

    [Fact]
    public async Task CreateCanDisablePersist()
    {
        var mqttSetup = new MockMqttMessageSenderSetup();
        var entityManager = new MqttEntityManager(mqttSetup.MessageSender, null!, GetOptions());

        await entityManager.CreateAsync("domain.sensor", new EntityCreationOptions(Persist: false));

        mqttSetup.LastPublishedMessage.Retain.Should().BeFalse();
    }

    [Fact]
    public async Task CreateCanSetAdditionalOptions()
    {
        var mqttSetup = new MockMqttMessageSenderSetup();
        var entityManager = new MqttEntityManager(mqttSetup.MessageSender, null!, GetOptions());

        var otherOptions = new { sub_class = "lights", up_state = "live" };

        await entityManager.CreateAsync("domain.sensor", additionalConfig: otherOptions);
        var payload = PayloadToDictionary(mqttSetup.LastPublishedMessage.PayloadSegment.Array ?? Array.Empty<byte>());

        payload?["sub_class"].ToString().Should().Be("lights");
        payload?["up_state"].ToString().Should().Be("live");
    }

    [Fact]
    public async Task CreateCanOverrideBaseConfig()
    {
        var mqttSetup = new MockMqttMessageSenderSetup();
        var entityManager = new MqttEntityManager(mqttSetup.MessageSender, null!, GetOptions());

        var otherOptions = new { command_topic = "my/topic" };

        await entityManager.CreateAsync("domain.sensor", additionalConfig: otherOptions);
        var payload = PayloadToDictionary(mqttSetup.LastPublishedMessage.PayloadSegment.Array ?? Array.Empty<byte>());

        payload?["command_topic"].ToString().Should().Be("my/topic");
    }

    [Fact]
    public async Task CreateAvailabilityTopicOffByDefault()
    {
        var mqttSetup = new MockMqttMessageSenderSetup();
        var entityManager = new MqttEntityManager(mqttSetup.MessageSender, null!, GetOptions());

        await entityManager.CreateAsync("domain.sensor");
        var payload = PayloadToDictionary(mqttSetup.LastPublishedMessage.PayloadSegment.Array ?? Array.Empty<byte>());

        payload?.ContainsKey("availability_topic").Should().BeFalse();
    }

    [Fact]
    public async Task CreateAvailabilityTopicSetForAvailUp()
    {
        var mqttSetup = new MockMqttMessageSenderSetup();
        var entityManager = new MqttEntityManager(mqttSetup.MessageSender, null!, GetOptions());

        await entityManager.CreateAsync("domain.sensor", new EntityCreationOptions(PayloadAvailable: "up"));
        var payload = PayloadToDictionary(mqttSetup.LastPublishedMessage.PayloadSegment.Array ?? Array.Empty<byte>());

        payload?.ContainsKey("availability_topic").Should().BeTrue();
        payload?["availability_topic"].ToString().Should().Be("homeassistant/domain/sensor/availability");
        payload?["payload_available"].ToString().Should().Be("up");
    }

    [Fact]
    public async Task CreateAvailabilityTopicSetForAvailDown()
    {
        var mqttSetup = new MockMqttMessageSenderSetup();
        var entityManager = new MqttEntityManager(mqttSetup.MessageSender, null!, GetOptions());

        await entityManager.CreateAsync("domain.sensor", new EntityCreationOptions(PayloadNotAvailable: "down"));
        var payload = PayloadToDictionary(mqttSetup.LastPublishedMessage.PayloadSegment.Array ?? Array.Empty<byte>());

        payload?.ContainsKey("availability_topic").Should().BeTrue();
        payload?["availability_topic"].ToString().Should().Be("homeassistant/domain/sensor/availability");
        payload?["payload_not_available"].ToString().Should().Be("down");
    }

    [Fact]
    public async Task CanRemove()
    {
        var mqttSetup = new MockMqttMessageSenderSetup();
        var entityManager = new MqttEntityManager(mqttSetup.MessageSender, null!, GetOptions());

        await entityManager.RemoveAsync("domain.sensor");

        mqttSetup.LastPublishedMessage.PayloadSegment.Should().BeEmpty();
    }

    [Fact]
    public async Task CanSetState()
    {
        var mqttSetup = new MockMqttMessageSenderSetup();
        var entityManager = new MqttEntityManager(mqttSetup.MessageSender, null!, GetOptions());

        await entityManager.SetStateAsync("domain.sensor", "NewState");
        var payload = Encoding.Default.GetString(mqttSetup.LastPublishedMessage.PayloadSegment.Array ?? Array.Empty<byte>());

        mqttSetup.LastPublishedMessage.Topic.Should().Be("homeassistant/domain/sensor/state");
        payload.Should().Be("NewState");
    }

    [Fact]
    public async Task CanSetStateToBlank()
    {
        var mqttSetup = new MockMqttMessageSenderSetup();
        var entityManager = new MqttEntityManager(mqttSetup.MessageSender, null!, GetOptions());

        await entityManager.SetStateAsync("domain.sensor", "");

        mqttSetup.LastPublishedMessage.Topic.Should().Be("homeassistant/domain/sensor/state");
        mqttSetup.LastPublishedMessage.PayloadSegment.Should().BeEmpty();
    }

    [Fact]
    public async Task CanSetAttributes()
    {
        var mqttSetup = new MockMqttMessageSenderSetup();
        var entityManager = new MqttEntityManager(mqttSetup.MessageSender, null!, GetOptions());

        var attributes = new { colour = "purple", ziggy = "stardust" };
        await entityManager.SetAttributesAsync("domain.sensor", attributes);
        var payload = PayloadToDictionary(mqttSetup.LastPublishedMessage.PayloadSegment.Array ?? Array.Empty<byte>());

        mqttSetup.LastPublishedMessage.Topic.Should().Be("homeassistant/domain/sensor/attributes");
        payload?["colour"].ToString().Should().Be("purple");
        payload?["ziggy"].ToString().Should().Be("stardust");
    }

    [Fact]
    public async Task CanSetAvailability()
    {
        var mqttSetup = new MockMqttMessageSenderSetup();
        var entityManager = new MqttEntityManager(mqttSetup.MessageSender, null!, GetOptions());

        await entityManager.SetAvailabilityAsync("domain.sensor", "up");
        var payload = Encoding.Default.GetString(mqttSetup.LastPublishedMessage.PayloadSegment.Array ?? Array.Empty<byte>());

        mqttSetup.LastPublishedMessage.Topic.Should().Be("homeassistant/domain/sensor/availability");
        payload.Should().Be("up");
    }


    private static Dictionary<string, object>? PayloadToDictionary(byte[] payload)
    {
        return JsonSerializer.Deserialize<Dictionary<string, object>>(
            Encoding.Default.GetString(payload)
        );
    }

    private static IOptions<MqttConfiguration> GetOptions()
    {
        var options = new Mock<IOptions<MqttConfiguration>>();

        options.Setup(o => o.Value)
            .Returns(() => new MqttConfiguration
            {
                Host = "localhost", 
                UserName = "id"
            });

        return options.Object;
    }
}
