using NetDaemon.AppModel.Internal;

namespace NetDaemon.AppModel.Tests.Internal;

public class AppInstanceFactoryTests
{
    [Fact]
    public void TestCreateInstantiatesApp()
    {
        // ARRANGE
        var provider = CreateServiceProvider();
        var factory = new AppInstanceFactory();

        // ACT
        var instance = factory.Create(provider, typeof(AppWithoutDependencies));

        // ASSERT
        Assert.NotNull(instance);
        Assert.IsAssignableFrom<AppWithoutDependencies>(instance);
    }

    [Fact]
    public void TestCreateInstantiatesAppWithDependencies()
    {
        // ARRANGE
        var provider = CreateServiceProvider(new AppDependency { Value = "Test Value" });
        var factory = new AppInstanceFactory();

        // ACT
        var instance = factory.Create(provider, typeof(AppWithDependencies));

        // ASSERT
        Assert.NotNull(instance);
        Assert.IsAssignableFrom<AppWithDependencies>(instance);
        Assert.Equal("Test Value", ((AppWithDependencies)instance).Dependency.Value);
    }

    private IServiceProvider CreateServiceProvider(AppDependency? dependency = default)
    {
        var services = new ServiceCollection();

        if (dependency is not null)
        {
            services.AddSingleton(dependency);
        }

        return services.BuildServiceProvider();
    }

    private class AppWithoutDependencies
    {
    }

    private class AppWithDependencies
    {
        public AppDependency Dependency { get; }

        public AppWithDependencies(AppDependency dependency)
        {
            Dependency = dependency;
        }
    }

    private class AppDependency
    {
        public string? Value { get; init; }
    }
}