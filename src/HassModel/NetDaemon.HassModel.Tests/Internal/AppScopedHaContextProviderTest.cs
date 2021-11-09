using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using System.Threading;
using FluentAssertions;
using JoySoftware.HomeAssistant.Client;
using JoySoftware.HomeAssistant.Model;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NetDaemon.HassModel.Common;
using NetDaemon.HassModel.Entities;
using Xunit;

namespace NetDaemon.HassModel.Tests.Internal
{
    public class AppScopedHaContextProviderTest
    {
        private readonly Subject<HassEvent> _hassEventSubjectMock = new ();
        private readonly Mock<IHassClient> _hassClientMock = new();

        private readonly HassEvent _sampleHassEvent = new ()
        {
            Origin = "Test",
            EventType = "test_event",
            TimeFired = new DateTime(2020, 12, 2),
            DataElement = JsonSerializer.Deserialize<JsonElement>(@"{""command"" : ""flip"",
                                                                     ""endpoint_id"" : 2}")
        };

        [Fact]
        public void TestCallService()
        {
            var haContext = CreateTarget();

            var target = ServiceTarget.FromEntity("domain.entity");
            var data = new { Name = "value" };
            haContext.CallService("domain", "service", target, data);
            
            _hassClientMock.Verify(c => c.CallService("domain", "service", data, It.Is<HassTarget>(t => t.EntityIds.Single() == "domain.entity"), false), Times.Once);
        }

        [Fact]
        public void TestStateChange()
        {
            var haContext = CreateTarget();
            var stateAllChangesObserverMock = new Mock<IObserver<StateChange>>();
            var stateChangesObserverMock = new Mock<IObserver<StateChange>>();

            haContext.StateAllChanges().Subscribe(stateAllChangesObserverMock.Object);
            haContext.StateChanges().Subscribe(stateChangesObserverMock.Object);
            
            _hassEventSubjectMock.OnNext(new HassEvent()
            {
                EventType = "state_change",
                Data = new HassStateChangedEventData()
                {
                   EntityId = "TestDomain.TestEntity",
                   NewState = new HassState(){ State = "newState" },
                   OldState = new HassState(){ State = "oldState" }
                }
            });

            stateAllChangesObserverMock.Verify(o => o.OnNext(It.Is<StateChange>(s => s.Entity.EntityId == "TestDomain.TestEntity")), Times.Once);
            stateChangesObserverMock.Verify(o => o.OnNext(It.Is<StateChange>(s => s.Entity.EntityId == "TestDomain.TestEntity")), Times.Once());

            haContext.GetState("TestDomain.TestEntity")!.State!.Should().Be("newState");
            // the state should come from the state cache so we do not expect a call to HassClient.GetState 
            _hassClientMock.Verify(m => m.GetState(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Events_PassesMappedEvents()
        {
            // Arrange
            var haContext = CreateTarget();
            Mock<IObserver<Event>> eventObserverMock = new ();

            haContext.Events.Subscribe(eventObserverMock.Object);
            
            // Act
            _hassEventSubjectMock.OnNext(_sampleHassEvent);
            
            // Assert
            eventObserverMock.Verify(e => e.OnNext(It.IsAny<Event>()));
            var @event = eventObserverMock.Invocations.Single().Arguments[0] as Event;
            @event!.Origin.Should().Be(_sampleHassEvent.Origin);
            @event!.EventType.Should().Be(_sampleHassEvent.EventType);
            @event!.TimeFired.Should().Be(_sampleHassEvent.TimeFired);
            @event!.DataElement.Should().Be(_sampleHassEvent.DataElement);
        }

        [Fact]
        public void EventsAndFilter_ShowsOnlyMatchingEventsAsCorrectType()
        {
            // Arrange
            var haContext = CreateTarget();
            Mock<IObserver<Event<TestEventData>>> typedEventObserverMock = new ();

            haContext.Events.Filter<TestEventData>("test_event").Subscribe(typedEventObserverMock.Object);
            
            // Act
            _hassEventSubjectMock.OnNext(_sampleHassEvent);
            _hassEventSubjectMock.OnNext(_sampleHassEvent with { EventType = "other_type" });
            
            // Assert
            typedEventObserverMock.Verify(e => e.OnNext(It.IsAny<Event<TestEventData>>()), Times.Once);
            var @event = (Event<TestEventData>)typedEventObserverMock.Invocations.Single().Arguments[0];
            
            @event.Data!.command.Should().Be("flip");
            @event.Data!.endpoint_id.Should().Be(2);
            @event.Data!.otherField.Should().BeNull(because: "it is not in the Json");
        }        
        
        [Fact]
        public void EventsStopAfterDispose()
        {
            // Arrange
            var haContext = CreateTarget();
            Mock<IObserver<Event>> eventObserverMock = new ();
            haContext.Events.Subscribe(eventObserverMock.Object);
            
            // Act
            _hassEventSubjectMock.OnNext(_sampleHassEvent);
            
            eventObserverMock.Verify(m => m.OnNext(It.Is<Event>(e => e.Origin == "Test")));

            // Act
            ((IDisposable)haContext).Dispose();
            _hassEventSubjectMock.OnNext(_sampleHassEvent);

            // Assert
            eventObserverMock.VerifyNoOtherCalls();
        }        
        
        public record TestEventData(string command, int endpoint_id, string otherField);

        private IHaContext CreateTarget()
        {
            var serviceCollection = new ServiceCollection();

            _hassClientMock.Setup(m => m.GetAllStates(It.IsAny<CancellationToken>())).ReturnsAsync(new List<HassState>());

            serviceCollection.AddSingleton(_hassClientMock.Object);
            serviceCollection.AddSingleton<IObservable<HassEvent>>(_hassEventSubjectMock);
            serviceCollection.AddScopedHaContext();
            
            var provider = serviceCollection.BuildServiceProvider();
            DependencyInjectionSetup.InitializeAsync(provider, CancellationToken.None);

            var haContext = provider.GetRequiredService<IHaContext>();
            return haContext;
        }
    }
}