using FluentAssertions;
using NetDaemon.Tests.Integration.Helpers;
using Xunit;

namespace NetDaemon.Tests.Integration;

public class WaitForConditionHelperTests
{
    [Fact]
    public async Task WaitUntilAsync_ShouldReturn_WhenConditionBecomesTrue()
    {
        var attempts = 0;

        await WaitForConditionHelper.WaitUntilAsync(
            _ => Task.FromResult(++attempts >= 3),
            TimeSpan.FromMilliseconds(200),
            TimeSpan.FromMilliseconds(10),
            "Condition was not met");

        attempts.Should().Be(3);
    }

    [Fact]
    public async Task WaitUntilAsync_ShouldThrowTimeoutException_WhenConditionDoesNotBecomeTrue()
    {
        var act = async () => await WaitForConditionHelper.WaitUntilAsync(
            _ => Task.FromResult(false),
            TimeSpan.FromMilliseconds(50),
            TimeSpan.FromMilliseconds(10),
            "Condition was not met");

        await act.Should().ThrowAsync<TimeoutException>()
            .WithMessage("Condition was not met");
    }
}
