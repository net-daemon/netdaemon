using System.Threading;
using System.Threading.Tasks;
using NetDaemon.HassModel.Common;

namespace NetDaemon.HassModel.Internal.Client;

internal class CacheManager : ICacheManager
{
    private readonly EntityAreaCache _entityAreaCache;
    private readonly EntityStateCache _entityStateCache;

    public CacheManager(EntityAreaCache entityAreaCache, EntityStateCache entityStateCache)
    {
        _entityAreaCache = entityAreaCache;
        _entityStateCache = entityStateCache;
    }

    public async Task Initialize(CancellationToken cancellationToken)
    {
        await _entityAreaCache.InitializeAsync(cancellationToken).ConfigureAwait(false);
        await _entityStateCache.InitializeAsync(cancellationToken).ConfigureAwait(false);
    }
}