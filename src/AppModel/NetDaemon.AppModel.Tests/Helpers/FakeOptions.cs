namespace NetDaemon.AppModel.Tests.Helpers;

internal class FakeOptions : IOptions<ApplicationLocationSetting>
{
    public FakeOptions(string path)
    {
        Value = new ApplicationLocationSetting {ApplicationFolder = path};
    }

    public ApplicationLocationSetting Value { get; init; }
}