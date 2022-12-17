using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Threading;
using NetDaemon.Client;
using NetDaemon.Client.HomeAssistant.Model;
using NetDaemon.Client.Internal.HomeAssistant.Commands;
using NetDaemon.HassModel.Tests.TestHelpers;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.HassModel.Internal;

namespace NetDaemon.HassModel.Tests.Internal;

public class EntityStateCacheTest
{
    [Fact]
    public async void StateChangeEventIsFirstStoredInCacheThanForwarded()
    {
        var entityId = "sensor.test";

        // Arrange
        using var testSubject = new Subject<HassEvent>();
        var _hassConnectionMock = new Mock<IHomeAssistantConnection>();
        var haRunnerMock = new Mock<IHomeAssistantRunner>();

        haRunnerMock.SetupGet(n => n.CurrentConnection).Returns(_hassConnectionMock.Object);

        _hassConnectionMock.Setup(
                m => m.SendCommandAndReturnResponseAsync<SimpleCommand, IReadOnlyCollection<HassState>>
                (
                    It.IsAny<SimpleCommand>(), It.IsAny<CancellationToken>()
                ))
            .ReturnsAsync(new List<HassState>
            {
                new() {EntityId = entityId, State = "InitialState"}
            });

        var serviceColletion = new ServiceCollection();
        _ = serviceColletion.AddTransient<IObservable<HassEvent>>(_ => testSubject);
        var sp = serviceColletion.BuildServiceProvider();

        using var cache = new EntityStateCache(haRunnerMock.Object, sp);

        var eventObserverMock = new Mock<IObserver<HassEvent>>();
        cache.AllEvents.Subscribe(eventObserverMock.Object);

        // ACT 1: after initialization of the cache it should show the values retieved from Hass
        await cache.InitializeAsync(CancellationToken.None);

        cache.GetState(entityId)!.State.Should().Be("InitialState", "The initial value should be available");

        // Act 2: now fire a state change event
        var changedEventData = new HassStateChangedEventData
        {
            EntityId = entityId,
            OldState = new HassState(),
            NewState = new HassState
            {
                State = "newState"
            }
        };

        eventObserverMock.Setup(m => m.OnNext(It.IsAny<HassEvent>()))
            .Callback(() =>
            {
#pragma warning disable 8602
                cache.GetState(entityId).State.Should().Be("newState");
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
    }

    [Fact]
    public async void AllEntityIds_returnsInitialPlusChangedEntities()
    {
        // Arrange
        using var testSubject = new Subject<HassEvent>();
        var _hassConnectionMock = new Mock<IHomeAssistantConnection>();
        var haRunnerMock = new Mock<IHomeAssistantRunner>();

        haRunnerMock.SetupGet(n => n.CurrentConnection).Returns(_hassConnectionMock.Object);

        _hassConnectionMock.Setup(
                m => m.SendCommandAndReturnResponseAsync<SimpleCommand, IReadOnlyCollection<HassState>>
                (
                    It.IsAny<SimpleCommand>(), It.IsAny<CancellationToken>()
                ))
            .ReturnsAsync(new List<HassState>
            {
                new() {EntityId = "sensor.sensor1", State = "InitialState"}
            });

        var serviceColletion = new ServiceCollection();
        _ = serviceColletion.AddTransient<IObservable<HassEvent>>(_ => testSubject);
        var sp = serviceColletion.BuildServiceProvider();

        using var cache = new EntityStateCache(haRunnerMock.Object, sp);

        var stateChangeObserverMock = new Mock<IObserver<HassEvent>>();
        cache.AllEvents.Subscribe(stateChangeObserverMock.Object);

        // ACT 1: after initialization of the cache it should show the values retieved from Hass
        await cache.InitializeAsync(CancellationToken.None);

        // Act 2: now fire a state change event
        var changedEventData = new HassStateChangedEventData
        {
            EntityId = "sensor.sensor2",
            OldState = new HassState(),
            NewState = new HassState
            {
                State = "newState"
            }
        };

        // Act
        testSubject.OnNext(new HassEvent
        {
            EventType = "state_changed",
            DataElement = changedEventData.AsJsonElement()
        });

        // Assert
        cache.AllEntityIds.Should().BeEquivalentTo("sensor.sensor1", "sensor.sensor2");
    }
}