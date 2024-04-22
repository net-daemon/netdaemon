using System.Reactive.Subjects;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Client;
using NetDaemon.Client.HomeAssistant.Model;
using NetDaemon.Client.Internal.HomeAssistant.Commands;
using NetDaemon.HassModel.Entities;
using NetDaemon.HassModel.Internal;
using NetDaemon.HassModel.Tests.TestHelpers;

namespace NetDaemon.HassModel.Tests.Internal;

public sealed class AppScopedHaContextProviderTest : IDisposable
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
    public async Task TestCallServiceWithResponseAsync()
    {
        var haContext = await CreateTargetAsync();

        var serviceTarget = ServiceTarget.FromEntity("domain.entity");
        var serviceData = new { Name = "value" };

        await haContext.CallServiceWithResponseAsync("domain", "service", serviceTarget, serviceData);

        // The following expected structure to be called by the underlying connection

        // {
        //     Id = 0,
        //     Type = "execute_script",
        //     Sequence = new object[]
        //     {
        //         new
        //         {
        //             service = "domain.service",
        //             data = serviceData,
        //             target = serviceTarget,
        //             response_variable = "service_result"
        //         },
        //         new
        //         {
        //             stop = "done",
        //             response_variable = "service_result"
        //         }
        //     }
        // }

        // Hack since verify did not work on that complex object
        var result = _hassConnectionMock.Invocations.Single(e =>
            e.Method.Name == "SendCommandAndReturnResponseAsync" &&
            e.Arguments[0] is CallExecuteScriptCommand);

        var executeCommand = result.Arguments[0] as CallExecuteScriptCommand;

        executeCommand!.Sequence[0].GetType().GetProperty("service")!.GetValue(executeCommand.Sequence[0])!.Should().Be("domain.service");
        executeCommand.Sequence[0].GetType().GetProperty("data")!.GetValue(executeCommand.Sequence[0])!.Should().BeEquivalentTo(serviceData);
        executeCommand.Sequence[0].GetType().GetProperty("target")!.GetValue(executeCommand.Sequence[0])!.Should().BeEquivalentTo(serviceTarget);
    }

    [Fact]
    public async Task TestStateChange()
    {
        var haContext = await CreateTargetAsync();
        var stateAllChangesObserverMock = new Mock<IObserver<IStateChange>>();
        var stateChangesObserverMock = new Mock<IObserver<IStateChange>>();

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

        await ((IAsyncDisposable)serviceScope).DisposeAsync();

        // Assert
        eventObserverMock.Verify(e => e.OnNext(It.IsAny<Event>()), Times.Once);
        var @event = eventObserverMock.Invocations[0].Arguments[0] as Event;
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

        await ((IAsyncDisposable)serviceScope).DisposeAsync();

        // Assert
        typedEventObserverMock.Verify(e => e.OnNext(It.IsAny<Event<TestEventData>>()), Times.Once);
        typedEventObserverMock.Verify(e => e.OnCompleted(), Times.Once);
        var @event = (Event<TestEventData>)typedEventObserverMock.Invocations[0].Arguments[0];

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
        _hassConnectionMock.Setup(n =>
                n.SubscribeToHomeAssistantEventsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_hassEventSubjectMock
            );
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

    public void Dispose()
    {
       _hassEventSubjectMock.Dispose();
       GC.SuppressFinalize(this);
    }
}
