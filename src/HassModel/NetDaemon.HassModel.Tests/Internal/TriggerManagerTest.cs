using System.Reactive.Subjects;
using System.Text.Json;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Client;
using NetDaemon.Client.Common.HomeAssistant.Model;
using NetDaemon.Client.HomeAssistant.Model;
using NetDaemon.Client.Internal.HomeAssistant.Commands;
using NetDaemon.HassModel.Tests.TestHelpers;

namespace NetDaemon.HassModel.Tests.Internal;

public class TriggerManagerTest
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
    public void RegisterTrigger()
    {
        var incomingTriggersMock = _triggerManager.RegisterTrigger(new {}).SubscribeMock();

        var message = new { }.AsJsonElement();

        _messageSubject.OnNext(new HassMessage(){Id = nextMessageId, Event = new HassEvent(){Variables = new HassVariable()
            {TriggerElement = message }}});

        // Assert
        incomingTriggersMock.Verify(e => e.OnNext(message));
    }
    
    [Fact]
    public async void NoMoreTriggersAfterDispose()
    {
        // Act
        var incomingTriggersMock = _triggerManager.RegisterTrigger(new {}).SubscribeMock();

        await ((IAsyncDisposable)_triggerManager).DisposeAsync().ConfigureAwait(false);
        
        // Assert, Dispose should unsubscribe with HA AND stop any messages that do pass        
        _hassConnectionMock.Verify(m => m.SendCommandAndReturnHassMessageResponseAsync(
            new UnsubscribeEventsCommand(nextMessageId), It.IsAny<CancellationToken>()));

        _messageSubject.OnNext(new HassMessage(){Id = nextMessageId, Event = new HassEvent(){Variables = new HassVariable()
            {TriggerElement = new JsonElement() }}});

        incomingTriggersMock.VerifyNoOtherCalls();
    }    
    

    [Fact]
    public void RegisterTriggerCorrectMessagesPerSubscription()
    {
        nextMessageId = 6;
        var incomingTriggersMock6 = _triggerManager.RegisterTrigger(new {}).SubscribeMock();

        nextMessageId = 7;
        var incomingTriggersMock7 = _triggerManager.RegisterTrigger(new {}).SubscribeMock();
        
        var message6 = new { tag = "six" }.AsJsonElement();
        var message7 = new { tag = "seven" }.AsJsonElement();

        // Assert
        _messageSubject.OnNext(new HassMessage{Id = 6, Event = new HassEvent(){Variables = new HassVariable()
            {TriggerElement = message6 }}});
        

        _messageSubject.OnNext(new HassMessage{Id = 7, Event = new HassEvent(){Variables = new HassVariable()
            {TriggerElement = message7 }}});

        incomingTriggersMock6.Verify(e => e.OnNext(message6), Times.Once);
        incomingTriggersMock7.Verify(e => e.OnNext(message7), Times.Once);
    }

    
}