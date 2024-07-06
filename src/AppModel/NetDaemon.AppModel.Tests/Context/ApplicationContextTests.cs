using NetDaemon.AppModel.Internal;
using NetDaemon.AppModel.Internal.AppFactories;

namespace NetDaemon.AppModel.Tests.Context;

public class ApplicationContextTests
{
    [Fact]
    public async Task TestApplicationContextIsDisposedMultipleTimesNotThrowsException()
    {
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var appFactory = Mock.Of<IAppFactory>();
        var applicationContext = new ApplicationContext(serviceProvider, appFactory);

        await applicationContext.DisposeAsync();
        await applicationContext.DisposeAsync();
    }
}
