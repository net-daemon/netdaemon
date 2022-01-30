using System.Reflection;
using LocalApps;
using NetDaemon.AppModel.Internal.AppFactoryProviders;

namespace NetDaemon.AppModel.Tests.AppFactories;

public class LocalAppFactoryTests
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

    [Fact]
    public void TestCustomAppFactoryCreatesApp()
    {
        // ARRANGE
        var serviceProvider = CreateServiceProvider(_ => new MyAppLocalApp(Mock.Of<IAppConfig<LocalTestSettings>>()));

        // ACT
        var appFactoryProviders = serviceProvider.GetRequiredService<IEnumerable<IAppFactoryProvider>>();
        var appFactories = appFactoryProviders.SelectMany(provider => provider.GetAppFactories()).ToList();
        var appFactory = appFactories.Single(factory => factory.Id == MyAppLocalApp.Id);
        var appInstance = appFactory.Create(serviceProvider);

        // ASSERT
        appInstance.Should().NotBeNull();
        appInstance.Should().BeOfType<MyAppLocalApp>();
    }
    
    private static IServiceProvider CreateServiceProvider(Assembly assembly)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddAppsFromAssembly(assembly);

        return serviceCollection.BuildServiceProvider();
    }

    private static IServiceProvider CreateServiceProvider<TAppType>(
        Func<IServiceProvider, TAppType> func,
        string? id = default,
        bool? focus = default) where TAppType : class
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddApp(func, id, focus);

        return serviceCollection.BuildServiceProvider();
    }
}