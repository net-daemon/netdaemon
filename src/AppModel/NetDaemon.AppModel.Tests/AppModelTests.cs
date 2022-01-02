using NetDaemon.AppModel.Internal;
using NetDaemon.AppModel.Tests.Helpers;
using LocalApps;

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
        Assert.Throws<InvalidOperationException>(() => TestHelpers.GetDynamicApplicationsFromYamlConfigPath("Fixtures/DynamicError"));
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
    public void TestSkippedApplications()
    {
        var appModel = TestHelpers.GetAppModelFromLocalAssembly("Fixtures/Local");
        var apps = appModel.LoadApplications(new List<string> { "LocalApps.MyAppLocalApp" }).Where(n => n.Id == "LocalApps.MyAppLocalApp");
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

