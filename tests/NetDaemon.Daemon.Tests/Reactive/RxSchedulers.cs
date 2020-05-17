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
        public async Task RunInShouldCallFunction()
        {
            // ARRANGE
            var called = false;

            // ACT
            DefaultDaemonRxApp.RunIn(TimeSpan.FromMilliseconds(100), () => called = true);

            // ASSERT
            await Task.Delay(10);
            Assert.False(called);

            await Task.Delay(100);
            Assert.True(called);
        }
    }
}