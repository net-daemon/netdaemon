using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using NetDaemon.Common;
using Xunit;
using NetDaemon.Daemon.Fakes;
using System.Diagnostics.CodeAnalysis;

namespace NetDaemon.Daemon.Tests
{
    public class SchedulerTests
    {
        [Fact]
        [SuppressMessage("", "CA1031")]
        public async void TestRunInShouldStartAndCompleteCorrectly()
        {
            // ARRANGE
            var startTime =
                new DateTime(2001, 2, 3, 4, 5, 6);

            var mockTimeManager = new TimeManagerMock(startTime);
            var isTaskRun = false;
            ISchedulerResult scheduledResult;
            await using (IScheduler scheduler = new Scheduler())
            {
                // ACT
                scheduledResult = scheduler.RunIn(200, async () =>
                {
                    isTaskRun = true;
                    await Task.Delay(1).ConfigureAwait(false);
                });

                // ASSERT
                // Assert not run before time

                await Task.Delay(100).ConfigureAwait(false);
                Assert.False(isTaskRun);

                await Task.Delay(150).ConfigureAwait(false);
            }

            try
            {
                await scheduledResult.Task.ConfigureAwait(false);
            }
            catch
            {
            }

            Assert.True(isTaskRun);
            Assert.True(scheduledResult.Task.IsCompletedSuccessfully);
        }

        [Fact]
        [SuppressMessage("", "CA1031")]
        public async void RunInShouldLogWarningForFaultyRun()
        {
            // ARRANGE
            var startTime =
                new DateTime(2001, 2, 3, 4, 5, 6);

            var mockTimeManager = new TimeManagerMock(startTime);
            var loggerMock = new LoggerMock();

            ISchedulerResult scheduledResult;
            await using (IScheduler scheduler = new Scheduler(null, loggerMock.LoggerFactory))
            {
                // ACT
                scheduledResult = scheduler.RunIn(20, () =>
                {
                    int i = int.Parse("Not an integer makes runtime error!", CultureInfo.InvariantCulture);
                    return Task.CompletedTask;
                });

                await Task.Delay(100).ConfigureAwait(false);
            }

            try
            {
                await scheduledResult.Task.ConfigureAwait(false);
            }
            catch
            {
            }
            // ASSERT
            loggerMock.AssertLogged(LogLevel.Warning, Times.Once());
        }

        [Fact]
        [SuppressMessage("", "CA1031")]
        public async void TestRunInShouldStartAndAncCancelCorrectly()
        {
            // ARRANGE
            var startTime =
                new DateTime(2001, 2, 3, 4, 5, 6);

            var mockTimeManager = new TimeManagerMock(startTime);
            var isTaskRun = false;
            ISchedulerResult scheduledResult;
            await using (IScheduler scheduler = new Scheduler())
            {
                // ACT
                scheduledResult = scheduler.RunIn(200, async () =>
                {
                    isTaskRun = true;
                    await Task.Delay(1).ConfigureAwait(false);
                });

                // ASSERT
                // Assert not run before time

                await Task.Delay(100).ConfigureAwait(false);
                Assert.False(isTaskRun);
                scheduledResult.CancelSource.Cancel();
                await Task.Delay(150).ConfigureAwait(false);
            }

            try
            {
                await scheduledResult.Task.ConfigureAwait(false);
            }
            catch
            {
            }

            Assert.False(isTaskRun);
            Assert.True(scheduledResult.Task.IsCanceled);
        }

        [Theory]
        [InlineData("00:00:00", "00:00:01", 1)]
        [InlineData("00:00:00", "00:01:00", 60)]
        [InlineData("00:00:00", "00:00:00", 0)]
        [InlineData("23:59:59", "00:00:00", 1)]
        [InlineData("00:00:01", "00:00:00", (24 * 60 * 60) - 1)]
        public async Task DailyTimeBetweenNowAndTargetTime(string nowTime, string targetTime, int nrOfSecondsRemaining)
        {
            // ARRANGE
            DateTime timePart = DateTime.ParseExact(nowTime, "HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime fakeTimeNow = new(2001, 01, 01, timePart.Hour, timePart.Minute, timePart.Second);
            DateTime timeTarget = DateTime.ParseExact(targetTime, "HH:mm:ss", CultureInfo.InvariantCulture);

            var mockTimeManager = new TimeManagerMock(fakeTimeNow);

            await using var scheduler = new Scheduler(mockTimeManager.Object);

            var timeToWait = scheduler.CalculateDailyTimeBetweenNowAndTargetTime(timeTarget);

            Assert.Equal(nrOfSecondsRemaining, timeToWait.TotalSeconds);
        }

        [Theory]
        [InlineData(0, 1, 1)]
        [InlineData(59, 0, 1)]
        [InlineData(0, 59, 59)]
        [InlineData(31, 30, 59)]
        public async Task EveryMinuteCalcTimeCorrectTargetDelay(short nowSeconds, short targetSeconds, short expectedDelaySeconds)
        {
            // ARRANGE
            var startTime =
                new DateTime(2001, 2, 3, 10, 0, nowSeconds);

            var mockTimeManager = new TimeManagerMock(startTime);

            await using var scheduler = new Scheduler(mockTimeManager.Object);

            var calculatedDelay = scheduler.CalculateEveryMinuteTimeBetweenNowAndTargetTime(targetSeconds);

            Assert.Equal(expectedDelaySeconds, calculatedDelay.TotalSeconds);
        }

        [Fact]
        [SuppressMessage("", "CA1508")]
        [SuppressMessage("", "CA1031")]
        public async void TestRunDailyUsingStartTimeCallsFuncCorrectly()
        {
            // ARRANGE
            var startTime =
                new DateTime(2001, 2, 3, 10, 0, 0);

            var mockTimeManager = new TimeManagerMock(startTime);
            var nrOfRuns = 0;
            ISchedulerResult scheduledResult;
            await using (IScheduler scheduler = new Scheduler(mockTimeManager.Object))
            {
                // ACT
                scheduledResult = scheduler.RunDaily("10:00:01", async () =>
               {
                   nrOfRuns++;
                   await Task.Delay(1).ConfigureAwait(false);
               });
                await Task.Delay(600).ConfigureAwait(false);

                // ASSERT
                Assert.True(nrOfRuns == 0);
                await Task.Delay(500).ConfigureAwait(false);
                Assert.True(nrOfRuns == 1);
            }
            try
            {
                await scheduledResult.Task.ConfigureAwait(false);
            }
            catch
            {
            }
        }

        [Fact]
        [SuppressMessage("", "CA1031")]
        public async void RunDailyFaultShouldLogWarning()
        {
            // ARRANGE
            var startTime =
                new DateTime(2001, 2, 3, 10, 0, 0);

            var mockTimeManager = new TimeManagerMock(startTime);
            var loggerMock = new LoggerMock();

            ISchedulerResult scheduledResult;
            await using (IScheduler scheduler = new Scheduler(mockTimeManager.Object, loggerMock.LoggerFactory))
            {
                // ACT
                scheduledResult = scheduler.RunDaily("10:00:01", () =>
               {
                   int i = int.Parse("Not an integer makes runtime error!", CultureInfo.InvariantCulture);
                   return Task.CompletedTask;
               });
                await Task.Delay(1500).ConfigureAwait(false);
            }
            try
            {
                await scheduledResult.Task.ConfigureAwait(false);
            }
            catch
            {
            }

            // ASSERT
            loggerMock.AssertLogged(LogLevel.Warning, Times.Once());
        }

        [Fact]
        [SuppressMessage("", "CA1031")]
        public async void RunDailyOnDaysFaultShouldLogWarning()
        {
            // ARRANGE
            var startTime =
                new DateTime(2001, 2, 3, 10, 0, 0);

            var mockTimeManager = new TimeManagerMock(startTime);
            var loggerMock = new LoggerMock();

            ISchedulerResult scheduledResult;
            await using (IScheduler scheduler = new Scheduler(mockTimeManager.Object, loggerMock.LoggerFactory))
            {
                // ACT
                scheduledResult = scheduler.RunDaily("10:00:01", new DayOfWeek[] { DayOfWeek.Saturday }, () =>
                 {
                     int i = int.Parse("Not an integer makes runtime error!", CultureInfo.InvariantCulture);
                     return Task.CompletedTask;
                 });
                await Task.Delay(1500).ConfigureAwait(false);
            }
            try
            {
                await scheduledResult.Task.ConfigureAwait(false);
            }
            catch
            {
            }

            // ASSERT
            loggerMock.AssertLogged(LogLevel.Warning, Times.Once());
        }

        [Fact]
        [SuppressMessage("", "CA1508")]
        [SuppressMessage("", "CA1031")]
        public async void TestRunDailyUsingStartTimeCancelsCorrectly()
        {
            // ARRANGE
            var startTime =
                new DateTime(2001, 2, 3, 10, 0, 0);

            var mockTimeManager = new TimeManagerMock(startTime);
            var nrOfRuns = 0;
            ISchedulerResult scheduledResult;
            await using (IScheduler scheduler = new Scheduler(mockTimeManager.Object))
            {
                // ACT
                scheduledResult = scheduler.RunDaily("10:00:01", async () =>
               {
                   nrOfRuns++;
                   await Task.Delay(1).ConfigureAwait(false);
               });
                await Task.Delay(600).ConfigureAwait(false);

                // ASSERT
                Assert.True(nrOfRuns == 0);
                scheduledResult.CancelSource.Cancel();
                await Task.Delay(500).ConfigureAwait(false);
                Assert.True(nrOfRuns == 0);
            }
            try
            {
                await scheduledResult.Task.ConfigureAwait(false);
            }
            catch
            {
            }
            Assert.True(scheduledResult.Task.IsCanceled);
        }

        [Theory]
        [InlineData("2001-02-03 10:00:00", DayOfWeek.Saturday)]
        [InlineData("2001-02-04 10:00:00", DayOfWeek.Sunday)]
        [InlineData("2001-02-05 10:00:00", DayOfWeek.Monday)]
        [InlineData("2001-02-06 10:00:00", DayOfWeek.Tuesday)]
        [InlineData("2001-02-07 10:00:00", DayOfWeek.Wednesday)]
        [InlineData("2001-02-08 10:00:00", DayOfWeek.Thursday)]
        [InlineData("2001-02-09 10:00:00", DayOfWeek.Friday)]
        [SuppressMessage("", "CA1508")]
        [SuppressMessage("", "CA1031")]
        public async void TestRunDailyUsingStartTimeOnWeekdayCallsFuncCorrectly(string time, DayOfWeek dayOfWeek)
        {
            // ARRANGE
            var startTime = DateTime.ParseExact(time, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            var mockTimeManager = new TimeManagerMock(startTime);
            ISchedulerResult scheduledResult;
            await using (IScheduler scheduler = new Scheduler(mockTimeManager.Object))
            {
                var nrOfRuns = 0;

                // ACT
                scheduledResult = scheduler.RunDaily("10:00:01", new DayOfWeek[] { dayOfWeek }, async () =>
                   {
                       nrOfRuns++;
                       await Task.Delay(1).ConfigureAwait(false);
                   });
                await Task.Delay(200).ConfigureAwait(false);

                // ASSERT
                Assert.Equal(0, nrOfRuns);
                await Task.Delay(1200).ConfigureAwait(false);
                Assert.Equal(1, nrOfRuns);
            }

            try
            {
                await scheduledResult.Task.ConfigureAwait(false);
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
        [SuppressMessage("", "CA1508")]
        [SuppressMessage("", "CA1031")]
        public async void TestRunDailyUsingStartTimeOnWeekdayNotCalled(string time)
        {
            // ARRANGE
            var startTime = DateTime.ParseExact(time, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            var mockTimeManager = new TimeManagerMock(startTime);
            ISchedulerResult scheduledResult;

            await using (IScheduler scheduler = new Scheduler(mockTimeManager.Object))
            {
                var nrOfRuns = 0;

                // ACT
                scheduledResult = scheduler.RunDaily("10:00:01", new DayOfWeek[] { DayOfWeek.Monday }, async () =>
                  {
                      nrOfRuns++;
                      await Task.Delay(1).ConfigureAwait(false);
                  });
                await Task.Delay(600).ConfigureAwait(false);

                // ASSERT
                Assert.True(nrOfRuns == 0);
                await Task.Delay(500).ConfigureAwait(false);
                Assert.False(nrOfRuns == 1);
            }

            try
            {
                await scheduledResult.Task.ConfigureAwait(false);
            }
            catch
            {
            }
        }

        [Fact]
        [SuppressMessage("", "CA1031")]
        public async void TestRunEveryMinuteStartTimeCallsFuncCorrectly()
        {
            // ARRANGE
            var startTime =
                new DateTime(2001, 2, 3, 10, 00, 59);

            var mockTimeManager = new TimeManagerMock(startTime);
            ISchedulerResult scheduledResult;
            await using (IScheduler scheduler = new Scheduler(mockTimeManager.Object))
            {
                var nrOfRuns = 0;

                // ACT
                scheduledResult = scheduler.RunEveryMinute(0, async () =>
                {
                    nrOfRuns++;
                    await Task.Delay(1).ConfigureAwait(false);
                });
                await Task.Delay(300).ConfigureAwait(false);

                // ASSERT
                Assert.Equal(0, nrOfRuns);
                await Task.Delay(1500).ConfigureAwait(false);
                Assert.True(nrOfRuns >= 1);
            }

            try
            {
                await scheduledResult.Task.ConfigureAwait(false);
            }
            catch
            {
            }
        }

        [Fact]
        [SuppressMessage("", "CA1031")]
        public async void RunEveryMinuteFaultyShouldLogWarning()
        {
            // ARRANGE
            var startTime =
                new DateTime(2001, 2, 3, 10, 00, 59);

            var mockTimeManager = new TimeManagerMock(startTime);
            var loggerMock = new LoggerMock();

            ISchedulerResult scheduledResult;
            await using (IScheduler scheduler = new Scheduler(mockTimeManager.Object, loggerMock.LoggerFactory))
            {
                // ACT
                scheduledResult = scheduler.RunEveryMinute(0, () =>
                {
                    int i = int.Parse("Not an integer makes runtime error!", CultureInfo.InvariantCulture);
                    return Task.CompletedTask;
                });
                await Task.Delay(1200).ConfigureAwait(false);
            }

            try
            {
                await scheduledResult.Task.ConfigureAwait(false);
            }
            catch
            {
            }
            // ASSERT
            loggerMock.AssertLogged(LogLevel.Warning, Times.Once());
        }

        [Fact]
        [SuppressMessage("", "CA1031")]
        public async void TestRunEveryMinuteStartTimeCanceledCorrectly()
        {
            // ARRANGE
            var startTime =
                new DateTime(2001, 2, 3, 10, 00, 59);

            var mockTimeManager = new TimeManagerMock(startTime);
            ISchedulerResult scheduledResult;
            await using (IScheduler scheduler = new Scheduler(mockTimeManager.Object))
            {
                var nrOfRuns = 0;

                // ACT
                scheduledResult = scheduler.RunEveryMinute(0, async () =>
                {
                    nrOfRuns++;
                    await Task.Delay(1).ConfigureAwait(false);
                });
                await Task.Delay(300).ConfigureAwait(false);

                // ASSERT
                Assert.Equal(0, nrOfRuns);
                scheduledResult.CancelSource.Cancel();
                await Task.Delay(1500).ConfigureAwait(false);
                Assert.Equal(0, nrOfRuns);
            }

            try
            {
                await scheduledResult.Task.ConfigureAwait(false);
            }
            catch
            {
            }
            Assert.True(scheduledResult.Task.IsCanceled);
        }

        [Fact]
        [SuppressMessage("", "CA1508")]
        [SuppressMessage("", "CA1031")]
        public async void TestRunEveryMinuteStartTimeNotZeroCallsFuncCorrectly()
        {
            // ARRANGE
            var startTime =
                new DateTime(2001, 2, 3, 10, 00, 19);

            var mockTimeManager = new TimeManagerMock(startTime);
            ISchedulerResult scheduledResult;

            await using (IScheduler scheduler = new Scheduler(mockTimeManager.Object))
            {
                var nrOfRuns = 0;

                // ACT
                scheduledResult = scheduler.RunEveryMinute(20, async () =>
                {
                    nrOfRuns++;
                    await Task.Delay(1).ConfigureAwait(false);
                });
                await Task.Delay(600).ConfigureAwait(false);

                // ASSERT
                Assert.True(nrOfRuns == 0);
                await Task.Delay(500).ConfigureAwait(false);
                Assert.True(nrOfRuns == 1);
            }

            try
            {
                await scheduledResult.Task.ConfigureAwait(false);
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

            await using (IScheduler scheduler = new Scheduler(mockTimeManager.Object))
            {
                // ACT
                var runTask = scheduler.RunEvery(20, async () => await Task.Delay(50).ConfigureAwait(false));

                await Task.WhenAny(runTask.Task, Task.Delay(500)).ConfigureAwait(false);
            }
            // ASSERT
            mockTimeManager.Verify(n => n.Delay(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ScheduleLongTaskWillCompensateTime()
        {
            // ARRANGE

            var mockTimeManager = new TimeManagerMock();

            await using (IScheduler scheduler = new Scheduler(mockTimeManager.Object))
            {
                // ACT
                var runTask = scheduler.RunEvery(20, async () => await Task.Delay(1).ConfigureAwait(false));

                await Task.WhenAny(runTask.Task, Task.Delay(100)).ConfigureAwait(false);
            }

            // ASSERT
            // Make it less times due to slow cloud CI build engines (4->2)
            mockTimeManager.Verify(n => n.Delay(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.AtLeast(2));
        }
    }
}