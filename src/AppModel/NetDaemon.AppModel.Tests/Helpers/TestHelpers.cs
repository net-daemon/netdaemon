using Microsoft.Extensions.Hosting;

namespace NetDaemon.AppModel.Tests.Helpers;

public static class TestHelpers
{
    internal static IReadOnlyCollection<IApplicationInstance> GetLocalApplicationsFromYamlConfigPath(string path)
    {
        var builder = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                services.AddAppFromLocalAssembly();
                services.AddTransient<IOptions<ApplicationLocationSetting>>(
                    _ => new FakeOptions(Path.Combine(AppContext.BaseDirectory, path)));
            })
            .ConfigureAppConfiguration((_, config) =>
            {
                config.AddYamlAppConfig(
                    Path.Combine(AppContext.BaseDirectory,
                        path));
            })
            .Build();
        var appModel = builder.Services.GetService<IAppModel>();

        return appModel!.LoadApplications();
    }

    internal static IReadOnlyCollection<IApplicationInstance> GetDynamicApplicationsFromYamlConfigPath(string path)
    {
        var builder = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                services.AddAppsFromSource();
                services.AddTransient<IOptions<ApplicationLocationSetting>>(
                    _ => new FakeOptions(Path.Combine(AppContext.BaseDirectory, path)));
            })
            .ConfigureAppConfiguration((_, config) =>
            {
                config.AddYamlAppConfig(
                    Path.Combine(AppContext.BaseDirectory,
                        path));
            })
            .Build();
        var appModel = builder.Services.GetService<IAppModel>();

        return appModel!.LoadApplications();
    }

    internal static IAppModel GetAppModelFromLocalAssembly(string path)
    {
        var builder = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                services.AddAppFromLocalAssembly();
                services.AddTransient<IOptions<ApplicationLocationSetting>>(
                    _ => new FakeOptions(Path.Combine(AppContext.BaseDirectory, path)));
            })
            .ConfigureAppConfiguration((_, config) =>
            {
                config.AddYamlAppConfig(
                    Path.Combine(AppContext.BaseDirectory,
                        path));
            })
            .Build();
        return builder.Services.GetService<IAppModel>() ?? throw new InvalidOperationException();
    }
}