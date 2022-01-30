using System.Reflection;
using NetDaemon.AppModel.Internal.AppAssemblyProviders;
using NetDaemon.AppModel.Tests.Helpers;

namespace NetDaemon.AppModel.Tests.AppAssemblyProviders;

public class DynamicAppAssemblyProviderTests
{
    [Fact]
    public void TestDynamicallyCompiledAssembliesAreProvided()
    {
        // ARRANGE
        var serviceProvider = CreateServiceProvider();
        
        // ACT
        var assemblyProviders = serviceProvider.GetRequiredService<IEnumerable<IAppAssemblyProvider>>();
        var assemblies = assemblyProviders.Select(provider => provider.GetAppAssembly()).ToList();

        // ASSERT
        assemblies.Should().Contain(assembly => CheckAssemblyHasType(assembly, "Apps.MyApp"));
    }

    private static bool CheckAssemblyHasType(Assembly assembly, string name)
    {
        var type = assembly.GetType(name);
        return type?.FullName == name;
    }

    private static IServiceProvider CreateServiceProvider()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddFakeOptions("Dynamic");
        serviceCollection.AddAppsFromSource();

        return serviceCollection.BuildServiceProvider();
    }
}