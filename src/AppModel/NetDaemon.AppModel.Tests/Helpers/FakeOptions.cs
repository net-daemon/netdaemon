namespace NetDaemon.AppModel.Tests.Helpers;

internal class FakeOptions<T> : IOptions<T> where T: class
{
    public FakeOptions(T settings)
    {
        Value = settings;
    }

    public T Value { get; init; }
}