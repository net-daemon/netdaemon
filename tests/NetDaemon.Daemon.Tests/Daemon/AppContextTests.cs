namespace NetDaemon.Daemon.Tests.Daemon;

[SuppressMessage("", "CA1812")]
internal class FakeNetDaemonAppBase : NetDaemonAppBase
{
}

public class ApplicationContextTests
{
    [Fact]
    [SuppressMessage("", "CA2007")]
    public async Task TestReturnCorrectValuesForIApplicationContextUsingAppBase()
    {
        // ARRANGE
        var serviceProviderMock = new ServiceProviderMock();
        await using var ctx = new AppBaseApplicationContext(typeof(FakeNetDaemonAppBase), "id", serviceProviderMock.Object);
        var iCtx = ctx as IApplicationContext;
        ctx.IsEnabled = true;

        // ACT & ASSERT
        Assert.Equal("id", iCtx.Id);
        Assert.Equal("switch.netdaemon_id", iCtx.EntityId);
        Assert.True(iCtx.IsEnabled);
    }

    [Fact]
    [SuppressMessage("", "CA2007")]
    public async Task TestReturnCorrectValuesForIApplicationContextUsingNonAppBase()
    {
        // ARRANGE
        var serviceProviderMock = new ServiceProviderMock();
        var persistenceServiceMock = new Mock<IPersistenceService>();

        serviceProviderMock.Services.Add(typeof(IPersistenceService), persistenceServiceMock.Object);

        await using var ctx = new NonBaseApplicationContext(typeof(FakeNetDaemonAppBase), "no_app_base_app_id", serviceProviderMock.Object);
        var iCtx = ctx as IApplicationContext;

        ctx.IsEnabled = false;

        // ACT & ASSERT
        Assert.Equal("no_app_base_app_id", iCtx.Id);
        Assert.Equal("switch.netdaemon_no_app_base_app_id", iCtx.EntityId);
        Assert.False(iCtx.IsEnabled);
    }
}
