using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Reactive.Testing;
using FluentAssertions;
using Moq;
using Xunit;

using NetDaemon.Extensions.Scheduler;

namespace NetDaemon.Extensions.Scheduling.Tests
{
    /// <summary>
    ///     Tests the scheduling features of the scheduler
    /// </summary>
    public class SchedulerExtensionTest
    {
        [Fact]
        public void TestRunInCallsFunction()
        {
            // ARRANGE
            var testScheduler = new TestScheduler();
            bool isCalled = false;
            var netDaemonScheduler = new NetDaemonScheduler(reactiveScheduler: testScheduler);
            netDaemonScheduler.RunIn(TimeSpan.FromMinutes(1), () => isCalled = true);

            // ACT
            testScheduler.AdvanceBy(TimeSpan.FromMinutes(1).Ticks);

            // ASSERT
            Assert.True(isCalled);
        }

        [Fact]
        public void TestRunInShouldNotCallFunctionIfNotDue()
        {
            // ARRANGE
            var testScheduler = new TestScheduler();
            bool isCalled = false;
            var netDaemonScheduler = new NetDaemonScheduler(reactiveScheduler: testScheduler);
            netDaemonScheduler.RunIn(TimeSpan.FromMinutes(1), () => isCalled = true);

            // ACT
            testScheduler.AdvanceBy(TimeSpan.FromSeconds(59).Ticks);

            // ASSERT
            Assert.False(isCalled);
        }

        [Fact]
        public void TestRunInLogsException()
        {
            // ARRANGE
            var testScheduler = new TestScheduler();
            var loggerMock = new Mock<ILogger<NetDaemonScheduler>>();

            var netDaemonScheduler = new NetDaemonScheduler(loggerMock.Object, reactiveScheduler: testScheduler);
            netDaemonScheduler.RunIn(TimeSpan.FromMinutes(1), () => throw new Exception("hello"));

            // ACT
            testScheduler.AdvanceBy(TimeSpan.FromMinutes(1).Ticks);

            // ASSERT that error is logged once
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((_, __) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((_, _) => true)), Times.Once);
        }

        [Fact]
        public void TestRunAtCallsFunction()
        {
            // ARRANGE
            var testScheduler = new TestScheduler();

            // sets the date to a specific time so we do not get errors in UTC
            var dueTime = new DateTime(2021, 1, 1, 0, 0, 0);
            testScheduler.AdvanceTo(dueTime.Ticks);

            bool isCalled = false;
            var netDaemonScheduler = new NetDaemonScheduler(reactiveScheduler: testScheduler);

            netDaemonScheduler.RunAt(testScheduler.Now.AddHours(1), () => isCalled = true);

            // ACT and ASSERT
            testScheduler.AdvanceBy(TimeSpan.FromMinutes(30).Ticks);
            Assert.False(isCalled);
            testScheduler.AdvanceBy(TimeSpan.FromMinutes(30).Ticks);
            Assert.True(isCalled);
        }

        [Fact]
        public void TestRunAtLogsException()
        {
            // ARRANGE
            var testScheduler = new TestScheduler();
            var loggerMock = new Mock<ILogger<NetDaemonScheduler>>();
            // sets the date to a specific time so we do not get errors in UTC
            var dueTime = new DateTime(2021, 1, 1, 0, 0, 0);
            testScheduler.AdvanceTo(dueTime.Ticks);

            var netDaemonScheduler = new NetDaemonScheduler(loggerMock.Object, reactiveScheduler: testScheduler);

            netDaemonScheduler.RunAt(testScheduler.Now.AddHours(1), () => throw new Exception("hello"));

            // ACT and ASSERT
            testScheduler.AdvanceBy(TimeSpan.FromHours(1).Ticks);
            // ASSERT that error is logged once
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((_, __) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((_, _) => true)), Times.Once);
        }

        [Fact]
        public void TestRunEveryCallsCorrectNrOfTimes()
        {
            // ARRANGE
            var testScheduler = new TestScheduler();
            // sets the date to a specific time so we do not get errors in UTC
            var dueTime = new DateTime(2021, 1, 1, 0, 0, 0);
            testScheduler.AdvanceTo(dueTime.Ticks);

            int nrOfTimesCalled = 0;
            var netDaemonScheduler = new NetDaemonScheduler(reactiveScheduler: testScheduler);

            netDaemonScheduler.RunEvery(TimeSpan.FromSeconds(1), () => nrOfTimesCalled++);

            // ACT and ASSERT
            testScheduler.AdvanceBy(TimeSpan.FromSeconds(5).Ticks);
            Assert.Equal(5, nrOfTimesCalled);
        }

        [Fact]
        public void TestRunEveryLogsException()
        {
            // ARRANGE
            var testScheduler = new TestScheduler();
            var loggerMock = new Mock<ILogger<NetDaemonScheduler>>();
            // sets the date to a specific time so we do not get errors in UTC
            var dueTime = new DateTime(2021, 1, 1, 0, 0, 0);
            testScheduler.AdvanceTo(dueTime.Ticks);

            var netDaemonScheduler = new NetDaemonScheduler(loggerMock.Object, reactiveScheduler: testScheduler);

            netDaemonScheduler.RunEvery(TimeSpan.FromSeconds(1), () => throw new Exception("hello"));

            // ACT and ASSERT
            testScheduler.AdvanceBy(TimeSpan.FromSeconds(5).Ticks);
            // ASSERT that error is logged once
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((_, __) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((_, _) => true)), Times.Exactly(5));
        }

        [Fact]
        public void TestRunEveryCallsCorrectNrOfTimesUsingForwardTime()
        {
            // ARRANGE
            var testScheduler = new TestScheduler();
            // sets the date to a specific time so we do not get errors in UTC
            var dueTime = new DateTime(2021, 1, 1, 0, 0, 0);
            testScheduler.AdvanceTo(dueTime.Ticks);

            int nrOfTimesCalled = 0;
            var netDaemonScheduler = new NetDaemonScheduler(reactiveScheduler: testScheduler);

            netDaemonScheduler.RunEvery(TimeSpan.FromSeconds(1), testScheduler.Now.AddSeconds(2), () => nrOfTimesCalled++);

            // ACT and ASSERT
            testScheduler.AdvanceBy(TimeSpan.FromSeconds(5).Ticks);
            Assert.Equal(4, nrOfTimesCalled);
        }

        [Fact]
        public void TestRunEveryLogsExceptionUsingForwardTime()
        {
            // ARRANGE
            var testScheduler = new TestScheduler();
            var loggerMock = new Mock<ILogger<NetDaemonScheduler>>();
            // sets the date to a specific time so we do not get errors in UTC
            var dueTime = new DateTime(2021, 1, 1, 0, 0, 0);
            testScheduler.AdvanceTo(dueTime.Ticks);

            var netDaemonScheduler = new NetDaemonScheduler(loggerMock.Object, reactiveScheduler: testScheduler);

            netDaemonScheduler.RunEvery(TimeSpan.FromSeconds(1), testScheduler.Now.AddSeconds(2), () => throw new Exception("hello"));

            // ACT and ASSERT
            testScheduler.AdvanceBy(TimeSpan.FromSeconds(5).Ticks);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((_, __) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((_, _) => true)), Times.Exactly(4));
        }

        [Fact]
        public void TestNowIsCorrectTime()
        {
            // ARRANGE
            var testScheduler = new TestScheduler();
            // sets the date to a specific time 
            var dueTime = new DateTime(2021, 1, 1, 0, 0, 0);
            testScheduler.AdvanceTo(dueTime.Ticks);

            var netDaemonScheduler = new NetDaemonScheduler(reactiveScheduler: testScheduler);

            // ACT & ASSERT
            Assert.Equal(testScheduler.Now, netDaemonScheduler.Now);
        }
    }
}