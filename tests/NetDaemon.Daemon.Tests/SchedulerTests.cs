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

        [Theory]
        [InlineData(0, 1, 1)]
        [InlineData(59, 0, 1)]
        [InlineData(0, 59, 59)]
        [InlineData(31, 30, 59)]
        public void EveryMinuteCalcTimeCorrectTargetDelay(short nowSeconds, short targetSeconds, short expectedDelaySeconds)
        {
            // ARRANGE
            var startTime =
                new DateTime(2001, 2, 3, 10, 0, nowSeconds);

            var mockTimeManager = new TimeManagerMock(startTime);

            var scheduler = new Scheduler(mockTimeManager.Object);

            var calculatedDelay = scheduler.CalculateEveryMinuteTimeBetweenNowAndTargetTime(targetSeconds);

            Assert.Equal(expectedDelaySeconds, calculatedDelay.TotalSeconds);
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
            Assert.True(nrOfRuns == 1);

            await scheduler.Stop();
            try
            {
                await runTask;
            }
            catch
            {
            }
        }

        [Theory]
        [InlineData("2001-02-03 10:00:00", DayOfWeek.Saturday)]
        [InlineData("2001-02-04 10:00:00", DayOfWeek.Sunday)]
        [InlineData("2001-02-05 10:00:00", DayOfWeek.Monday)]
        [InlineData("2001-02-06 10:00:00", DayOfWeek.Tuesday)]
        [InlineData("2001-02-07 10:00:00", DayOfWeek.Wednesday)]
        [InlineData("2001-02-08 10:00:00", DayOfWeek.Thursday)]
        [InlineData("2001-02-09 10:00:00", DayOfWeek.Friday)]
        public async void TestRunDailyUsingStartTimeOnWeekdayCallsFuncCorrectly(string time, DayOfWeek dayOfWeek)
        {
            // ARRANGE
            var startTime = DateTime.ParseExact(time, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            var mockTimeManager = new TimeManagerMock(startTime);

            var scheduler = new Scheduler(mockTimeManager.Object);

            var nrOfRuns = 0;

            // ACT
            var runTask = scheduler.RunDailyAsync("10:00:01", new DayOfWeek[] { dayOfWeek }, async () =>
              {
                  nrOfRuns++;
                  await Task.Delay(1);
              });
            await Task.Delay(600);

            // ASSERT
            Assert.True(nrOfRuns == 0);
            await Task.Delay(800);
            Assert.True(nrOfRuns >= 1);

            await scheduler.Stop();
            try
            {
                await runTask;
            }
            catch
            {
            }
        }

        [Theory]
        [InlineData("2001-02-03 10:00:00")]
        [InlineData("2001-02-04 10:00:00")]
        [InlineData("2001-02-06 10:00:00")]
        [InlineData("2001-02-07 10:00:00")]
        [InlineData("2001-02-08 10:00:00")]
        [InlineData("2001-02-09 10:00:00")]
        public async void TestRunDailyUsingStartTimeOnWeekdayNotCalled(string time)
        {
            // ARRANGE
            var startTime = DateTime.ParseExact(time, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            var mockTimeManager = new TimeManagerMock(startTime);

            var scheduler = new Scheduler(mockTimeManager.Object);

            var nrOfRuns = 0;

            // ACT
            var runTask = scheduler.RunDailyAsync("10:00:01", new DayOfWeek[] { DayOfWeek.Monday }, async () =>
              {
                  nrOfRuns++;
                  await Task.Delay(1);
              });
            await Task.Delay(600);

            // ASSERT
            Assert.True(nrOfRuns == 0);
            await Task.Delay(500);
            Assert.False(nrOfRuns == 1);

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
        public async void TestRunEveryMinuteStartTimeCallsFuncCorrectly()
        {
            // ARRANGE
            var startTime =
                new DateTime(2001, 2, 3, 10, 00, 59);

            var mockTimeManager = new TimeManagerMock(startTime);

            var scheduler = new Scheduler(mockTimeManager.Object);

            var nrOfRuns = 0;

            // ACT
            var runTask = scheduler.RunEveryMinuteAsync(0, async () =>
           {
               nrOfRuns++;
               await Task.Delay(1);
           });
            await Task.Delay(300);

            // ASSERT
            Assert.Equal(0, nrOfRuns);
            await Task.Delay(1500);
            Assert.True(nrOfRuns >= 1);

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
        public async void TestRunEveryMinuteStartTimeNotZeroCallsFuncCorrectly()
        {
            // ARRANGE
            var startTime =
                new DateTime(2001, 2, 3, 10, 00, 19);

            var mockTimeManager = new TimeManagerMock(startTime);

            var scheduler = new Scheduler(mockTimeManager.Object);

            var nrOfRuns = 0;

            // ACT
            var runTask = scheduler.RunEveryMinuteAsync(20, async () =>
           {
               nrOfRuns++;
               await Task.Delay(1);
           });
            await Task.Delay(600);

            // ASSERT
            Assert.True(nrOfRuns == 0);
            await Task.Delay(500);
            Assert.True(nrOfRuns == 1);

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