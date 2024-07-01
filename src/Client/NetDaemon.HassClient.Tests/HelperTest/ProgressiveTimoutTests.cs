namespace NetDaemon.HassClient.Tests.HelperTest;

public class ProgressiveTimeoutTests
{
    [Fact]
    public void TestProgressiveTimeout()
    {
        var progressiveTimeout = new ProgressiveTimeout(TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(1000), 2.0);
        progressiveTimeout.Timeout.Should().Be(TimeSpan.FromSeconds(100));
        progressiveTimeout.Timeout.Should().Be(TimeSpan.FromSeconds(200));
        progressiveTimeout.Timeout.Should().Be(TimeSpan.FromSeconds(400));
        progressiveTimeout.Timeout.Should().Be(TimeSpan.FromSeconds(800));
        progressiveTimeout.Timeout.Should().Be(TimeSpan.FromSeconds(1000));
        progressiveTimeout.Timeout.Should().Be(TimeSpan.FromSeconds(1000));
        progressiveTimeout.Timeout.Should().Be(TimeSpan.FromSeconds(1000));
    }

    [Fact]
    public void TestProgressiveTimeoutInputChecksThrows()
    {
        // Check that start timeout is alwasy greater than zero
        Assert.Throws<ArgumentOutOfRangeException>(() => new ProgressiveTimeout(TimeSpan.Zero, TimeSpan.FromSeconds(100), 2.0));
        // Check that max timeout is greater than start timeout
        Assert.Throws<ArgumentOutOfRangeException>(() => new ProgressiveTimeout(TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(99), 2.0));
        // Check that max timeout is not same as start timeout
        Assert.Throws<ArgumentOutOfRangeException>(() => new ProgressiveTimeout(TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(100), 2.0));
        // Check that increase factor is greater than 1
        Assert.Throws<ArgumentOutOfRangeException>(() => new ProgressiveTimeout(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(100), 1.0));
    }

    [Fact]
    public void TestProgressiveTimeoutReset()
    {
        var progressiveTimeout = new ProgressiveTimeout(TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(1000), 2.0);
        progressiveTimeout.Timeout.Should().Be(TimeSpan.FromSeconds(100));
        progressiveTimeout.Timeout.Should().Be(TimeSpan.FromSeconds(200));
        progressiveTimeout.Reset();
        progressiveTimeout.Timeout.Should().Be(TimeSpan.FromSeconds(100));
    }
}
