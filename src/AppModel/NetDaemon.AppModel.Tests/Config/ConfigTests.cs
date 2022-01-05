using System.Reflection;
using LocalApps;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NetDaemon.AppModel.Internal;

namespace NetDaemon.AppModel.Tests.Config;

public class ConfigTests
{
    [Fact]
    public void TestAddJsonConfigGetsSettingsCorrectly()
    {
        // ARRANGE

        var configRoot = GetConfigurationRootForJson("Config/Fixtures");

        // ACT
        var settings = configRoot.GetSection("TestConfig").Get<TestSettings>();

        // CHECK
        settings.AString
            .Should()
            .Be("Hello test!");
    }

    [Fact]
    public void TestAddYamlConfigGetsSettingsCorrectly()
    {
        // ARRANGE
        var configRoot = GetConfigurationRootForYaml("Config/Fixtures");

        // ACT
        var settings = configRoot.GetSection("TestConfig").Get<TestSettings>();

        // CHECK
        settings.AString
            .Should()
            .Be("Hello test!");
    }

    [Fact]
    public void TestDuplicateKeyShouldThrowInvalidDataException()
    {
        // ARRANGE
        var configurationBuilder = new ConfigurationBuilder() as IConfigurationBuilder;

        configurationBuilder.AddYamlFile(Path.Combine(AppContext.BaseDirectory,
            "Config/FailedConfig", "Fail.yaml"), false, false);

        // ACT
        // CHECK
        Assert.Throws<InvalidDataException>(() => configurationBuilder.Build());
    }

    [Fact]
    public void TestAppGetCorrectJsonConfigInjected()
    {
        // ARRANGE
        var builder = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                // get apps from test project
                services.AddAppsFromAssembly(Assembly.GetCallingAssembly());
                services.AddTransient<InjectMeWithConfigPlease>();
            })
            .ConfigureAppConfiguration((_, config) =>
            {
                config.AddJsonAppConfig(
                    Path.Combine(AppContext.BaseDirectory,
                        "Config/Fixtures"));
            })
            .Build();

        // ACT
        var cfgClass = builder.Services.GetService<InjectMeWithConfigPlease>();

        // CHECK
        cfgClass!.Settings.Value.AString.Should().Be("Hello test!");
    }

    [Fact]
    public void TestAppGetCorrectYamlConfigInjected()
    {
        // ARRANGE
        var builder = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                // get apps from test project
                services.AddAppsFromAssembly(Assembly.GetCallingAssembly());
                services.AddTransient<InjectMeWithConfigPlease>();
            })
            .ConfigureAppConfiguration((_, config) =>
            {
                config.AddYamlAppConfig(
                    Path.Combine(AppContext.BaseDirectory,
                        "Config/Fixtures"));
            })
            .Build();

        // ACT
        var cfgClass = builder.Services.GetService<InjectMeWithConfigPlease>();

        // CHECK
        cfgClass!.Settings.Value.AString.Should().Be("Hello test yaml!");
    }

    [Fact]
    public void TestAddYamlConfigWithTypeConverterGetsSettingsCorrectly2()
    {
        // ARRANGE
        var builder = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                // get apps from test project
                services.AddAppsFromAssembly(Assembly.GetExecutingAssembly());
                services.AddSingleton<IInjectMePlease, InjectMeImplementation>();
                services.AddSingleton(sp =>
                    new Converter<string, EntityClass>(s =>
                        ActivatorUtilities.CreateInstance<EntityClass>(sp, s)));
            })
            .ConfigureAppConfiguration((_, config) =>
            {
                config.AddYamlAppConfig(
                    Path.Combine(AppContext.BaseDirectory,
                        "Fixtures/Local"));
            })
            .Build();

        var appModel = builder.Services.GetService<IAppModel>();

        // ACT
        var loadApps = appModel!.LoadApplications();
        var appCtx = (ApplicationContext)loadApps.First(n => n.Id == "LocalApps.MyAppLocalApp");
        var app = (MyAppLocalApp)appCtx.Instance;
        // CHECK
        loadApps.Should().HaveCount(3);
        app.Settings.Entity!.EntityId.Should().Be("light.test");
        app.Settings.Entity!.ServiceProvider.Should().NotBeNull();
    }

    private static IConfigurationRoot GetConfigurationRootForYaml(string path)
    {
        return GetConfigurationRoot(path);
    }

    private static IConfigurationRoot GetConfigurationRootForJson(string path)
    {
        return GetConfigurationRoot(path, false);
    }

    private static IConfigurationRoot GetConfigurationRoot(string path, bool yaml = true)
    {
        var configurationBuilder = new ConfigurationBuilder() as IConfigurationBuilder;

        if (yaml)
            configurationBuilder.AddYamlAppConfig(
                Path.Combine(AppContext.BaseDirectory,
                    path));
        else
            configurationBuilder.AddJsonAppConfig(
                Path.Combine(AppContext.BaseDirectory,
                    path));
        return configurationBuilder.Build();
    }
}

#region -- Test classes --

internal class InjectMeWithConfigPlease
{
    public InjectMeWithConfigPlease(IAppConfig<TestSettings> settings)
    {
        Settings = settings;
    }

    public IAppConfig<TestSettings> Settings { get; }
}

internal class TestSettings
{
    public string AString { get; set; } = string.Empty;
}

#endregion