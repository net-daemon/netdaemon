using LocalApps;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetDaemon.AppModel.Internal;
using NetDaemon.AppModel.Tests.Helpers;

namespace NetDaemon.AppModel.Tests.Internal;

public class AppModelTests
{
    [Fact]
    public void TestGetDynamicallyCompiledApplications()
    {
        // ARRANGE
        // ACT
        var loadApps = TestHelpers.GetDynamicApplicationsFromYamlConfigPath("Fixtures/Dynamic");

        // CHECK
        loadApps.Should().HaveCount(1);
    }

    [Fact]
    public void TestGetDynamicallyCompiledApplicationsWithCompilerError()
    {
        // ACT and CHECK
        Assert.Throws<InvalidOperationException>(() =>
            TestHelpers.GetDynamicApplicationsFromYamlConfigPath("Fixtures/DynamicError"));
    }

    [Fact]
    public void TestGetApplicationsLocal()
    {
        // ARRANGE
        // ACT
        var loadApps = TestHelpers.GetLocalApplicationsFromYamlConfigPath("Fixtures/Local");


        // CHECK
        loadApps.Should().HaveCount(3);

        // check the application instance is init ok
        var appInstance = (ApplicationContext)loadApps.First(n => n.Id == "LocalApps.MyAppLocalApp");
        var app = (MyAppLocalApp)appInstance.Instance;
        app.Settings.AString.Should().Be("Hello world!");
    }

    [Fact]
    public void TestGetApplicationsWithIdSet()
    {
        // ARRANGE
        // ACT
        var loadApps = TestHelpers.GetLocalApplicationsFromYamlConfigPath("Fixtures/Local");

        // CHECK
        var appInstance = loadApps.Where(n => n.Id == "SomeId");
        appInstance.Should().NotBeEmpty();
    }

    [Fact]
    public void TestGetApplicationsShouldReturnNonErrorOnes()
    {
        var loggerMock = new Mock<ILogger<IAppModel>>();

        // ARRANGE
        var builder = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                services.AddAppModelLocalAssembly();
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
        var loadApps = appModel!.LoadApplications();


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
    public void TestSkippedApplications()
    {
        var appModel = TestHelpers.GetAppModelFromLocalAssembly("Fixtures/Local");
        var apps = appModel.LoadApplications(new List<string> { "LocalApps.MyAppLocalApp" })
            .Where(n => n.Id == "LocalApps.MyAppLocalApp");
        apps.Should().BeEmpty();
    }

    [Fact]
    public async Task TestGetApplicationsLocalWith()
    {
        // ARRANGE
        // ACT
        var loadApps = TestHelpers.GetLocalApplicationsFromYamlConfigPath("Fixtures/Local");

        // CHECK

        // check the application instance is init ok
        var appInstance = (IApplicationContext)loadApps.First(n => n.Id == "LocalApps.MyAppLocalAppWithDispose");
        var app = (MyAppLocalAppWithDispose)appInstance.Instance;
        await appInstance.DisposeAsync().ConfigureAwait(false);
        app.AsyncDisposeIsCalled.Should().BeTrue();
        app.DisposeIsCalled.Should().BeTrue();
    }
}