using System.Reflection;
using LocalApps;
using NetDaemon.AppModel.Internal.AppAssemblyProviders;
using NetDaemon.AppModel.Tests.Helpers;

namespace NetDaemon.AppModel.Tests.AppAssemblyProviders;

public class CombinedAppAssemblyProviderTests
{
    [Fact]
    public void TestCombinedAssembliesAreProvided()
    {
        // ARRANGE
        var serviceProvider = CreateServiceProvider(typeof(MyAppLocalApp).Assembly);
        
        // ACT
        var assemblyProviders = serviceProvider.GetRequiredService<IEnumerable<IAppAssemblyProvider>>();
        var assemblies = assemblyProviders.Select(provider => provider.GetAppAssembly()).ToList();

        // ASSERT
        assemblies.Should().Contain(assembly => CheckAssemblyHasType(assembly, MyAppLocalApp.Id));
        assemblies.Should().Contain(assembly => CheckAssemblyHasType(assembly, "Apps.MyApp"));
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
        serviceCollection.AddFakeOptions("Dynamic");
        serviceCollection.AddAppsFromAssembly(assembly);
        serviceCollection.AddAppsFromSource();

        return serviceCollection.BuildServiceProvider();
    }
}