namespace NetDaemon.HassModel.Internal;

internal class CacheManager : ICacheManager
{
    private readonly EntityAreaCache _entityAreaCache;
    private readonly EntityStateCache _entityStateCache;

    public CacheManager(EntityAreaCache entityAreaCache, EntityStateCache entityStateCache)
    {
        _entityAreaCache = entityAreaCache;
        _entityStateCache = entityStateCache;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await _entityAreaCache.InitializeAsync(cancellationToken).ConfigureAwait(false);
        await _entityStateCache.InitializeAsync(cancellationToken).ConfigureAwait(false);
    }
}