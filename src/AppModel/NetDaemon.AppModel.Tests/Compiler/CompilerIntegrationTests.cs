
using NetDaemon.AppModel.Internal.Compiler;

namespace NetDaemon.AppModel.Tests.Internal.Compiler;

public class CompilerIntegrationTests
{
    [Fact]
    public void TestDynamicCompileHasType()
    {
        // ARRANGE
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddAppModelDynamicCompliedAssembly();
        serviceCollection.AddOptions<ApplicationLocationSetting>()
            .Configure(options =>
            {
                options.ApplicationFolder = Path.Combine(AppContext.BaseDirectory, "Compiler", "Fixtures");
            });
        serviceCollection.AddLogging();
        var provider = serviceCollection.BuildServiceProvider();

        var factory = provider.GetService<ICompilerFactory>();
        using var compiler = factory?.New();

        // ACT
        var compilerResult = compiler?.Compile() ?? throw new NullReferenceException("Not expected null");

        // CHECK
        compilerResult.CompiledAssembly.FullName.Should().StartWith("daemon_apps_");
        var types = compilerResult.AssemblyContext.Assemblies.SelectMany(n => n.GetTypes()).ToList();
        types.Where(n => n.Name == "SimpleApp").Should().HaveCount(1);
    }

}