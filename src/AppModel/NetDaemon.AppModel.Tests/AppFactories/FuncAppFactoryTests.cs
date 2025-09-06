using System.Reactive.Disposables;
using Microsoft.Extensions.Hosting;
using NetDaemon.AppModel.Internal;

namespace NetDaemon.AppModel.Tests.AppFactories;

public class FuncAppTests
{
    [Fact]
    public async Task AddNetDaemonApp_Func()
    {
        // ARRANGE
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();

        // ACT
        bool appStarted = false;
        serviceCollection.AddNetDaemonApp(() => { appStarted = true; }, "SomeID");

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var appModelcontext = serviceProvider.GetRequiredService<IAppModelContext>();
        await appModelcontext.InitializeAsync(CancellationToken.None);

        // ASSERT
        appStarted.Should().BeTrue();
        appModelcontext.Applications.Single().Id.Should().Be("SomeID");
    }

    [Fact]
    public async Task AddNetDaemonApp_Func_GetsInjectedArguments()
    {
        // ARRANGE
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        var dependency1 = new Dependency1();
        var dependency2 = new Dependency2();
        serviceCollection.AddSingleton(dependency1);
        serviceCollection.AddSingleton(dependency2);

        // ACT, the delegate gets the dependencies injected
        serviceCollection.AddNetDaemonApp((Dependency1 d1, Dependency2 d2) =>
        {
            d1.Value = "Value1 From App";
            d2.Value = "Value2 From App";
        }, "SomeID");

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var appModelcontext = serviceProvider.GetRequiredService<IAppModelContext>();
        await appModelcontext.InitializeAsync(CancellationToken.None);

        // ASSERT
        dependency1.Value.Should().Be("Value1 From App");
        dependency2.Value.Should().Be("Value2 From App");
    }


    [Fact]
    public async Task AddNetDaemonApp_Func_GetsScopedInjectedArguments()
    {
        // ARRANGE
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddScoped<Dependency1>();

        Dependency1? firstAppDep = null;
        Dependency1? secondAppDep = null;

        // ACT, the delegates get the dependencies injected
        serviceCollection.AddNetDaemonApp((Dependency1 d1) =>
        {
            firstAppDep = d1;
            d1.Value = "Value1 From App1";
        }, "FirstApp");

        serviceCollection.AddNetDaemonApp((Dependency1 d1) =>
        {
            secondAppDep = d1;
            d1.Value = "Value1 From App2";
        }, "SecondApp");

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var appModelcontext = serviceProvider.GetRequiredService<IAppModelContext>();
        await appModelcontext.InitializeAsync(CancellationToken.None);

        // ASSERT, the two apps should get different instances of the scoped dependency
        firstAppDep.Should().NotBeSameAs(secondAppDep);
        firstAppDep!.Value.Should().Be("Value1 From App1");
        secondAppDep!.Value.Should().Be("Value1 From App2");
    }

    [Fact]
    public async Task AddNetDaemonApp_Func_ResultGetsDisposed()
    {
        // ARRANGE
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        bool isDisposed = false;

        // ACT
        serviceCollection.AddNetDaemonApp(() => Disposable.Create(() => isDisposed = true), "AppId");

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var appModelcontext = serviceProvider.GetRequiredService<AppModelContext>();
        await appModelcontext.InitializeAsync(CancellationToken.None);

        // ASSERT
        isDisposed.Should().BeFalse();
        await appModelcontext.DisposeAsync();
        isDisposed.Should().BeTrue();
    }

    [Fact]
    public async Task AddNetDaemonApp_Func_OneFocusAppOtherNotRun()
    {
        // ARRANGE
        var hostEnviromentMock = new Mock<IHostEnvironment>();
        hostEnviromentMock.Setup(m => m.EnvironmentName).Returns(Environments.Development);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(hostEnviromentMock.Object);
        serviceCollection.AddLogging();
        bool app1Started = false;
        bool app2Started = false;

        // ACT
        serviceCollection.AddNetDaemonApp(() => { app1Started = true; }, "App1", focus: true);
        serviceCollection.AddNetDaemonApp(() => { app2Started = true; }, "App2");

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var appModelcontext = serviceProvider.GetRequiredService<AppModelContext>();
        await appModelcontext.InitializeAsync(CancellationToken.None);

        // ASSERT
        app1Started.Should().BeTrue();
        app2Started.Should().BeFalse();
    }

    private class Dependency1
    {
        public string? Value { get; set; }
    }

    private class Dependency2
    {
        public string? Value { get; set; }
    }
}
