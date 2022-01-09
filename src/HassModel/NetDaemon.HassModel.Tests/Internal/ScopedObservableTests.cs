﻿using System;
using NetDaemon.Infrastructure.ObservableHelpers;
using System.Reactive.Subjects;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using NetDaemon.HassModel.Tests.TestHelpers;
using System.Threading.Tasks;

namespace NetDaemon.HassModel.Tests.Internal
{
    public class ScopedObservableTests
    {
        [Fact]
        public void WhenScopeIsDisposedSubscribersAreDetached()
        {
            var testSubject = new Subject<string>();
            var loggerMock = new Mock<ILogger>();
            // Create 2 ScopedObservables for the same subject
            var scoped1 = new ScopedObservable<string>(testSubject, loggerMock.Object);
            var scoped2 = new ScopedObservable<string>(testSubject, loggerMock.Object);

            // First scope has 2 subscribers, second has 1
            var scope1AObserverMock = new Mock<IObserver<string>>();
            var scope1BObserverMock = new Mock<IObserver<string>>();
            scoped1.Subscribe(scope1AObserverMock.Object);
            scoped1.Subscribe(scope1BObserverMock.Object);

            var scope2ObserverMock = new Mock<IObserver<string>>();
            scoped2.Subscribe(scope2ObserverMock.Object);

            // Now start firing events
            testSubject.OnNext("Event1");
            scope1AObserverMock.Verify(o => o.OnNext("Event1"), Times.Once);
            scope1BObserverMock.Verify(o => o.OnNext("Event1"), Times.Once);
            scope2ObserverMock.Verify(o => o.OnNext("Event1"), Times.Once);

            scoped1.Dispose();
            testSubject.OnNext("Event2");

            scope1AObserverMock.Verify(o => o.OnNext("Event2"), Times.Never, "Event should not reach Observer of disposed scope");
            scope1BObserverMock.Verify(o => o.OnNext("Event2"), Times.Never, "Event should not reach Observer of disposed scope");
            scope2ObserverMock.Verify(o => o.OnNext("Event2"), Times.Once);

            scoped2.Dispose();
            testSubject.OnNext("Event3");
            scope1AObserverMock.Verify(o => o.OnNext("Event3"), Times.Never, "Event should not reach Observer of disposed scope");
            scope1BObserverMock.Verify(o => o.OnNext("Event3"), Times.Never, "Event should not reach Observer of disposed scope");
            scope2ObserverMock.Verify(o => o.OnNext("Event3"), Times.Never, "Event should not reach Observer of disposed scope");
        }
    }
}