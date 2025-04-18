namespace NetDaemon.HassModel.Internal;

internal class CacheManager(EntityStateCache entityStateCache, RegistryCache registryCache)
    : ICacheManager
{
    private readonly TaskCompletionSource<object?> _initializationTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await entityStateCache.InitializeAsync(cancellationToken).ConfigureAwait(false);
        await registryCache.InitializeAsync(cancellationToken).ConfigureAwait(false);

        _initializationTcs.SetResult(null);
    }

    public Task EnsureInitializedAsync() => _initializationTcs.Task;
}
