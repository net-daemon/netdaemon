using NetDaemon.AppModel.Internal.AppFactoryProviders;
using NetDaemon.AppModel.Tests.Helpers;

namespace NetDaemon.AppModel.Tests.AppFactoryProviders;

public class DynamicAppFactoryProviderTests
{
    [Fact]
    public void TestDynamicAssemblyAppFactoriesAreProvidedWithoutFocus()
    {
        // ARRANGE
        var serviceProvider = CreateServiceProvider();

        // ACT
        var appFactoryProviders = serviceProvider.GetRequiredService<IEnumerable<IAppFactoryProvider>>();
        var appFactories = appFactoryProviders.SelectMany(provider => provider.GetAppFactories()).ToList();

        // ASSERT
        appFactories.Should().Contain(factory => factory.Id == "Apps.NonFocusApp" &&
                                                 factory.HasFocus == false);
    }

    [Fact]
    public void TestDynamicAssemblyAppFactoriesAreProvidedWithFocus()
    {
        // ARRANGE
        var serviceProvider = CreateServiceProvider();

        // ACT
        var appFactoryProviders = serviceProvider.GetRequiredService<IEnumerable<IAppFactoryProvider>>();
        var appFactories = appFactoryProviders.SelectMany(provider => provider.GetAppFactories()).ToList();

        // ASSERT
        appFactories.Should().Contain(factory => factory.Id == "Apps.MyFocusApp" &&
                                                 factory.HasFocus == true);
    }

    private static IServiceProvider CreateServiceProvider()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddFakeOptions("DynamicWithFocus");
        serviceCollection.AddAppsFromSource();

        return serviceCollection.BuildServiceProvider();
    }
}