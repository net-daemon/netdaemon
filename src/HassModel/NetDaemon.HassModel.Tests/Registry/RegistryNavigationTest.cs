using System.Reactive.Subjects;
using Microsoft.Extensions.Logging.Abstractions;
using NetDaemon.Client;
using NetDaemon.Client.HomeAssistant.Model;
using NetDaemon.Client.Internal.HomeAssistant.Commands;
using NetDaemon.HassModel.Internal;

namespace NetDaemon.HassModel.Tests.Registry;

public class RegistryNavigationTest
{
    private Mock<IHomeAssistantConnection> _connectionMock;

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

        var haContextMock = new Mock<IHaContext>();
        return new HaRegistry(haContextMock.Object, cache);
    }

    [Fact]
    public async Task TestNavigateModel()
    {
        SetupCommandResult("config/entity_registry/list",
            [
                new HassEntity { EntityId = "light.mb_nightlight", AreaId = "master_bedroom" },
                new HassEntity { EntityId = "light.babyroom_nightlight", AreaId = "baby_room", Labels = ["stay_on"] },
            ]);

        SetupCommandResult("config/device_registry/list",
            [
                new HassDevice { Id = "24:AB:1B:9A", Name = "" }
            ]);

        SetupCommandResult("config/area_registry/list",
            [
                new HassArea { Name = "Master Bedroom", Id = "master_bedroom", FloorId = "upstairs", Labels = ["bedroom"] },
                new HassArea { Name = "Baby room", Id = "baby room", FloorId = "upstairs", Labels = ["bedroom"] },

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


        // Act:
        var registry = await InitializeCacheAndBuildRegistry();

        // Assert, navigate the model

        registry.Floors.Should().BeEquivalentTo(
        [
            new { Name = "DownStairs" },
            new { Name = "Upstairs" },
            new { Name = "Attic" },
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
    }

    private  void SetupCommandResult<TResult>(string command, IReadOnlyCollection<TResult> result)
    {
        _connectionMock.Setup(m => m.SendCommandAndReturnResponseAsync<SimpleCommand, IReadOnlyCollection<TResult>>(
            new SimpleCommand(command), It.IsAny<CancellationToken>())).ReturnsAsync(result);
    }
}
