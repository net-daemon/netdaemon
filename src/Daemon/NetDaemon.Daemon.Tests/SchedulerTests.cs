using JoySoftware.HomeAssistant.NetDaemon.Daemon;
using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using System.Diagnostics;
using System.Globalization;

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

            var mockTimeManager = new TimeManagerMock(startTime);

            var scheduler = new Scheduler();

            var isTaskRun = false;

            // ACT
            var runTask = scheduler.RunInAsync(200, async () =>
            {
                isTaskRun = true;
                await Task.Delay(1);
            });

            // ASSERT
            // Assert not run before time

            await Task.Delay(100);
            Assert.False(isTaskRun);

            await Task.Delay(150);

            await scheduler.Stop();
            try
            {
                await runTask;
            }
            catch
            {
            }

            Assert.True(isTaskRun);
            Assert.True(runTask.IsCompletedSuccessfully);
        }
        // Todo: Fix test in azure
        // [Fact]
        // public async void TestRunInEveryCallsFuncCorrectly()
        // {
        //     // ARRANGE
        //     var startTime =
        //         new DateTime(2001, 2, 3, 4, 5, 6);

        //     var mockTimeManager = new TimeManagerMock(startTime);

        //     var scheduler = new Scheduler();

        //     var nrOfRuns = 0;

        //     var watch = new Stopwatch();

        //     // ACT
        //     var runTask = scheduler.RunEveryAsync(10, async () =>
        //     {
        //         nrOfRuns++;
        //         await Task.Delay(1);
        //     });
        //     watch.Start();
        //     await Task.Delay(50);
        //     watch.Stop();

        //     // ASSERT
        //     Assert.True(nrOfRuns > watch.ElapsedMilliseconds / 10);
        //     // Test again
        //     await scheduler.Stop();
        //     try
        //     {
        //         await runTask;
        //     }
        //     catch
        //     {
        //     }
        // }

        [Theory]
        [InlineData("00:00:00", "00:00:01", 1)]
        [InlineData("00:00:00", "00:01:00", 60)]
        [InlineData("00:00:00", "00:00:00", 0)]
        [InlineData("23:59:59", "00:00:00", 1)]
        [InlineData("00:00:01", "00:00:00", (24 * 60 * 60) - 1)]
        public void DailyTimeBetweenNowAndTargetTime(string nowTime, string targetTime, int nrOfSecondsRemaining)
        {
            // ARRANGE
            DateTime timePart = DateTime.ParseExact(nowTime, "HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime fakeTimeNow = new DateTime(2001, 01, 01, timePart.Hour, timePart.Minute, timePart.Second);
            DateTime timeTarget = DateTime.ParseExact(targetTime, "HH:mm:ss", CultureInfo.InvariantCulture);

            var mockTimeManager = new TimeManagerMock(fakeTimeNow);

            var scheduler = new Scheduler(mockTimeManager.Object);

            var timeToWait = scheduler.CalculateDailyTimeBetweenNowAndTargetTime(timeTarget);

            Assert.Equal(nrOfSecondsRemaining, timeToWait.TotalSeconds);

        }

        [Fact]
        public async void TestRunDailyUsingStartTimeCallsFuncCorrectly()
        {
            // ARRANGE
            var startTime =
                new DateTime(2001, 2, 3, 10, 0, 0);

            var mockTimeManager = new TimeManagerMock(startTime);

            var scheduler = new Scheduler(mockTimeManager.Object);

            var nrOfRuns = 0;

            // ACT
            var runTask = scheduler.RunDailyAsync("10:00:01", async () =>
           {
               nrOfRuns++;
               await Task.Delay(1);
           });
            await Task.Delay(600);

            // ASSERT
            Assert.True(nrOfRuns == 0);
            await Task.Delay(500);
            Assert.True(nrOfRuns > 0);

            await scheduler.Stop();
            try
            {
                await runTask;
            }
            catch
            {
            }
        }


        [Fact]
        public async Task ScheduleLongTaskWillCompensateTimeToZero()
        {
            // ARRANGE

            var mockTimeManager = new TimeManagerMock();

            var scheduler = new Scheduler(mockTimeManager.Object);

            // ACT
            var runTask = scheduler.RunEveryAsync(20, async () =>
            {
                await Task.Delay(25);
            });

            await Task.WhenAny(runTask, Task.Delay(500));
            await scheduler.Stop();

            // ASSERT

            mockTimeManager.Verify(n => n.Delay(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);


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