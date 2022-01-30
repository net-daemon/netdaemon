namespace NetDaemon.AppModel.Tests.Helpers;

internal static class FakeOptionsExtensions
{
    public static IServiceCollection AddFakeOptions(this IServiceCollection services, string name)
    {
        return services.AddTransient(_ => CreateFakeOptions(name));
    }

    private static IOptions<AppConfigurationLocationSetting> CreateFakeOptions(string name)
    {
        var path = Path.Combine(AppContext.BaseDirectory, Path.Combine(AppContext.BaseDirectory, $"Fixtures/{name}"));
        return new FakeOptions(path);
    }
}