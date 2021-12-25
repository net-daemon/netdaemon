using Microsoft.Extensions.Hosting;

namespace NetDaemon.AppModel.Tests.Helpers;

public static class TestHelpers
{
    internal static IReadOnlyCollection<IApplicationInstance> GetLocalApplicationsFromYamlConfigPath(string path)
    {
        var builder = Host.CreateDefaultBuilder()
                    .ConfigureServices((context, services) =>
                    {
                        services.AddAppModelLocalAssembly();
                        services.AddTransient<IOptions<ApplicationLocationSetting>>(
                            _ => new FakeOptions(Path.Combine(AppContext.BaseDirectory, path)));
                    })
                    .ConfigureAppConfiguration((hostingContext, config) =>
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
                    .ConfigureServices((context, services) =>
                    {
                        services.AddAppModelDynamicCompliedAssembly();
                        services.AddTransient<IOptions<ApplicationLocationSetting>>(
                            _ => new FakeOptions(Path.Combine(AppContext.BaseDirectory, path)));
                    })
                    .ConfigureAppConfiguration((hostingContext, config) =>
                    {
                        config.AddYamlAppConfig(
                            Path.Combine(AppContext.BaseDirectory,
                                path));
                    })
                    .Build();
        var appModel = builder.Services.GetService<IAppModel>();

        return appModel!.LoadApplications();
    }
}
