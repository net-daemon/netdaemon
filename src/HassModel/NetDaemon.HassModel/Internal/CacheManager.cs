namespace NetDaemon.HassModel.Internal;

internal class CacheManager(EntityStateCache entityStateCache, RegistryCache registryCache)
    : ICacheManager
{
    public async Task InitializeAsync(IHomeAssistantConnection homeAssistantConnection, CancellationToken cancellationToken)
    {
        await entityStateCache.InitializeAsync(homeAssistantConnection, cancellationToken).ConfigureAwait(false);
        await registryCache.InitializeAsync(homeAssistantConnection, cancellationToken).ConfigureAwait(false);
    }
}
