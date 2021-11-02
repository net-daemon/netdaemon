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
    }
}