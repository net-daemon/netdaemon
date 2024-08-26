using System.Reactive.Subjects;
using NetDaemon.Client;
using NetDaemon.Client.HomeAssistant.Model;
using NetDaemon.Client.Internal.HomeAssistant.Commands;
using NetDaemon.HassModel.Tests.TestHelpers;
using NetDaemon.Client.Internal.Extensions;
using NetDaemon.HassModel.Internal;

namespace NetDaemon.HassModel.Tests.Internal;

public class EntityStateCacheTest
{
    [Fact]
    public async Task StateChangeEventIsFirstStoredInCacheThanForwarded()
    {
        var entityId = "sensor.test";

        // Arrange
        using var testSubject = new Subject<HassEvent>();
        var hassConnectionMock = new Mock<IHomeAssistantConnection>();
        var haRunnerMock = new Mock<IHomeAssistantRunner>();

        haRunnerMock.SetupGet(n => n.CurrentConnection).Returns(hassConnectionMock.Object);

        hassConnectionMock
            .Setup(m => m.SendCommandAndReturnResponseAsync<SimpleCommand, IReadOnlyCollection<HassState>>
                    (It.IsAny<SimpleCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new HassState {EntityId = entityId, State = "InitialState"}]);

        hassConnectionMock
            .Setup(n => n.SubscribeToHomeAssistantEventsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(testSubject);

        using var cache = new EntityStateCache(haRunnerMock.Object);

        var eventObserverMock = new Mock<IObserver<HassEvent>>();
        cache.AllEvents.Subscribe(eventObserverMock.Object);

        // ACT 1: after initialization of the cache it should show the values retrieved from Hass
        await cache.InitializeAsync(CancellationToken.None);

        cache.GetState(entityId)!.State.Should().Be("InitialState", "The initial value should be available");

        // Act 2: now fire a state change event
        var changedEventData = new
        {
            entity_id = entityId,
            old_state = new { },
            new_state = new
            {
                state = "newState",
                context = new
                {
                    id = "contextId",
                    parent_id = "parentId"
                }
            }
        };

        eventObserverMock.Setup(m => m.OnNext(It.IsAny<HassEvent>()))
            .Callback(() =>
            {
#pragma warning disable 8602
                cache.GetState(entityId).State.Should().Be("newState", because: "The cache should already have the new value when the event handler runs");
#pragma warning restore 8602
            });

        // Act
        testSubject.OnNext(new HassEvent
        {
            EventType = "state_changed",
            DataElement = changedEventData.AsJsonElement()
        });

        // Assert
        eventObserverMock.Verify(m => m.OnNext(It.IsAny<HassEvent>()));
        cache.GetState(entityId)!.State.Should().Be("newState");
        cache.GetState(entityId)!.Context!.Id.Should().Be("contextId");
        cache.GetState(entityId)!.Context!.ParentId.Should().Be("parentId");
    }

    [Fact]
    public async Task AllEntityIds_returnsInitialPlusChangedEntities()
    {
        // Arrange
        using var testSubject = new Subject<HassEvent>();
        var hassConnectionMock = new Mock<IHomeAssistantConnection>();
        var haRunnerMock = new Mock<IHomeAssistantRunner>();

        haRunnerMock.SetupGet(n => n.CurrentConnection).Returns(hassConnectionMock.Object);

        hassConnectionMock
            .Setup(m => m.SendCommandAndReturnResponseAsync<SimpleCommand, IReadOnlyCollection<HassState>>
                (It.IsAny<SimpleCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new()
                {
                    EntityId = "sensor.sensor1",
                    State = "InitialState",
                    AttributesJson = new { brightness = 100 }.ToJsonElement(),
                }]);

        hassConnectionMock.Setup(n =>
                n.SubscribeToHomeAssistantEventsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(testSubject);

        using var cache = new EntityStateCache(haRunnerMock.Object);

        var stateChangeObserverMock = new Mock<IObserver<HassEvent>>();
        cache.AllEvents.Subscribe(stateChangeObserverMock.Object);

        // ACT 1: after initialization of the cache it should show the values retieved from Hass
        await cache.InitializeAsync(CancellationToken.None);

        // initial value for sensor.sensor1 shoul be visible right away
        cache.GetState("sensor.sensor1")!.AttributesJson.GetValueOrDefault().GetProperty("brightness").GetInt32().Should().Be(100);

        // Act 2: now fire 2 state change events
        testSubject.OnNext(new HassEvent
        {
            EventType = "state_changed",
            DataElement = new
            {
                entity_id = "sensor.sensor1",
                old_state = new { },
                new_state = new
                {
                    state = "newState",
                    context = new
                    {
                        id = "contextId"
                    },
                    attributes = new {brightness = 200}
                }
            }.AsJsonElement()
        });

        testSubject.OnNext(new HassEvent
        {
            EventType = "state_changed",
            DataElement = new
            {
                entity_id = "sensor.sensor2",
                old_state = new { },
                new_state = new
                {
                    state = "newState",
                    context = new
                    {
                        id = "contextId"
                    },
                    attributes = new {brightness = 300}
                }
            }.AsJsonElement()
        });

        // Assert
        cache.AllEntityIds.Should().BeEquivalentTo("sensor.sensor1", "sensor.sensor2");
        cache.GetState("sensor.sensor1")!.AttributesJson.GetValueOrDefault().GetProperty("brightness").GetInt32().Should().Be(200);
        cache.GetState("sensor.sensor1")!.Context!.Id.Should().NotBeNullOrEmpty();
        cache.GetState("sensor.sensor2")!.AttributesJson.GetValueOrDefault().GetProperty("brightness").GetInt32().Should().Be(300);
        cache.GetState("sensor.sensor2")!.Context!.Id.Should().Be("contextId");
    }
}
