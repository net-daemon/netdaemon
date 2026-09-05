using NetDaemon.AppModel.Internal;

namespace NetDaemon.AppModel.Tests.Context;

public class CurrentAppTests
{
    [Fact]
    public async Task AppCanInjectCurrentAppAndGetsItsOwnId()
    {
        // ARRANGE
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddNetDaemonApp<AppThatCapturesItsId>();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var appModelContext = serviceProvider.GetRequiredService<IAppModelContext>();

        // ACT
        await appModelContext.InitializeAsync(CancellationToken.None);

        // ASSERT
        var application = (Application)appModelContext.Applications.Single();
        var instance = (AppThatCapturesItsId)application.ApplicationContext!.Instance!;
        instance.CapturedId.Should().Be(typeof(AppThatCapturesItsId).FullName);
    }

    private sealed class AppThatCapturesItsId
    {
        public AppThatCapturesItsId(ICurrentApp currentApp)
        {
            CapturedId = currentApp.Id;
        }

        public string CapturedId { get; }
    }
}
