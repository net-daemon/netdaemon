using System.Reactive.Subjects;
using System.Text.Json;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Client;
using NetDaemon.Client.HomeAssistant.Model;
using NetDaemon.Client.Internal.HomeAssistant.Commands;
using NetDaemon.HassModel.Entities;
using NetDaemon.HassModel.Internal;
using NetDaemon.HassModel.Tests.TestHelpers;

namespace NetDaemon.HassModel.Tests.Internal;

public class AppScopedHaContextProviderTest
{
    private readonly Mock<IHomeAssistantConnection> _hassConnectionMock = new();
    private readonly Subject<HassEvent> _hassEventSubjectMock = new();

    private readonly HassEvent _sampleHassEvent = new()
    {
        Origin = "Test",
        EventType = "test_event",
        TimeFired = new DateTime(2020, 12, 2),
        DataElement = JsonSerializer.Deserialize<JsonElement>(@"{""command"" : ""flip"", ""endpoint_id"" : 2}")
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
            c => c.SendCommandAsync<CallServiceCommand>(expectedCommand,
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
        var provider = await CreateServiceProvider();
        var serviceScope = provider.CreateScope();

        var haContext = serviceScope.ServiceProvider.GetRequiredService<IHaContext>();

        Mock<IObserver<Event>> eventObserverMock = new();

        haContext.Events.Subscribe(eventObserverMock.Object);

        // Act
        _hassEventSubjectMock.OnNext(_sampleHassEvent);
        _hassEventSubjectMock.OnCompleted();
        
        await ((IAsyncDisposable)serviceScope).DisposeAsync().ConfigureAwait(false);
        
        // Assert
        eventObserverMock.Verify(e => e.OnNext(It.IsAny<Event>()), Times.Once);
        var @event = eventObserverMock.Invocations.First().Arguments[0] as Event;
        @event!.Origin.Should().Be(_sampleHassEvent.Origin);
        @event.EventType.Should().Be(_sampleHassEvent.EventType);
        @event.TimeFired.Should().Be(_sampleHassEvent.TimeFired);
        @event.DataElement.Should().Be(_sampleHassEvent.DataElement);
    }

    [Fact]
    public async Task EventsAndFilter_ShowsOnlyMatchingEventsAsCorrectType()
    {
        // Arrange
        var provider = await CreateServiceProvider();
        var serviceScope = provider.CreateScope();

        var haContext = serviceScope.ServiceProvider.GetRequiredService<IHaContext>();
        Mock<IObserver<Event<TestEventData>>> typedEventObserverMock = new();

        haContext.Events.Filter<TestEventData>("test_event").Subscribe(typedEventObserverMock.Object);

        _hassEventSubjectMock.OnNext(_sampleHassEvent);
        _hassEventSubjectMock.OnNext(_sampleHassEvent with { EventType = "other_type" });
        _hassEventSubjectMock.OnCompleted();

        await ((IAsyncDisposable)serviceScope).DisposeAsync().ConfigureAwait(false);

        // Assert
        typedEventObserverMock.Verify(e => e.OnNext(It.IsAny<Event<TestEventData>>()), Times.Once);
        typedEventObserverMock.Verify(e => e.OnCompleted(), Times.Once);
        var @event = (Event<TestEventData>)typedEventObserverMock.Invocations.First().Arguments[0];

        @event.Data!.command.Should().Be("flip");
        @event.Data!.endpoint_id.Should().Be(2);
        @event.Data!.otherField.Should().BeNull("it is not in the Json");
    }

    [Fact]
    public async Task EventsStopAfterDispose()
    {
        // Arrange
        var provider = await CreateServiceProvider();
        var serviceScope = provider.CreateScope();

        var haContext = serviceScope.ServiceProvider.GetRequiredService<IHaContext>();
        
        Mock<IObserver<Event>> eventObserverMock = new();
        haContext.Events.Subscribe(eventObserverMock.Object);

        // Act
        _hassEventSubjectMock.OnNext(_sampleHassEvent);
        _hassEventSubjectMock.OnCompleted();

        await ((IAsyncDisposable)serviceScope).DisposeAsync();
        eventObserverMock.Verify(m => m.OnNext(It.Is<Event>(e => e.Origin == "Test")));
        eventObserverMock.Verify(m => m.OnCompleted());


        // Act
        _hassEventSubjectMock.OnNext(_sampleHassEvent);

        // Assert
        eventObserverMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task TestThatCallServiceTrackBackgroundTask()
    {
        var provider = await CreateServiceProvider();

        var haContext = provider.CreateScope().ServiceProvider.GetRequiredService<IHaContext>();
        var backgroundTrackerMock = provider.GetRequiredService<Mock<IBackgroundTaskTracker>>();
        var target = ServiceTarget.FromEntity("domain.entity");
        var data = new { Name = "value" };

        haContext.CallService("domain", "service", target, data);

        backgroundTrackerMock.Verify(n => n.TrackBackgroundTask(It.IsAny<Task?>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task TestThatSendEventTrackBackgroundTask()
    {
        var provider = await CreateServiceProvider();

        var haContext = provider.CreateScope().ServiceProvider.GetRequiredService<IHaContext>();
        var backgroundTrackerMock = provider.GetRequiredService<Mock<IBackgroundTaskTracker>>();

        haContext.SendEvent("any_type", null);

        backgroundTrackerMock.Verify(n => n.TrackBackgroundTask(It.IsAny<Task?>(), It.IsAny<string>()), Times.Once);
    }

    private async Task<IHaContext> CreateTargetAsync()
    {
        var provider = await CreateServiceProvider();
        var haContext = provider.CreateScope().ServiceProvider.GetRequiredService<IHaContext>();
        
        return haContext;
    }

    private async Task<ServiceProvider> CreateServiceProvider()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();


        serviceCollection.AddSingleton(_hassConnectionMock.Object);
        serviceCollection.AddSingleton<IObservable<HassEvent>>(_hassEventSubjectMock);

        var haRunnerMock = new Mock<IHomeAssistantRunner>();

        haRunnerMock.SetupGet(n => n.CurrentConnection).Returns(_hassConnectionMock.Object);
        serviceCollection.AddSingleton(_ => haRunnerMock.Object);

        var apiManagerMock = new Mock<IHomeAssistantApiManager>();

        serviceCollection.AddSingleton(_ => apiManagerMock.Object);
        serviceCollection.AddScopedHaContext();

        var backgroundTaskTrackerMock = new Mock<IBackgroundTaskTracker>();
        serviceCollection.AddScoped<Mock<IBackgroundTaskTracker>>(_=> backgroundTaskTrackerMock);
        serviceCollection.AddScoped(_ => backgroundTaskTrackerMock.Object);

        var provider = serviceCollection.BuildServiceProvider();

        await provider.GetRequiredService<ICacheManager>().InitializeAsync(CancellationToken.None);

        return provider;
    }

    public record TestEventData(string command, int endpoint_id, string otherField);
}
