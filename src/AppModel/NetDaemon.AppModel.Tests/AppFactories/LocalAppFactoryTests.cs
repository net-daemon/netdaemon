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
    public void TestCustomAppFactoryCreatesAppWithoutId()
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
    
    [Fact]
    public void TestCustomAppFactoryCreatesAppWithId()
    {
        // ARRANGE
        var serviceProvider = CreateServiceProvider(_ => new MyAppLocalAppWithId());

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
    public void TestCustomAppFactoryCreatesWithCustomId()
    {
        // ARRANGE
        var serviceProvider = CreateServiceProvider(_ => new MyAppLocalAppWithId(), "CustomId");

        // ACT
        var appFactoryProviders = serviceProvider.GetRequiredService<IEnumerable<IAppFactoryProvider>>();
        var appFactories = appFactoryProviders.SelectMany(provider => provider.GetAppFactories()).ToList();
        var appFactory = appFactories.Single(factory => factory.Id == "CustomId");
        var appInstance = appFactory.Create(serviceProvider);

        // ASSERT
        appInstance.Should().NotBeNull();
        appInstance.Should().BeOfType<MyAppLocalAppWithId>();
    }
    
    [Fact]
    public void TestCustomAppFactoryCreatesWithCustomFocusTrue()
    {
        // ARRANGE
        var serviceProvider = CreateServiceProvider(_ => new MyAppLocalAppWithId(), "CustomId", true);

        // ACT
        var appFactoryProviders = serviceProvider.GetRequiredService<IEnumerable<IAppFactoryProvider>>();
        var appFactories = appFactoryProviders.SelectMany(provider => provider.GetAppFactories()).ToList();
        var appFactory = appFactories.Single(factory => factory.Id == "CustomId" && factory.HasFocus);
        var appInstance = appFactory.Create(serviceProvider);

        // ASSERT
        appInstance.Should().NotBeNull();
        appInstance.Should().BeOfType<MyAppLocalAppWithId>();
    }
    
    [Fact]
    public void TestCustomAppFactoryCreatesWithCustomFocusFalse()
    {
        // ARRANGE
        var serviceProvider = CreateServiceProvider(_ => new MyAppLocalAppWithId(), "CustomId", false);

        // ACT
        var appFactoryProviders = serviceProvider.GetRequiredService<IEnumerable<IAppFactoryProvider>>();
        var appFactories = appFactoryProviders.SelectMany(provider => provider.GetAppFactories()).ToList();
        var appFactory = appFactories.Single(factory => factory.Id == "CustomId" && factory.HasFocus == false);
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