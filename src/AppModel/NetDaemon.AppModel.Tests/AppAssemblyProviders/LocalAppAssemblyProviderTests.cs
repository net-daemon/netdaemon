using System.Reflection;
using LocalApps;
using NetDaemon.AppModel.Internal.AppAssemblyProviders;

namespace NetDaemon.AppModel.Tests.AppAssemblyProviders;

public class LocalAppAssemblyProviderTests
{
    [Fact]
    public void TestLocalAppAssembliesAreProvided()
    {
        // ARRANGE
        var serviceProvider = CreateServiceProvider(typeof(MyAppLocalApp).Assembly);

        // ACT
        var assemblyProviders = serviceProvider.GetRequiredService<IEnumerable<IAppAssemblyProvider>>();
        var assemblies = assemblyProviders.Select(provider => provider.GetAppAssembly()).ToList();

        // ASSERT
        assemblies.Should().Contain(assembly => CheckAssemblyHasType(assembly, MyAppLocalApp.Id));
    }

    private static bool CheckAssemblyHasType(Assembly assembly, string name)
    {
        var type = assembly.GetType(name);
        return type?.FullName == name;
    }

    private static IServiceProvider CreateServiceProvider(Assembly assembly)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddAppsFromAssembly(assembly);

        return serviceCollection.BuildServiceProvider();
    }
}