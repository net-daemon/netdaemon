using NetDaemon.AppModel.Internal;
using NetDaemon.AppModel.Internal.Resolver;

namespace NetDaemon.AppModel.Tests.Context;

public class ApplicationScopeTests
{
    /// <summary>
    ///     This test do integration test that uses dynamic compilation. Only thing that is faked is the path to
    ///     the apps folder that it is injected as transient to fake it.
    /// </summary>
    [Fact]
    public void TestFailedInitializedScopeThrows()
    {
        var scope = new ApplicationScope();

        Assert.Throws<InvalidOperationException>(() => scope.ApplicationContext);
    }

    [Fact]
    public void TestInitializedScopeReturnsOk()
    {
        var scope = new ApplicationScope
        {
            ApplicationContext = new ApplicationContext(
                new ServiceCollection().BuildServiceProvider(),
                Mock.Of<IAppInstance>()
            )
        };
        var ctx = scope.ApplicationContext;

        ctx.Should().NotBeNull();
    }
}