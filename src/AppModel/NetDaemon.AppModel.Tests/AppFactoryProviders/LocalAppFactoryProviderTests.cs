using System.Reflection;
using LocalApps;
using NetDaemon.AppModel.Internal.AppFactoryProviders;

namespace NetDaemon.AppModel.Tests.AppFactoryProviders;

public class LocalAppFactoryProviderTests
{
    [Fact]
    public void TestLocalAssemblyAppFactoriesAreProvidedWithFullNameId()
    {
        // ARRANGE
        var serviceProvider = CreateServiceProvider(typeof(MyAppLocalApp).Assembly);
        
        // ACT
        var appFactoryProviders = serviceProvider.GetRequiredService<IEnumerable<IAppFactoryProvider>>();
        var appFactories = appFactoryProviders.SelectMany(provider => provider.GetAppFactories()).ToList();
        
        // ASSERT
        appFactories.Should().Contain(factory => factory.Id == MyAppLocalApp.Id);
    }
    
    [Fact]
    public void TestLocalAssemblyAppFactoriesAreProvidedWithCustomId()
    {
        // ARRANGE
        var serviceProvider = CreateServiceProvider(typeof(MyAppLocalAppWithId).Assembly);
        
        // ACT
        var appFactoryProviders = serviceProvider.GetRequiredService<IEnumerable<IAppFactoryProvider>>();
        var appFactories = appFactoryProviders.SelectMany(provider => provider.GetAppFactories()).ToList();
        
        // ASSERT
        appFactories.Should().Contain(factory => factory.Id == MyAppLocalAppWithId.Id);
    }
    
    [Fact]
    public void TestLocalAssemblyAppFactoriesAreProvidedWithoutFocus()
    {
        // ARRANGE
        var serviceProvider = CreateServiceProvider(typeof(MyAppLocalApp).Assembly);
        
        // ACT
        var appFactoryProviders = serviceProvider.GetRequiredService<IEnumerable<IAppFactoryProvider>>();
        var appFactories = appFactoryProviders.SelectMany(provider => provider.GetAppFactories()).ToList();
        
        // ASSERT
        appFactories.Should().Contain(factory => factory.Id == MyAppLocalApp.Id &&
                                                 factory.HasFocus == false);
    }
    
    [Fact]
    public void TestLocalSingleAppFactoriesAreProvidedWithFullNameId()
    {
        // ARRANGE
        var serviceProvider = CreateServiceProvider<MyAppLocalApp>();
        
        // ACT
        var appFactoryProviders = serviceProvider.GetRequiredService<IEnumerable<IAppFactoryProvider>>();
        var appFactories = appFactoryProviders.SelectMany(provider => provider.GetAppFactories()).ToList();
        
        // ASSERT
        appFactories.Should().Contain(factory => factory.Id == MyAppLocalApp.Id);
    }

    [Fact]
    public void TestLocalSingleAppFactoriesAreProvidedWithCustomId()
    {
        // ARRANGE
        var serviceProvider = CreateServiceProvider<MyAppLocalAppWithId>();
        
        // ACT
        var appFactoryProviders = serviceProvider.GetRequiredService<IEnumerable<IAppFactoryProvider>>();
        var appFactories = appFactoryProviders.SelectMany(provider => provider.GetAppFactories()).ToList();
        
        // ASSERT
        appFactories.Should().Contain(factory => factory.Id == MyAppLocalAppWithId.Id);
    }
    
    [Fact]
    public void TestLocalSingleAppFactoriesAreProvidedWithoutFocus()
    {
        // ARRANGE
        var serviceProvider = CreateServiceProvider<MyAppLocalApp>();
        
        // ACT
        var appFactoryProviders = serviceProvider.GetRequiredService<IEnumerable<IAppFactoryProvider>>();
        var appFactories = appFactoryProviders.SelectMany(provider => provider.GetAppFactories()).ToList();
        
        // ASSERT
        appFactories.Should().Contain(factory => factory.Id == MyAppLocalApp.Id &&
                                                 factory.HasFocus == false);
    }
    
    private static IServiceProvider CreateServiceProvider(Assembly assembly)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddAppsFromAssembly(assembly);
        
        return serviceCollection.BuildServiceProvider();
    }
    
    private static IServiceProvider CreateServiceProvider<TAppType>()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddAppFromType<TAppType>();
        
        return serviceCollection.BuildServiceProvider();
    }
}
