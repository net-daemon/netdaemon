namespace NetDaemon.AppModel;

public interface IAsyncInitializable
{
    Task InitializeAsync(CancellationToken cancellationToken);
}