using JoySoftware.HomeAssistant.NetDaemon.Daemon;
using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace NetDaemon.Daemon.Tests
{
    public class SchedulerTests
    {
        [Fact]
        public async void TestRunInShouldStartAndCompleteCorrectly()
        {
            // ARRANGE
            var startTime =
                new DateTime(2001, 2, 3, 4, 5, 6);

            var mockTimeManager = new TimeManagerMock { CurrentTime = startTime };

            var scheduler = new Scheduler();

            var isTaskRun = false;

            // ACT
            var runTask = scheduler.RunInAsync(20, async () =>
            {
                isTaskRun = true;
                await Task.Delay(1);
            });
            
            // ASSERT
            // Assert not run before time

            await Task.Delay(10);
            Assert.False(isTaskRun);

            await Task.Delay(35);

            Assert.True(isTaskRun);
            Assert.True(runTask.IsCompletedSuccessfully);
        }

        [Fact]
        public async void TestRunInEveryCallsFuncCorrectly()
        {
            // ARRANGE
            var startTime =
                new DateTime(2001, 2, 3, 4, 5, 6);

            var mockTimeManager = new TimeManagerMock { CurrentTime = startTime };

            var scheduler = new Scheduler();

            var nrOfRuns = 0;

           // ACT
           var runTask = scheduler.RunEveryAsync(10, async () =>
           {
               nrOfRuns++;
               await Task.Delay(1);
           });
           await Task.Delay(50);

            // ASSERT

            Assert.True(nrOfRuns>=4 && nrOfRuns <= 5);
            await scheduler.Stop();
            await runTask;
            Assert.True(runTask.IsCompletedSuccessfully);
        }

        [Fact]
        public async Task ScheduleLongTaskWillCompensateTimeToZero()
        {
            // ARRANGE

            var mockTimeManager = new TimeManagerMock( );

            var scheduler = new Scheduler(mockTimeManager.Object);

            // ACT
            var runTask = scheduler.RunEveryAsync(20, async () =>
            {
                await Task.Delay(25);
            });

            await Task.WhenAny(runTask, Task.Delay(100));
            await scheduler.Stop();

            // ASSERT

            mockTimeManager.Verify(n=>n.Delay(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);

        }

        [Fact]
        public async Task ScheduleLongTaskWillCompensateTime()
        {
            // ARRANGE

            var mockTimeManager = new TimeManagerMock();

            var scheduler = new Scheduler(mockTimeManager.Object);

            // ACT
            var runTask = scheduler.RunEveryAsync(20, async () =>
            {
                await Task.Delay(1);
            });

            await Task.WhenAny(runTask, Task.Delay(100));
            await scheduler.Stop();

            // ASSERT

            mockTimeManager.Verify(n => n.Delay(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.AtLeast(4));

        }
    }
}