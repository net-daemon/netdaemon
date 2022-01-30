using NetDaemon.AppModel.Internal.AppFactoryProviders;
using NetDaemon.AppModel.Tests.Helpers;

namespace NetDaemon.AppModel.Tests.AppFactories;

public class DynamicAppFactoryTests
{
    [Fact]
    public void TestDynamicAppFactoryCreatesApp()
    {
        // ARRANGE
        var serviceProvider = CreateServiceProvider();
        
        // ACT
        var appFactoryProviders = serviceProvider.GetRequiredService<IEnumerable<IAppFactoryProvider>>();
        var appFactories = appFactoryProviders.SelectMany(provider => provider.GetAppFactories()).ToList();
        var appFactory = appFactories.Single(factory => factory.Id == "Apps.InjectedApp");
        var appInstance = appFactory.Create(serviceProvider);
        
        // ASSERT
        appInstance.Should().NotBeNull();
        appInstance.GetType().FullName.Should().BeEquivalentTo("Apps.InjectedApp");
    }
    
    private static IServiceProvider CreateServiceProvider()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddFakeOptions("DynamicWithServiceCollection");
        serviceCollection.AddAppsFromSource();

        return serviceCollection.BuildServiceProvider();
    }
}
