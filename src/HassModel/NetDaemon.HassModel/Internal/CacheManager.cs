namespace NetDaemon.HassModel.Internal;

internal class CacheManager(EntityStateCache entityStateCache, RegistryCache registryCache)
    : ICacheManager
{
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await entityStateCache.InitializeAsync(cancellationToken).ConfigureAwait(false);
        await registryCache.InitializeAsync(cancellationToken).ConfigureAwait(false);
    }
}
