using System.Reactive.Disposables;
using Microsoft.Extensions.Hosting;
using NetDaemon.AppModel.Internal;
using NetDaemon.AppModel.Internal.AppFactories;

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
        serviceCollection.AddNetDaemonApp("SomeID", () => { appStarted = true; });

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
        serviceCollection.AddNetDaemonApp("SomeID", (Dependency1 d1, Dependency2 d2) =>
        {
            d1.Value = "Value1 From App";
            d2.Value = "Value2 From App";
        });

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
        serviceCollection.AddNetDaemonApp("FirstApp", (Dependency1 d1) =>
        {
            firstAppDep = d1;
            d1.Value = "Value1 From App1";
        });

        serviceCollection.AddNetDaemonApp("SecondApp", (Dependency1 d1) =>
        {
            secondAppDep = d1;
            d1.Value = "Value1 From App2";
        });

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
        serviceCollection.AddNetDaemonApp("AppId", () => Disposable.Create(() => isDisposed = true));

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
        serviceCollection.AddNetDaemonApp("App1", () => { app1Started = true; }, focus: true);
        serviceCollection.AddNetDaemonApp("App2", () => { app2Started = true; });

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var appModelcontext = serviceProvider.GetRequiredService<AppModelContext>();
        await appModelcontext.InitializeAsync(CancellationToken.None);

        // ASSERT
        app1Started.Should().BeTrue();
        app2Started.Should().BeFalse();
    }


    [Fact]
    public async Task AddNetDaemonApp_AsyncAction()
    {
        // ARRANGE
        var serviceCollection = new ServiceCollection();
        bool appStarted = false;
        bool appInitialized = false;

        TaskCompletionSource app1StartedTcs = new();

        // ACT
        var factory = new FuncAppFactory(async () =>
        {
            appStarted = true;
            await app1StartedTcs.Task;
            appInitialized = true;
        }, "app1", true);

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var appContext = new ApplicationContext(serviceProvider, factory);
        appStarted.Should().BeTrue();
        appInitialized.Should().BeFalse();

        app1StartedTcs.SetResult();
        await appContext.InitializeAsync();
        appInitialized.Should().BeTrue();
    }

    [Fact]
    public async Task AddNetDaemonApp_AsyncFuncOfDisposable_AwaitsTaskAndDisposesResult()
    {
        // ARRANGE
        var serviceCollection = new ServiceCollection();
        bool appDisposed = false;

        TaskCompletionSource<IDisposable> app1StartedTcs = new();

        async Task<IDisposable> Handler() => await app1StartedTcs.Task;

        var factory = new FuncAppFactory(Handler, "app1", false);

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var appContext = new ApplicationContext(serviceProvider, factory);

        // Make the FuncApp return an IDisposable that should be disposed when the Context is Disposed
        app1StartedTcs.SetResult(Disposable.Create(()=> appDisposed = true));
        await appContext.InitializeAsync();

        appContext.Instance.Should().BeAssignableTo<IDisposable>();

        appDisposed.Should().BeFalse();
        await appContext.DisposeAsync();
        appDisposed.Should().BeTrue();
    }

    private sealed class Dependency1
    {
        public string? Value { get; set; }
    }

    private sealed class Dependency2
    {
        public string? Value { get; set; }
    }
}
