using System.Reflection;
using LocalApps;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetDaemon.AppModel.Internal;
using NetDaemon.AppModel.Tests.Helpers;

namespace NetDaemon.AppModel.Tests.Internal;

public class AppModelTests
{
    [Fact]
    public async Task TestGetDynamicallyCompiledApplications()
    {
        // ARRANGE
        // ACT
        var loadApps = await TestHelpers.GetDynamicApplicationsFromYamlConfigPath("Fixtures/Dynamic");

        // CHECK
        loadApps.Should().HaveCount(1);
    }

    [Fact]
    public async Task TestGetDynamicallyCompiledApplicationsWithCompilerError()
    {
        // ACT and CHECK
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            TestHelpers.GetDynamicApplicationsFromYamlConfigPath("Fixtures/DynamicError"));
    }

    [Fact]
    public async Task TestGetApplicationsLocal()
    {
        // ARRANGE
        // ACT
        var loadApps = await TestHelpers.GetLocalApplicationsFromYamlConfigPath("Fixtures/Local").ConfigureAwait(false);

        // CHECK
        loadApps.Should().HaveCount(4);

        // check the application instance is init ok
        var application = (Application)loadApps.First(n => n.Id == "LocalApps.MyAppLocalApp");
        var instance = (MyAppLocalApp?)application.ApplicationContext?.Instance;
        instance!.Settings.AString.Should().Be("Hello world!");
    }

    internal class FakeAppStateManager : IAppStateManager
    {
        public ApplicationState? State { get; set; }
        public Task<ApplicationState> GetStateAsync(string applicationId)
        {
            if (applicationId == "LocalApps.MyAppLocalApp")
                return Task.FromResult(ApplicationState.Disabled);
            return Task.FromResult(ApplicationState.Enabled);
        }

        public Task SaveStateAsync(string applicationId, ApplicationState state)
        {
            State = state;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task TestGetApplicationsLocalWithDisabled()
    {
        // ARRANGE
        var builder = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                // get apps from test project
                services.AddAppsFromAssembly(Assembly.GetExecutingAssembly());
                services.AddSingleton<IAppStateManager, FakeAppStateManager>();
            })
            .ConfigureAppConfiguration((_, config) =>
            {
                config.AddYamlAppConfig(
                    Path.Combine(AppContext.BaseDirectory,
                        "Fixtures/Local"));
            })
            .Build();

        var fakeStateManager = (FakeAppStateManager?)builder.Services.GetService<IAppStateManager>();
        var appModel = builder.Services.GetService<IAppModel>();
        var appModelContext = await appModel!.InitializeAsync(CancellationToken.None).ConfigureAwait(false);

        // ACT
        var apps = appModelContext.Applications;

        // CHECK

        // check the application instance is init ok
        var application = (Application)apps.First(n => n.Id == "LocalApps.MyAppLocalApp");
        application.State.Should().Be(ApplicationState.Disabled);
        Assert.Null((MyAppLocalApp?)application.ApplicationContext?.Instance);

        // set state to enabled
        await application.SetStateAsync(ApplicationState.Enabled).ConfigureAwait(false);
        application.State.Should().Be(ApplicationState.Running);
        Assert.NotNull((MyAppLocalApp?)application.ApplicationContext?.Instance);

        fakeStateManager!.State.Should().Be(ApplicationState.Running);
    }
    
    [Fact]
    public async Task TestGetApplicationsLocalWithEnabled()
    {
        // ARRANGE
        var builder = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                // get apps from test project
                services.AddAppsFromAssembly(Assembly.GetExecutingAssembly());
            })
            .ConfigureAppConfiguration((_, config) =>
            {
                config.AddYamlAppConfig(
                    Path.Combine(AppContext.BaseDirectory,
                        "Fixtures/Local"));
            })
            .Build();

        var appModel = builder.Services.GetService<IAppModel>();
        var appModelContext = await appModel!.InitializeAsync(CancellationToken.None).ConfigureAwait(false);

        // ACT
        var apps = appModelContext.Applications;

        // CHECK

        // check the application instance is init ok
        var application = (Application)apps.First(n => n.Id == "LocalApps.MyAppLocalAppWithDispose");
        application.State.Should().Be(ApplicationState.Running);
        Assert.NotNull((MyAppLocalAppWithDispose?)application.ApplicationContext?.Instance);

        // set state to enabled
        await application.SetStateAsync(ApplicationState.Disabled).ConfigureAwait(false);
        application.State.Should().Be(ApplicationState.Disabled);
        Assert.Null((MyAppLocalAppWithDispose?)application.ApplicationContext?.Instance);
    }

    [Fact]
    public async Task TestGetApplicationsWithIdSet()
    {
        // ARRANGE
        // ACT
        var loadApps = await TestHelpers.GetLocalApplicationsFromYamlConfigPath("Fixtures/Local").ConfigureAwait(false);

        // CHECK
        var appContext = loadApps.Where(n => n.Id == "SomeId");
        appContext.Should().NotBeEmpty();
    }

    [Fact]
    public async Task TestSetStateToRunningShouldThrowException()
    {
        // ARRANGE
        var loggerMock = new Mock<ILogger<IApplication>>();
        var providerMock = new Mock<IServiceProvider>();
        // ACT
        var app = new Application("", typeof(object), loggerMock.Object, providerMock.Object);

        // CHECK
        await Assert.ThrowsAsync<ArgumentException>(() => app.SetStateAsync(ApplicationState.Running));
    }

    [Fact]
    public async Task TestGetApplicationsShouldReturnNonErrorOnes()
    {
        var loggerMock = new Mock<ILogger<IApplication>>();

        // ARRANGE
        var builder = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                // get apps from test project
                services.AddAppsFromAssembly(Assembly.GetExecutingAssembly());
                services.AddTransient<IOptions<ApplicationLocationSetting>>(
                    _ => new FakeOptions(Path.Combine(AppContext.BaseDirectory, "Fixtures/LocalError")));
                services.AddTransient(_ => loggerMock.Object);
            })
            .ConfigureAppConfiguration((_, config) =>
            {
                config.AddYamlAppConfig(
                    Path.Combine(AppContext.BaseDirectory,
                        "Fixtures/LocalError"));
            })
            .Build();
        var appModel = builder.Services.GetService<IAppModel>();

        // ACT
        var loadApps = (await appModel!
            .InitializeAsync(CancellationToken.None)).Applications;


        // CHECK
        loadApps.Where(n => n.Id == "LocalAppsWithErrors.MyAppLocalAppWithError").First()
            .State.Should().Be(ApplicationState.Error);

        // Verify that the error is logged
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((_, __) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((_, _) => true)), Times.Once);
    }

    [Fact]
    public async Task TestGetApplicationsLocalWith()
    {
        // ARRANGE
        // ACT
        var loadApps = await TestHelpers.GetLocalApplicationsFromYamlConfigPath("Fixtures/Local");

        // CHECK

        // check the application instance is init ok
        var application = (Application)loadApps.First(n => n.Id == "LocalApps.MyAppLocalAppWithDispose");
        var app = (MyAppLocalAppWithDispose?)application.ApplicationContext?.Instance;
        application.State.Should().Be(ApplicationState.Running);
        await application.DisposeAsync().ConfigureAwait(false);
        app!.AsyncDisposeIsCalled.Should().BeTrue();
        app.DisposeIsCalled.Should().BeTrue();
    }
}