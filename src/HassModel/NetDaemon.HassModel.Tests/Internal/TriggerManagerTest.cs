using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Text.Json;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Client;
using NetDaemon.Client.Common.HomeAssistant.Model;
using NetDaemon.Client.HomeAssistant.Model;
using NetDaemon.Client.Internal.HomeAssistant.Commands;
using NetDaemon.HassModel.Tests.TestHelpers;

namespace NetDaemon.HassModel.Tests.Internal;

public sealed class TriggerManagerTest : IDisposable
{
    private readonly ITriggerManager _triggerManager;

    private readonly Mock<IHomeAssistantConnection> _hassConnectionMock = new();
    private readonly Subject<HassMessage> _messageSubject = new();

    private int nextMessageId = 5;

    public TriggerManagerTest()
    {
        _hassConnectionMock.Setup(m => m.SendCommandAndReturnHassMessageResponseAsync(
                It.IsAny<SubscribeTriggerCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new HassMessage { Id = nextMessageId });

        _hassConnectionMock.Setup(m => m.SendCommandAndReturnHassMessageResponseAsync(
                It.IsAny<UnsubscribeEventsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new HassMessage { Id = nextMessageId });

        _hassConnectionMock
            .As<IHomeAssistantHassMessages>()
            .SetupGet(m => m.OnHassMessage)
            .Returns(_messageSubject);

        var provider = CreateServiceProvider();
        _triggerManager = provider.GetRequiredService<ITriggerManager>();
    }


    private ServiceProvider CreateServiceProvider()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddScopedHaContext();

        var haRunnerMock = new Mock<IHomeAssistantRunner>();

        haRunnerMock.SetupGet(n => n.CurrentConnection).Returns(_hassConnectionMock.Object);
        serviceCollection.AddSingleton(_ => haRunnerMock.Object);

        var provider = serviceCollection.BuildServiceProvider();

        return provider;
    }


    [Fact]
    public async Task RegisterTrigger()
    {
        var incomingTriggersTask = _triggerManager.RegisterTrigger(new {}).FirstAsync().ToTask().ToFunc();

        var message = new { }.AsJsonElement();

        _messageSubject.OnNext(new HassMessage(){Id = nextMessageId, Event = new HassEvent(){Variables = new HassVariable()
            {TriggerElement = message }}});

        // Assert
        await incomingTriggersTask.Should()
            .CompleteWithinAsync(TimeSpan.FromSeconds(1), "the message should have arrived by now")
            .WithResult(message);
    }

    [Fact]
    public async Task NoMoreTriggersAfterDispose()
    {
        // Act
        var incomingTriggersTask = _triggerManager.RegisterTrigger(new {}).FirstAsync().ToTask().ToFunc();

        await ((IAsyncDisposable)_triggerManager).DisposeAsync();

        // Assert, Dispose should unsubscribe with HA AND stop any messages that do pass

        _messageSubject.OnNext(new HassMessage(){Id = nextMessageId, Event = new HassEvent(){Variables = new HassVariable()
            {TriggerElement = new JsonElement() }}});

        await incomingTriggersTask.Should()
            .NotCompleteWithinAsync(TimeSpan.FromSeconds(1), "no messages should arrive after being disposed");

        _hassConnectionMock.Verify(m => m.SendCommandAndReturnHassMessageResponseAsync(
            new UnsubscribeEventsCommand(nextMessageId), It.IsAny<CancellationToken>()));
    }


    [Fact]
    public async Task RegisterTriggerCorrectMessagesPerSubscription()
    {
        nextMessageId = 6;
        var incomingTriggersTask6 = _triggerManager.RegisterTrigger(new {}).FirstAsync().ToTask().ToFunc();

        nextMessageId = 7;
        var incomingTriggersTask7 = _triggerManager.RegisterTrigger(new {}).FirstAsync().ToTask().ToFunc();

        var message6 = new { tag = "six" }.AsJsonElement();
        var message7 = new { tag = "seven" }.AsJsonElement();

        _messageSubject.OnNext(new HassMessage{Id = 6, Event = new HassEvent(){Variables = new HassVariable()
            {TriggerElement = message6 }}});


        _messageSubject.OnNext(new HassMessage{Id = 7, Event = new HassEvent(){Variables = new HassVariable()
            {TriggerElement = message7 }}});

        // Assert
        await incomingTriggersTask6.Should()
            .CompleteWithinAsync(TimeSpan.FromSeconds(1), $"{nameof(message6)} should have arrived by now")
            .WithResult(message6);

        await incomingTriggersTask7.Should()
            .CompleteWithinAsync(TimeSpan.FromSeconds(1), $"{nameof(message7)} should have arrived by now")
            .WithResult(message7);
    }

    public void Dispose()
    {
        _messageSubject.Dispose();
    }
}
