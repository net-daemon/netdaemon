using System.Reflection;
using LocalApps;
using NetDaemon.AppModel.Internal.AppFactoryProviders;
using NetDaemon.AppModel.Tests.Helpers;

namespace NetDaemon.AppModel.Tests.AppFactoryProviders;

public class CombinedAppFactoryProviderTests
{
    [Fact]
    public void TestCombinedAssemblyAppFactoriesAreProvided()
    {
        // ARRANGE
        var serviceProvider = CreateServiceProvider<MyAppLocalApp>(typeof(MyAppLocalAppWithId).Assembly);
        
        // ACT
        var appFactoryProviders = serviceProvider.GetRequiredService<IEnumerable<IAppFactoryProvider>>();
        var appFactories = appFactoryProviders.SelectMany(provider => provider.GetAppFactories()).ToList();
        
        // ASSERT
        appFactories.Should().Contain(factory => factory.Id == MyAppLocalApp.Id);
        appFactories.Should().Contain(factory => factory.Id == MyAppLocalAppWithId.Id);
        appFactories.Should().Contain(factory => factory.Id == "Apps.MyApp");
    }

    private static IServiceProvider CreateServiceProvider<TAppType>(Assembly assembly)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddFakeOptions("Dynamic");
        serviceCollection.AddAppFromType<TAppType>();
        serviceCollection.AddAppsFromAssembly(assembly);
        serviceCollection.AddAppsFromSource();

        return serviceCollection.BuildServiceProvider();
    }
}
