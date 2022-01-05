namespace NetDaemon.AppModel.Internal;

internal class Application : IApplication
{
    public Application(
        string id,
        ApplicationContext? appContext = null
    )
    {
        Id = id;
        ApplicationContext = appContext;
    }

    // Used in tests
    internal ApplicationContext? ApplicationContext { get; }

    public string? Id { get; }

    public ApplicationState State => ApplicationContext is null ? ApplicationState.Disabled : ApplicationState.Enabled;

    public async ValueTask DisposeAsync()
    {
        if (ApplicationContext is not null) await ApplicationContext.DisposeAsync().ConfigureAwait(false);
    }
}