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
        loadApps.Should().HaveCount(3);

        // check the application instance is init ok
        var application = (Application)loadApps.First(n => n.Id == "LocalApps.MyAppLocalApp");
        var instance = (MyAppLocalApp?)application?.ApplicationContext?.Instance;
        instance!.Settings.AString.Should().Be("Hello world!");
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
    public async Task TestGetApplicationsShouldReturnNonErrorOnes()
    {
        var loggerMock = new Mock<ILogger<IAppModelContext>>();

        // ARRANGE
        var builder = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                // get apps from test project
                services.AddAppsFromAssembly(Assembly.GetExecutingAssembly());
                services.AddTransient<IOptions<ApplicationLocationSetting>>(
                    _ => new FakeOptions(Path.Combine(AppContext.BaseDirectory, "Fixtures/LocalError")));
                services.AddTransient(n => loggerMock.Object);
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
        loadApps.Where(n => n.Id == "LocalAppsWithErrors.MyAppLocalAppWithError").Should().BeEmpty();

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
        var app = (MyAppLocalAppWithDispose?)application?.ApplicationContext?.Instance;
        application!.State.Should().Be(ApplicationState.Enabled);
        await application!.DisposeAsync().ConfigureAwait(false);
        app!.AsyncDisposeIsCalled.Should().BeTrue();
        app!.DisposeIsCalled.Should().BeTrue();
    }
}