using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NetDaemon.Daemon.Fakes;
using Xunit;

namespace NetDaemon.Daemon.Tests.Reactive
{
    /// <summary>
    ///     Tests the fluent API parts of the daemon
    /// </summary>
    /// <remarks>
    ///     Mainly the tests checks if correct underlying call to "CallService"
    ///     has been made.
    /// </remarks>
    public class RxSchedulerTest : CoreDaemonHostTestBase
    {
        [Fact]
        [SuppressMessage("", "CA2201")]
        public async Task CreateObservableIntervallFailureShouldLogError()
        {
            // ARRANGE
            await using var app = new BaseTestRxApp();
            await app.StartUpAsync(DefaultDaemonHost).ConfigureAwait(false);
            app.IsEnabled = true;

            // ACT
            using var disposable = app.CreateObservableIntervall(TimeSpan.FromMilliseconds(10), () => throw new Exception("hello!"));

            await Task.Delay(150).ConfigureAwait(false);
            // ASSERT
            LoggerMock.AssertLogged(LogLevel.Error, Times.AtLeastOnce());
        }

        [Fact]
        public async Task CreateObservableIntervallShouldCallFunction()
        {
            // ARRANGE
            await using var app = new BaseTestRxApp
            {
                IsEnabled = true
            };

            var called = false;

            // ACT
            using var disposable = app.CreateObservableIntervall(TimeSpan.FromMilliseconds(10), () => called = true);

            await Task.Delay(150).ConfigureAwait(false);
            // ASSERT
            Assert.True(called);
        }

        [Fact]
        [SuppressMessage("", "CA2201")]
        public async Task CreateObservableTimerFailureShouldLogError()
        {
            // ARRANGE
            await using var app = new BaseTestRxApp();
            await app.StartUpAsync(DefaultDaemonHost).ConfigureAwait(false);
            app.IsEnabled = true;

            // ACT
            using var disposable = app.CreateObservableTimer(DateTime.Now, TimeSpan.FromMilliseconds(10), () => throw new Exception("Hello!"));

            await Task.Delay(150).ConfigureAwait(false);
            // ASSERT
            LoggerMock.AssertLogged(LogLevel.Error, Times.AtLeastOnce());
        }

        [Fact]
        public async Task CreateObservableTimerShouldCallFunction()
        {
            // ARRANGE
            await using var app = new BaseTestRxApp
            {
                IsEnabled = true
            };

            var called = false;

            // ACT
            using var disposable = app.CreateObservableTimer(DateTime.Now, TimeSpan.FromMilliseconds(10), () => called = true);

            await Task.Delay(100).ConfigureAwait(false);
            // ASSERT
            Assert.True(called);
        }

        [Fact]
        public void RunDailyOneHourAfterShouldCallCreateObservableIntervall()
        {
            // ARRANGE
            var time = DateTime.Now;
            var timeOneHourBack = time.AddHours(1);
            var timeFormat = timeOneHourBack.ToString("HH:mm:ss", CultureInfo.InvariantCulture);

            // ACT
            DefaultMockedRxApp.Object.RunDaily(timeFormat, () => Console.WriteLine("Test"));

            // ASSERT
            DefaultMockedRxApp.Verify(n => n.CreateObservableTimer(It.IsAny<DateTime>(), TimeSpan.FromDays(1), It.IsAny<Action>()), Times.Once());
        }

        [Fact]
        public void RunDailyOneHourBeforeShouldCallCreateObservableIntervall()
        {
            // ARRANGE
            var time = DateTime.Now;
            var timeOneHourBack = time.Subtract(TimeSpan.FromHours(1));
            var timeFormat = timeOneHourBack.ToString("HH:mm:ss", CultureInfo.InvariantCulture);

            // ACT
            DefaultMockedRxApp.Object.RunDaily(timeFormat, () => System.Console.WriteLine("Test"));

            // ASSERT
            DefaultMockedRxApp.Verify(n => n.CreateObservableTimer(It.IsAny<DateTime>(), TimeSpan.FromDays(1), It.IsAny<Action>()), Times.Once());
        }

        [Fact]
        public void RunDailyShouldCallCreateObservableIntervall()
        {
            // ARRANGE

            // ACT
            DefaultMockedRxApp.Object.RunDaily("10:00:00", () => Console.WriteLine("Test"));

            // ASSERT
            DefaultMockedRxApp.Verify(n => n.CreateObservableTimer(It.IsAny<DateTime>(), TimeSpan.FromDays(1), It.IsAny<Action>()), Times.Once());
        }

        [Fact]
        public void RunDailyShouldThrowExceptionOnErrorFormat()
        {
            // ARRANGE
            // ACT
            // ASSERT
            Assert.Throws<FormatException>(() =>
             DefaultMockedRxApp.Object.RunDaily("no good input", () => Console.WriteLine("Test")));
        }

        [Fact]
        public void RunEveryHourShouldCallCreateObservableIntervall()
        {
            // ARRANGE
            // ACT
            DefaultMockedRxApp.Object.RunEveryHour("10:00", () => Console.WriteLine("Test"));

            // ASSERT
            DefaultMockedRxApp.Verify(n => n.CreateObservableTimer(It.IsAny<DateTime>(), It.IsAny<TimeSpan>(), It.IsAny<Action>()), Times.Once());
        }

        [Fact]
        public void RunEveryHourShouldThrowExceptionOnErrorFormat()
        {
            // ARRANGE
            // ACT
            // ASSERT
            Assert.Throws<FormatException>(() =>
            DefaultMockedRxApp.Object.RunEveryHour("no good input", () => Console.WriteLine("Test")));
        }

        [Fact]
        public void RunEveryMinuteShouldCallCreateObservableIntervall()
        {
            // ARRANGE
            // ACT
            DefaultMockedRxApp.Object.RunEveryMinute(1, () => Console.WriteLine("Test"));

            // ASSERT
            DefaultMockedRxApp.Verify(n => n.CreateObservableTimer(It.IsAny<DateTime>(), It.IsAny<TimeSpan>(), It.IsAny<Action>()), Times.Once());
        }

        [Fact]
        public void RunEveryMinuteShouldThrowExceptionOnErrorFormat()
        {
            // ARRANGE
            // ACT
            // ASSERT
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            DefaultMockedRxApp.Object.RunEveryMinute(-1, () => Console.WriteLine("Test")));
        }

        [Fact]
        public void RunEveryShouldCallCreateObservableIntervall()
        {
            // ARRANGE

            // ACT
            DefaultMockedRxApp.Object.RunEvery(TimeSpan.FromSeconds(5), () => Console.WriteLine("Test"));

            // ASSERT
            DefaultMockedRxApp.Verify(n => n.CreateObservableIntervall(TimeSpan.FromSeconds(5), It.IsAny<Action>()), Times.Once());
        }
        [Fact]
        [SuppressMessage("", "CA2201")]
        public async Task RunInFailureShouldLogError()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);
            // ACT
            DefaultDaemonRxApp.RunIn(TimeSpan.FromMilliseconds(100), () => throw new Exception("RxError"));

            // ASSERT
            await Task.Delay(150).ConfigureAwait(false);
            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);
            LoggerMock.AssertLogged(LogLevel.Error, Times.Once());
        }

        [Fact]
        public async Task RunInShouldCallFunction()
        {
            // ARRANGE
            var called = false;
            await InitializeFakeDaemon().ConfigureAwait(false);

            // ACT
            DefaultDaemonRxApp.RunIn(TimeSpan.FromMilliseconds(100), () => called = true);

            // ASSERT
            Assert.False(called);

            await Task.Delay(150).ConfigureAwait(false);

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);
            Assert.True(called);
        }
    }
}