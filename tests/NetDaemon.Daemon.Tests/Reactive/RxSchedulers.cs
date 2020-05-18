using Moq;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NetDaemon.Daemon.Tests
{
    /// <summary>
    ///     Tests the fluent API parts of the daemon
    /// </summary>
    /// <remarks>
    ///     Mainly the tests checks if correct underlying call to "CallService"
    ///     has been made.
    /// </remarks>
    public class RxSchedulerTest : DaemonHostTestBase
    {
        public RxSchedulerTest() : base()
        {
        }

        [Fact]
        public void RunEveryShouldCallCreateObservableIntervall()
        {
            // ARRANGE

            // ACT
            DefaultMockedRxApp.Object.RunEvery(TimeSpan.FromSeconds(5), () => System.Console.WriteLine("Test"));

            // ASSERT
            DefaultMockedRxApp.Verify(n => n.CreateObservableIntervall(TimeSpan.FromSeconds(5), It.IsAny<Action>()), Times.Once());
        }

        [Fact]
        public void RunDailyShouldCallCreateObservableIntervall()
        {
            // ARRANGE

            // ACT
            DefaultMockedRxApp.Object.RunDaily("10:00:00", () => System.Console.WriteLine("Test"));

            // ASSERT
            DefaultMockedRxApp.Verify(n => n.CreateObservableTimer(It.IsAny<DateTime>(), TimeSpan.FromDays(1), It.IsAny<Action>()), Times.Once());
        }

        [Fact]
        public void RunDailyOneHourBeforeShouldCallCreateObservableIntervall()
        {

            // ARRANGE
            var time = DateTime.Now;
            var timeOneHourBack = time.Subtract(TimeSpan.FromHours(1));
            var timeFormat = timeOneHourBack.ToString("HH:mm:ss");

            // ACT
            DefaultMockedRxApp.Object.RunDaily(timeFormat, () => System.Console.WriteLine("Test"));

            // ASSERT
            DefaultMockedRxApp.Verify(n => n.CreateObservableTimer(It.IsAny<DateTime>(), TimeSpan.FromDays(1), It.IsAny<Action>()), Times.Once());
        }

        [Fact]
        public void RunDailyOneHourAfterShouldCallCreateObservableIntervall()
        {

            // ARRANGE
            var time = DateTime.Now;
            var timeOneHourBack = time.AddHours(1);
            var timeFormat = timeOneHourBack.ToString("HH:mm:ss");

            // ACT
            DefaultMockedRxApp.Object.RunDaily(timeFormat, () => System.Console.WriteLine("Test"));

            // ASSERT
            DefaultMockedRxApp.Verify(n => n.CreateObservableTimer(It.IsAny<DateTime>(), TimeSpan.FromDays(1), It.IsAny<Action>()), Times.Once());
        }

        [Fact]
        public void RunEveryHourShouldCallCreateObservableIntervall()
        {

            // ARRANGE
            // ACT
            DefaultMockedRxApp.Object.RunEveryHour("10:00", () => System.Console.WriteLine("Test"));

            // ASSERT
            DefaultMockedRxApp.Verify(n => n.CreateObservableTimer(It.IsAny<DateTime>(), It.IsAny<TimeSpan>(), It.IsAny<Action>()), Times.Once());
        }

        [Fact]
        public void RunEveryMinuteShouldCallCreateObservableIntervall()
        {

            // ARRANGE
            // ACT
            DefaultMockedRxApp.Object.RunEveryMinute(0, () => System.Console.WriteLine("Test"));

            // ASSERT
            DefaultMockedRxApp.Verify(n => n.CreateObservableTimer(It.IsAny<DateTime>(), It.IsAny<TimeSpan>(), It.IsAny<Action>()), Times.Once());
        }

        [Fact]
        public void RunDailyShouldThrowExceptionOnErrorFormat()
        {
            // ARRANGE
            // ACT
            // ASSERT
            Assert.Throws<FormatException>(() =>
             DefaultMockedRxApp.Object.RunDaily("no good input", () => System.Console.WriteLine("Test")));
        }

        [Fact]
        public void RunEveryHourShouldThrowExceptionOnErrorFormat()
        {
            // ARRANGE
            // ACT
            // ASSERT
            Assert.Throws<FormatException>(() =>
             DefaultMockedRxApp.Object.RunEveryHour("no good input", () => System.Console.WriteLine("Test")));
        }

        [Fact]
        public void RunEveryMinuteShouldThrowExceptionOnErrorFormat()
        {
            // ARRANGE
            // ACT
            // ASSERT
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            DefaultMockedRxApp.Object.RunEveryMinute(-1, () => System.Console.WriteLine("Test")));
        }

        [Fact]
        public async Task RunInShouldCallFunction()
        {
            // ARRANGE
            var called = false;

            // ACT
            DefaultDaemonRxApp.RunIn(TimeSpan.FromMilliseconds(100), () => called = true);

            // ASSERT
            Assert.False(called);

            await Task.Delay(150);
            Assert.True(called);
        }




    }
}