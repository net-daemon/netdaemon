namespace NetDaemon.HassClient.Tests.HelperTest;

public class ProgressiveTimeoutTests
{
    [Fact]
    public void TestProgressiveTimeout()
    {
        var progressiveTimeout = new ProgressiveTimeout(100, 1000, 2.0);
        progressiveTimeout.GetNextTimeout().Should().Be(100);
        progressiveTimeout.GetNextTimeout().Should().Be(200);
        progressiveTimeout.GetNextTimeout().Should().Be(400);
        progressiveTimeout.GetNextTimeout().Should().Be(800);
        progressiveTimeout.GetNextTimeout().Should().Be(1000);
        progressiveTimeout.GetNextTimeout().Should().Be(1000);
        progressiveTimeout.GetNextTimeout().Should().Be(1000);
    }

    [Fact]
    public void TestProgressiveTimeoutInputChecksThrows()
    {
        // Check that start timeout is alwasy greater than zero
        Assert.Throws<ArgumentOutOfRangeException>(() => new ProgressiveTimeout(0, 1000, 2.0));
        // Check that max timeout is greater than start timeout
        Assert.Throws<ArgumentOutOfRangeException>(() => new ProgressiveTimeout(100, 99, 2.0));
        // Check that increase factor is greater than 1
        Assert.Throws<ArgumentOutOfRangeException>(() => new ProgressiveTimeout(100, 1000, 1.0));
    }

    [Fact]
    public void TestProgressiveTimeoutReset()
    {
        var progressiveTimeout = new ProgressiveTimeout(100, 1000, 2.0);
        progressiveTimeout.GetNextTimeout().Should().Be(100);
        progressiveTimeout.GetNextTimeout().Should().Be(200);
        progressiveTimeout.Reset();
        progressiveTimeout.GetNextTimeout().Should().Be(100);
    }
}
