using System.Reactive.Subjects;
using Microsoft.Extensions.Logging.Abstractions;
using NetDaemon.Client;
using NetDaemon.Client.HomeAssistant.Model;
using NetDaemon.Client.Internal.HomeAssistant.Commands;
using NetDaemon.HassModel.Internal;

namespace NetDaemon.HassModel.Tests.Registry;

public class RegistryNavigationTest
{
    private readonly Mock<IHomeAssistantConnection> _connectionMock;

    public RegistryNavigationTest()
    {
        _connectionMock = new Mock<IHomeAssistantConnection>();
        _connectionMock.Setup(m => m.SubscribeToHomeAssistantEventsAsync(null, It.IsAny<CancellationToken>())).ReturnsAsync(new Subject<HassEvent>());
    }

    private async Task<HaRegistry> InitializeCacheAndBuildRegistry()
    {
        var runnerMock = new Mock<IHomeAssistantRunner>();
        runnerMock.SetupGet(m => m.CurrentConnection).Returns(_connectionMock.Object);

        var cache = new RegistryCache(runnerMock.Object, new NullLogger<RegistryCache>());
        await cache.InitializeAsync(CancellationToken.None);

        var haContextMock = new Mock<IHaContext> { CallBase = true };
        return new HaRegistry(haContextMock.Object, cache);
    }

    private void InitializeDataModel()
    {
        SetupCommandResult("config/entity_registry/list",
            [
                new HassEntity { EntityId = "light.mb_nightlight", AreaId = "master_bedroom"},
                new HassEntity { EntityId = "light.babyroom_nightlight", AreaId = "baby_room", Labels = ["stay_on"] },
                new HassEntity { EntityId = "sensor.babyroom_humidity", DeviceId = "24:AB:1B:9A"},
                new HassEntity { EntityId = "light.desk_lamp", AreaId = "study", Options = new HassEntityOptions()
                    {
                        Conversation = new HassEntityConversationOptions()
                        {
                            ShouldExpose = true
                        }
                    }
                },
            ]);

        SetupCommandResult("config/device_registry/list",
            [
                new HassDevice { Id = "24:AB:1B:9A", Name = "SomeSensor" , AreaId = "baby_room"}
            ]);

        SetupCommandResult("config/area_registry/list",
            [
                new HassArea { Name = "Master Bedroom", Id = "master_bedroom", FloorId = "upstairs", Labels = ["bedroom"] },
                new HassArea { Name = "Baby room", Id = "baby_room", FloorId = "upstairs", Labels = ["bedroom"] },

                new HassArea { Name = "Study", Id = "study", FloorId = "attic" },
                new HassArea { Name = "Storage", Id = "storage", FloorId = "attic" },
            ]);

        SetupCommandResult("config/floor_registry/list",
            [
                new HassFloor { Id = "downstairs", Name = "DownStairs", Level = 0 },
                new HassFloor { Id = "upstairs", Name = "Upstairs", Level = 1 },
                new HassFloor { Id = "attic", Name = "Attic", Level = 2 },
            ]
        );
        SetupCommandResult("config/label_registry/list",
            [
                new HassLabel { Id = "bedroom", Name = "Bedroom", Description = "Areas that serve as bedrooms" },
                new HassLabel { Id = "stay_on", Name = "Stay On", Description = "Lights that should stay on at night" },
            ]);
    }

    [Fact]
    public async Task TestNavigateModel()
    {
        // Setup:
        InitializeDataModel();

        // Act:
        var registry = await InitializeCacheAndBuildRegistry();

        // Assert, navigate the model

        registry.Entities.Should().BeEquivalentTo(
        [
            new { Id = "light.mb_nightlight" },
            new { Id = "light.babyroom_nightlight" },
            new { Id = "sensor.babyroom_humidity" },
            new { Id = "light.desk_lamp"}
        ]);

        registry.Devices.Should().BeEquivalentTo(
        [
            new { Name = "SomeSensor" },
        ]);

        registry.Areas.Should().BeEquivalentTo(
        [
            new { Name = "Master Bedroom" },
            new { Name = "Baby room" },
            new { Name = "Study" },
            new { Name = "Storage" },
        ]);

        registry.Floors.Should().BeEquivalentTo(
        [
            new { Name = "DownStairs" },
            new { Name = "Upstairs" },
            new { Name = "Attic" },
        ]);

        registry.Labels.Should().BeEquivalentTo(
        [
            new { Name = "Bedroom" },
            new { Name = "Stay On" },
        ]);

        registry.GetFloor("attic")!.Areas.Should().BeEquivalentTo([
            new { Name = "Study" },
            new { Name = "Storage" },
        ]);

        registry.GetLabel("stay_on")!.Entities.Should().BeEquivalentTo(
        [
            new { EntityId = "light.babyroom_nightlight" }
        ]);


        registry.GetLabel("stay_on")!.Entities.Should().Contain(e => e.EntityId == "light.babyroom_nightlight");

        registry.GetEntityRegistration("light.mb_nightlight")!.Area!.Name.Should().Be("Master Bedroom");

        registry.GetDevice("24:AB:1B:9A")!.Entities.Should().BeEquivalentTo(
        [
            new { EntityId = "sensor.babyroom_humidity" }
        ]);

        var area = registry.GetArea("baby_room")!;

        area.Devices.Should().BeEquivalentTo([
            new { Name = "SomeSensor" }
        ]);
        area.Entities.Should().BeEquivalentTo([
            new { EntityId = "light.babyroom_nightlight" },
            new { EntityId = "sensor.babyroom_humidity" },
        ]);

    }

    [Fact]
    public async Task TestConversationOptions()
    {
        // Setup:
        InitializeDataModel();

        // Act:
        var registry = await InitializeCacheAndBuildRegistry();

        registry.GetEntityRegistration("light.desk_lamp")!
            .Options!
            .ConversationOptions!
            .ShouldExpose
            .Should()
            .BeTrue();
    }

        private void SetupCommandResult<TResult>(string command, IReadOnlyCollection<TResult> result)
    {
        _connectionMock.Setup(m => m.SendCommandAndReturnResponseAsync<SimpleCommand, IReadOnlyCollection<TResult>>(
            new SimpleCommand(command), It.IsAny<CancellationToken>())).ReturnsAsync(result);
    }
}
