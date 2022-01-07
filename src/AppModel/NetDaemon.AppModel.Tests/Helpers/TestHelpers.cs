using System.Reflection;
using Microsoft.Extensions.Hosting;

namespace NetDaemon.AppModel.Tests.Helpers;

public static class TestHelpers
{
    internal static async Task<IReadOnlyCollection<IApplication>> GetLocalApplicationsFromYamlConfigPath(string path)
    {
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
                        path));
            })
            .Build();
        var appModel = builder.Services.GetService<IAppModel>();
        var appModelContext = await appModel!.InitializeAsync(CancellationToken.None).ConfigureAwait(false);
        return appModelContext.Applications;
    }

    internal static async Task<IReadOnlyCollection<IApplication>> GetDynamicApplicationsFromYamlConfigPath(string path)
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
        var appModelContext = await appModel!.InitializeAsync(CancellationToken.None).ConfigureAwait(false);
        return appModelContext.Applications;
    }
}