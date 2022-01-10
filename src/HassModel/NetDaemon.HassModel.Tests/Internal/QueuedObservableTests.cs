using System;
using NetDaemon.Infrastructure.ObservableHelpers;
using System.Reactive.Subjects;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using NetDaemon.HassModel.Tests.TestHelpers;
using System.Threading.Tasks;
using FluentAssertions;
using NetDaemon.HassModel.Common;

namespace NetDaemon.HassModel.Tests.Internal
{
    public class QueuedObservableTests
    {
        [Fact]
        public async Task WhenScopeIsDisposedSubscribersAreDetached()
        {
            var testSubject = new Subject<string>();
            var loggerMock = new Mock<ILogger<IHaContext>>();
            // Create 2 ScopedObservables for the same subject
            var scoped1 = new QueuedObservable<string>(loggerMock.Object);
            scoped1.Initialize(testSubject);
            var scoped2 = new QueuedObservable<string>(loggerMock.Object);
            scoped2.Initialize(testSubject);

            // First scope has 2 subscribers, second has 1
            var scope1AObserverMock = new Mock<IObserver<string>>();
            var scope1BObserverMock = new Mock<IObserver<string>>();
            scoped1.Subscribe(scope1AObserverMock.Object);
            scoped1.Subscribe(scope1BObserverMock.Object);

            var scope2ObserverMock = new Mock<IObserver<string>>();
            scoped2.Subscribe(scope2ObserverMock.Object);

            var waitTasks = new Task[]
            {
                scope1AObserverMock.WaitForInvocationAndVerify(o => o.OnNext("Event1")),
                scope1BObserverMock.WaitForInvocationAndVerify(o => o.OnNext("Event1")),
                scope2ObserverMock.WaitForInvocationAndVerify(o => o.OnNext("Event1"))
            };
            
            // Now start firing events
            testSubject.OnNext("Event1");
            await Task.WhenAll(waitTasks);
            
            await scoped1.DisposeAsync();
            testSubject.OnNext("Event2");

            scope1AObserverMock.Verify(o => o.OnNext("Event2"), Times.Never, "Event should not reach Observer of disposed scope");
            scope1BObserverMock.Verify(o => o.OnNext("Event2"), Times.Never, "Event should not reach Observer of disposed scope");
            scope2ObserverMock.Verify(o => o.OnNext("Event2"), Times.Once);

            await scoped2.DisposeAsync();
            testSubject.OnNext("Event3");
            scope1AObserverMock.Verify(o => o.OnNext("Event3"), Times.Never, "Event should not reach Observer of disposed scope");
            scope1BObserverMock.Verify(o => o.OnNext("Event3"), Times.Never, "Event should not reach Observer of disposed scope");
            scope2ObserverMock.Verify(o => o.OnNext("Event3"), Times.Never, "Event should not reach Observer of disposed scope");
        }
        [Fact]
        public async Task TestQueuedObservableShouldHaveFinishedTasksOnDispose()
        {
            var source = new Subject<int>();
            var queue = new QueuedObservable<int>(Mock.Of<ILogger<IHaContext>>());
            queue.Initialize(source);
            var subscriber = new Mock<IObserver<int>>();
            queue.Subscribe(subscriber.Object);
            
            // It is not enough that DisposeAsync waits on task
            // since it can be aborted by the cancellation token                                                                                                             
            var waitOnCallTask = subscriber.WaitForInvocation(n => n.OnNext(1));
            source.OnNext(1);
            await waitOnCallTask.ConfigureAwait(false);

            await queue.DisposeAsync().ConfigureAwait(false);
            subscriber.Verify(s => s.OnNext(1));
        }
        
        [Fact]
        public async Task TestQueuedObservableShouldLogOnException()
        {
            var source = new Subject<int>();
            var loggerMock = new Mock<ILogger<IHaContext>>();
            var queue = new QueuedObservable<int>(loggerMock.Object);
            queue.Initialize(source);
            var subscriber = new Mock<IObserver<int>>();
            subscriber.Setup(n => n.OnNext(1)).Throws<InvalidOperationException>();
            queue.Subscribe(subscriber.Object);
            
            source.OnNext(1);

            await queue.DisposeAsync().ConfigureAwait(false);
            // Verify that an error has been logged
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((_, __) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((_, _) => true)), Times.Once);
        }        
        
        [Fact]
        public async Task TestQueuedObservableShouldStillHaveSubscribersOnException()
        {
            var source = new Subject<int>();
            var loggerMock = new Mock<ILogger<IHaContext>>();
            var queue = new QueuedObservable<int>(loggerMock.Object);
            queue.Initialize(source);
            var subscriber = new Mock<IObserver<int>>();
            subscriber.Setup(n => n.OnNext(1)).Throws<InvalidOperationException>();
            queue.Subscribe(subscriber.Object);

            var waitOnCallTask = subscriber.WaitForInvocation(n => n.OnNext(1));
            source.OnNext(1);
            await waitOnCallTask.ConfigureAwait(false);
            source.HasObservers.Should().BeTrue();
            
            await queue.DisposeAsync().ConfigureAwait(false);
            source.HasObservers.Should().BeFalse();
            
        }
    }
}