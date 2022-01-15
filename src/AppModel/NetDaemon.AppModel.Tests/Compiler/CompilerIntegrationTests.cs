using NetDaemon.AppModel.Internal.Compiler;

namespace NetDaemon.AppModel.Tests.Internal.Compiler;

public class CompilerIntegrationTests
{
    [Fact]
    public void TestDynamicCompileHasType()
    {
        // ARRANGE
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddLogging();
        serviceCollection.AddOptions<AppConfigurationLocationSetting>()
            .Configure(options =>
            {
                options.ApplicationConfigurationFolder = Path.Combine(AppContext.BaseDirectory, "Compiler", "Fixtures");
            });
        serviceCollection.AddAppsFromSource();
        var provider = serviceCollection.BuildServiceProvider();

        var factory = provider.GetService<ICompilerFactory>();
        using var compiler = factory?.New();

        // ACT
        var (collectibleAssemblyLoadContext, compiledAssembly) = compiler?.Compile()
                                                                 ?? throw new NullReferenceException(
                                                                     "Not expected null");

        // CHECK
        compiledAssembly.FullName.Should().StartWith("daemon_apps_");
        var types = collectibleAssemblyLoadContext.Assemblies.SelectMany(n => n.GetTypes()).ToList();
        types.Where(n => n.Name == "SimpleApp").Should().HaveCount(1);
    }
}