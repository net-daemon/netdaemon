using System.Reflection;
using LocalApps;
using NetDaemon.AppModel.Internal.AppFactoryProviders;

namespace NetDaemon.AppModel.Tests.AppFactories;

public class ClassAppFactoryTests
{
    [Fact]
    public void TestLocalAppFactoryCreatesApp()
    {
        // ARRANGE
        var serviceProvider = CreateServiceProvider(typeof(MyAppLocalAppWithId).Assembly);

        // ACT
        var appFactoryProviders = serviceProvider.GetRequiredService<IEnumerable<IAppFactoryProvider>>();
        var appFactories = appFactoryProviders.SelectMany(provider => provider.GetAppFactories()).ToList();
        var appFactory = appFactories.Single(factory => factory.Id == MyAppLocalAppWithId.Id);
        var appInstance = appFactory.Create(serviceProvider);

        // ASSERT
        appInstance.Should().NotBeNull();
        appInstance.Should().BeOfType<MyAppLocalAppWithId>();
    }

    private static IServiceProvider CreateServiceProvider(Assembly assembly)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddAppsFromAssembly(assembly);

        return serviceCollection.BuildServiceProvider();
    }
}
