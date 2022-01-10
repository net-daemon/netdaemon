using System;
using System.Reactive.Subjects;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using NetDaemon.HassModel.Common;
using NetDaemon.Infrastructure.ObservableHelpers;
using Xunit;

namespace NetDaemon.HassModel.Tests.Internal;

public class QueuedObservabeTest
{
    [Fact]
    public async void EventsSouldbeforwarded()
    {
        var source = new Subject<int>();

        var queue = new QueuedObservable<int>(Mock.Of<ILogger<IHaContext>>());
        queue.Initialize(source);

        var subscriber = new Mock<IObserver<int>>();
        queue.Subscribe(subscriber.Object);
        source.OnNext(1);
        source.OnNext(2);
        source.OnNext(3);
        await queue.DisposeAsync().ConfigureAwait(false);
            
        subscriber.Verify(s => s.OnNext(1), Times.Once);
        subscriber.Verify(s => s.OnNext(2), Times.Once);
        subscriber.Verify(s => s.OnNext(3), Times.Once);
    }
    
    [Fact]
    public async void SubscribersShouldNotBlockEachOther()
    {
        var source = new Subject<int>();

        var queue1 = new QueuedObservable<int>(Mock.Of<ILogger<IHaContext>>());
        queue1.Initialize(source);

        var subscriber = new Mock<IObserver<int>>();
        queue1.Subscribe(subscriber.Object);
        
        // The second subscriber will block the first event
        var queue2 = new QueuedObservable<int>(Mock.Of<ILogger<IHaContext>>());
        queue2.Initialize(source);
        var blockSubscribers = new ManualResetEvent(false);
        queue2.Subscribe(_ => blockSubscribers.WaitOne());

        // now send three events
        source.OnNext(1);
        source.OnNext(2);
        source.OnNext(3);
        await queue1.DisposeAsync().ConfigureAwait(false);

        // all events should reach the first subscriber
        subscriber.Verify(s => s.OnNext(1), Times.Once);
        subscriber.Verify(s => s.OnNext(2), Times.Once);
        subscriber.Verify(s => s.OnNext(3), Times.Once);

        blockSubscribers.Set();
    }    
}