using System;
using System.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NetDaemon.Client.Common;
using NetDaemon.Client.Common.HomeAssistant.Model;
using NetDaemon.Client.Internal.HomeAssistant.Commands;
using NetDaemon.HassModel.Common;
using NetDaemon.HassModel.Entities;
using NetDaemon.HassModel.Tests.TestHelpers;
using NetDaemon.Infrastructure.ObservableHelpers;
using Xunit;

namespace NetDaemon.HassModel.Tests.Internal;

public class AppScopedHaContextProviderUsingClientTest
{
    private readonly Mock<IHomeAssistantConnection> _hassConnectionMock = new();
    private readonly Subject<HassEvent> _hassEventSubjectMock = new();

    private readonly HassEvent _sampleHassEvent = new()
    {
        Origin = "Test",
        EventType = "test_event",
        TimeFired = new DateTime(2020, 12, 2),
        DataElement = JsonSerializer.Deserialize<JsonElement>(@"{""command"" : ""flip"",
                                                                     ""endpoint_id"" : 2}")
    };

    [Fact]
    public async void TestCallService()
    {
        var haContext = await CreateTargetAsync();

        var target = ServiceTarget.FromEntity("domain.entity");
        var data = new { Name = "value" };
        haContext.CallService("domain", "service", target, data);

        var expectedCommand = new CallServiceCommand
        {
            Domain = "domain",
            Service = "service",
            ServiceData = data,
            Target = new HassTarget
            {
                EntityIds = target.EntityIds
            }
        };
        _hassConnectionMock.Verify(
            c => c.SendCommandAndReturnResponseAsync<CallServiceCommand, object>(expectedCommand,
                It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TestStateChange()
    {
        var haContext = await CreateTargetAsync();
        var stateAllChangesObserverMock = new Mock<IObserver<StateChange>>();
        var stateChangesObserverMock = new Mock<IObserver<StateChange>>();

        haContext.StateAllChanges().Subscribe(stateAllChangesObserverMock.Object);
        haContext.StateChanges().Subscribe(stateChangesObserverMock.Object);

        _hassEventSubjectMock.OnNext(new HassEvent
        {
            EventType = "state_changed",
            DataElement = @"
                    {
                        ""entity_id"": ""TestDomain.TestEntity"",
                        ""new_state"": {
                            ""state"": ""newState""
                        },
                        ""old_state"": {
                            ""state"": ""oldState""
                        } 
                    }
                    ".AsJsonElement()
        });

        haContext.GetState("TestDomain.TestEntity")!.State!.Should().Be("newState");
        // the state should come from the state cache so we do not expect a call to HassClient.GetState 
    }


    [Fact]
    public async Task Events_PassesMappedEvents()
    {
        // Arrange
        var haContext = await CreateTargetAsync();
        Mock<IObserver<Event>> eventObserverMock = new();

        haContext.Events.Subscribe(eventObserverMock.Object);

        // Act
        _hassEventSubjectMock.OnNext(_sampleHassEvent);
        await Task.Yield(); // make sure other tasks run before we assert
        // Assert
        eventObserverMock.Verify(e => e.OnNext(It.IsAny<Event>()));
        var @event = eventObserverMock.Invocations.Single().Arguments[0] as Event;
        @event!.Origin.Should().Be(_sampleHassEvent.Origin);
        @event.EventType.Should().Be(_sampleHassEvent.EventType);
        @event.TimeFired.Should().Be(_sampleHassEvent.TimeFired);
        @event.DataElement.Should().Be(_sampleHassEvent.DataElement);
    }

    [Fact]
    public async Task EventsAndFilter_ShowsOnlyMatchingEventsAsCorrectType()
    {
        // Arrange
        var haContext = await CreateTargetAsync();
        Mock<IObserver<Event<TestEventData>>> typedEventObserverMock = new();

        haContext.Events.Filter<TestEventData>("test_event").Subscribe(typedEventObserverMock.Object);

        _hassEventSubjectMock.OnNext(_sampleHassEvent);
        _hassEventSubjectMock.OnNext(_sampleHassEvent with { EventType = "other_type" });

        await Task.Yield(); // make sure other tasks run before we assert 

        // Assert
        typedEventObserverMock.Verify(e => e.OnNext(It.IsAny<Event<TestEventData>>()), Times.Once);
        var @event = (Event<TestEventData>)typedEventObserverMock.Invocations.Single().Arguments[0];

        @event.Data!.command.Should().Be("flip");
        @event.Data!.endpoint_id.Should().Be(2);
        @event.Data!.otherField.Should().BeNull("it is not in the Json");
    }

    [Fact]
    public async Task EventsStopAfterDispose()
    {
        // Arrange
        var haContext = await CreateTargetAsync();
        Mock<IObserver<Event>> eventObserverMock = new();
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

    private async Task<IHaContext> CreateTargetAsync()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddSingleton(_hassConnectionMock.Object);
        serviceCollection.AddSingleton<IObservable<HassEvent>>(_hassEventSubjectMock);

        var haRunnerMock = new Mock<IHomeAssistantRunner>();

        haRunnerMock.SetupGet(n => n.CurrentConnection).Returns(_hassConnectionMock.Object);
        serviceCollection.AddSingleton(n => haRunnerMock.Object);

        var apiManagerMock = new Mock<IHomeAssistantApiManager>();

        serviceCollection.AddSingleton(n => apiManagerMock.Object);
        serviceCollection.AddScopedHaContext2();

        var provider = serviceCollection.BuildServiceProvider();
        await provider.GetRequiredService<ICacheManager>().InitializeAsync(CancellationToken.None);
        
        serviceCollection.AddScoped<NonQueuedObservableMock<HassEvent>>();
        serviceCollection.AddScoped<IQueuedObservable<HassEvent>>(s => s.GetRequiredService<NonQueuedObservableMock<HassEvent>>());
        var haContext = provider.GetRequiredService<IHaContext>();
        
        return haContext;
    }

    public record TestEventData(string command, int endpoint_id, string otherField);
}