using System.Reactive.Subjects;
using Microsoft.Extensions.Logging.Abstractions;
using NetDaemon.Client;
using NetDaemon.Client.HomeAssistant.Model;
using NetDaemon.Client.Internal.HomeAssistant.Commands;
using NetDaemon.HassModel.Entities;
using NetDaemon.HassModel.Internal;

namespace NetDaemon.HassModel.Tests.Registry;

public class RegistryNavigationTest
{
    [Fact]
    public async Task TestNavigateModel()
    {
        var connectionMock = new Mock<IHomeAssistantConnection>();
        connectionMock.Setup(m => m.SubscribeToHomeAssistantEventsAsync(null, It.IsAny<CancellationToken>())).ReturnsAsync(new Subject<HassEvent>());

        connectionMock.Setup(m => m.SendCommandAndReturnResponseAsync<SimpleCommand, IReadOnlyCollection<HassEntity>>(
            new SimpleCommand("config/entity_registry/list"), It.IsAny<CancellationToken>())).ReturnsAsync(
            [
                new HassEntity { EntityId = "light.mb_nightlight", AreaId = "master_bedroom" },
                new HassEntity { EntityId = "light.babyroom_nightlight", AreaId = "baby_room", Labels = ["stay_on"] },
            ]
        );

        connectionMock.Setup(m => m.SendCommandAndReturnResponseAsync<SimpleCommand, IReadOnlyCollection<HassDevice>>(
            new SimpleCommand("config/device_registry/list"), It.IsAny<CancellationToken>())).ReturnsAsync(
            [
                new HassDevice { Id = "24:AB:1B:9A", Name = "" }
            ]
        );

        connectionMock.Setup(m => m.SendCommandAndReturnResponseAsync<SimpleCommand, IReadOnlyCollection<HassArea>>(
            new SimpleCommand("config/area_registry/list"), It.IsAny<CancellationToken>())).ReturnsAsync(
            [
                new HassArea { Name = "Master Bedroom", Id = "master_bedroom", FloorId = "upstairs", Labels = ["bedroom"] },
                new HassArea { Name = "Baby room", Id = "baby room", FloorId = "upstairs", Labels = ["bedroom"] },

                new HassArea { Name = "Study", Id = "study", FloorId = "attic" },
                new HassArea { Name = "Storage", Id = "storage", FloorId = "attic" },
            ]
        );

        connectionMock.Setup(m => m.SendCommandAndReturnResponseAsync<SimpleCommand, IReadOnlyCollection<HassFloor>>(
            new SimpleCommand("config/floor_registry/list"), It.IsAny<CancellationToken>())).ReturnsAsync(
            [
                new HassFloor() { Id = "downstairs", Name = "DownStairs", Level = 0 },
                new HassFloor() { Id = "upstairs", Name = "Upstairs", Level = 1 },
                new HassFloor() { Id = "attic", Name = "Attic", Level = 2 },
            ]
        );
        connectionMock.Setup(m => m.SendCommandAndReturnResponseAsync<SimpleCommand, IReadOnlyCollection<HassLabel>>(
            new SimpleCommand("config/label_registry/list"), It.IsAny<CancellationToken>())).ReturnsAsync(
            [
                new HassLabel { Id = "bedroom", Name = "Bedroom", Description = "Areas that serve as bedrooms" },
                new HassLabel { Id = "stay_on", Name = "Stay On", Description = "Lights that should stay on at night" },
            ]
        );


        var runnerMock = new Mock<IHomeAssistantRunner>();
        runnerMock.SetupGet(m => m.CurrentConnection).Returns(connectionMock.Object);
        var cache = new RegistryCache(runnerMock.Object, new NullLogger<RegistryCache>());

        await cache.InitializeAsync(CancellationToken.None).ConfigureAwait(false);
        var haContextMock = new Mock<IHaContext>();
        var registry = new HaRegistry(haContextMock.Object, cache);

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
    }
}
