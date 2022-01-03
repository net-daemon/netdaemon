using System;
using System.Reactive.Subjects;
using System.Threading;
using FluentAssertions;
using JoySoftware.HomeAssistant.Client;
using JoySoftware.HomeAssistant.Model;
using Moq;
using NetDaemon.HassModel.Internal.HassClient;
using Xunit;

namespace NetDaemon.HassModel.Tests.Internal
{
    public class EntityStateCacheTest
    {
        [Fact]
        public async void StateChangeEventIsFirstStoredInCacheThanForwarded()
        {
            var entityId = "sensor.test";

            // Arrange
            using var testSubject = new Subject<HassEvent>();
            var hassClientMock = new Mock<IHassClient>();

            hassClientMock.Setup(m => m.GetAllStates(CancellationToken.None)).ReturnsAsync(new HassState[]
            {
                new () { EntityId = entityId, State = "InitialState" }
            });

            using var cache = new EntityStateCache(hassClientMock.Object, testSubject);

            var stateChangeObserverMock = new Mock<IObserver<HassStateChangedEventData>>();
            cache.StateAllChanges.Subscribe(stateChangeObserverMock.Object);

            // ACT 1: after initialization of the cache it should show the values retieved from Hass
            await cache.InitializeAsync(CancellationToken.None);

            cache.GetState(entityId)!.State.Should().Be("InitialState", because: "The initial value should be available");

            // Act 2: now fire a state change event
            var changedEventData = new HassStateChangedEventData()
            {
                EntityId = entityId,
                OldState = new HassState(),
                NewState = new HassState()
                {
                    State = "newState"
                }
            };

            stateChangeObserverMock.Setup(m => m.OnNext(It.IsAny<HassStateChangedEventData>()))
                .Callback(() =>
                {
#pragma warning disable 8602
                    cache.GetState(entityId).State.Should().Be("newState");
#pragma warning restore 8602
                });

            // Act
            testSubject.OnNext(new HassEvent
            {
                Data = changedEventData
            });

            // Assert
            stateChangeObserverMock.Verify(m => m.OnNext(It.IsAny<HassStateChangedEventData>()));
            cache.GetState(entityId)!.State.Should().Be("newState");
        }

        [Fact]
        public async void AllEntityIds_returnsInitialPlusChangedEntities()
        {
            // Arrange
            using var testSubject = new Subject<HassEvent>();
            var hassClientMock = new Mock<IHassClient>();

            hassClientMock.Setup(m => m.GetAllStates(CancellationToken.None)).ReturnsAsync(new HassState[]
            {
                new () { EntityId = "sensor.sensor1", State = "InitialState" }
            });

            using var cache = new EntityStateCache(hassClientMock.Object, testSubject);

            var stateChangeObserverMock = new Mock<IObserver<HassStateChangedEventData>>();
            cache.StateAllChanges.Subscribe(stateChangeObserverMock.Object);

            // ACT 1: after initialization of the cache it should show the values retieved from Hass
            await cache.InitializeAsync(CancellationToken.None);

            // Act 2: now fire a state change event
            var changedEventData = new HassStateChangedEventData()
            {
                EntityId = "sensor.sensor2",
                OldState = new HassState(),
                NewState = new HassState()
                {
                    State = "newState"
                }
            };

            // Act
            testSubject.OnNext(new HassEvent { Data = changedEventData });

            // Assert
            cache.AllEntityIds.Should().BeEquivalentTo("sensor.sensor1", "sensor.sensor2");
        }

    }
}