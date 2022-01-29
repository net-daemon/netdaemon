namespace NetDaemon.AppModel.Tests.Helpers;

internal class FakeOptions : IOptions<AppConfigurationLocationSetting>
{
    public FakeOptions(string path)
    {
        Value = new AppConfigurationLocationSetting {ApplicationConfigurationFolder = path};
    }

    public AppConfigurationLocationSetting Value { get; init; }
}
