using LocalApps;
using NetDaemon.AppModel.Internal;

namespace NetDaemon.AppModel.Tests.AppFactories;

public class ClassAppFactoryTests
{
    [Fact]
    public async Task AddAppsFromAssembly()
    {
        // ARRANGE
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();

        // ACT
        serviceCollection.AddAppsFromAssembly(typeof(MyAppLocalAppWithId).Assembly);

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var appModelcontext = serviceProvider.GetRequiredService<IAppModelContext>();
        await appModelcontext.InitializeAsync(CancellationToken.None);

        // ASSERT
        var app = (Application) appModelcontext.Applications.Single(a => a.Id == MyAppLocalAppWithId.Id);
        app.ApplicationContext!.Instance.Should().BeOfType<MyAppLocalAppWithId>();
    }

    [Fact]
    public async Task AddNetDaemonApp_GenericTypeArg()
    {
        // ARRANGE
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();

        // ACT
        serviceCollection.AddNetDaemonApp<MyAppLocalAppWithId>();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var appModelcontext = serviceProvider.GetRequiredService<IAppModelContext>();
        await appModelcontext.InitializeAsync(CancellationToken.None);

        // ASSERT
        appModelcontext.Applications.Single().Id.Should().Be(MyAppLocalAppWithId.Id);
        var app = (Application)appModelcontext.Applications.Single();
        app.ApplicationContext!.Instance.Should().BeOfType<MyAppLocalAppWithId>();
    }

    [Fact]
    public async Task AddNetDaemonApp_TypeParam()
    {
        // ARRANGE
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();

        // ACT
#pragma warning disable CA2263 // warning about preferring type argument instead of typeof
        serviceCollection.AddNetDaemonApp(typeof(MyAppLocalAppWithId));
#pragma warning restore CA2263

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var appModelcontext = serviceProvider.GetRequiredService<IAppModelContext>();
        await appModelcontext.InitializeAsync(CancellationToken.None);

        // ASSERT
        appModelcontext.Applications.Single().Id.Should().Be(MyAppLocalAppWithId.Id);
        var app = (Application)appModelcontext.Applications.Single();
        app.ApplicationContext!.Instance.Should().BeOfType<MyAppLocalAppWithId>();
    }
}
