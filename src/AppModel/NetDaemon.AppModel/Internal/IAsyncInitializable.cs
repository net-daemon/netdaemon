namespace NetDaemon.AppModel.Internal;

public interface IAsyncInitializable
{
    Task InitializeAsync(CancellationToken cancellationToken);
}