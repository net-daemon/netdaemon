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
    public void TestGetDynaimcallyCompiledApplicationsWithCompilerError()
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
        loadApps.Should().HaveCount(2);

        // check the application instance is init ok
        var appInstance = (ApplicationContext)loadApps.Where(n => n.Id == "LocalApps.MyAppLocalApp").First();
        var app = (MyAppLocalApp)appInstance.Instance!;
        app.Settings.AString.Should().Be("Hello world!");
    }

    [Fact]
    public async Task TestGetApplicationsLocalWith()
    {
        // ARRANGE
        // ACT
        var loadApps = TestHelpers.GetLocalApplicationsFromYamlConfigPath("Fixtures/Local");

        // CHECK

        // check the application instance is init ok
        var appInstance = (IApplicationContext)loadApps.Where(n => n.Id == "LocalApps.MyAppLocalAppWithDispose").First();
        var app = (MyAppLocalAppWithDispose)appInstance.Instance!;
        await appInstance.DisposeAsync().ConfigureAwait(false);
        app.AsyncDisposeIsCalled.Should().BeTrue();
        app.DisposeIsCalled.Should().BeTrue();
    }
}

