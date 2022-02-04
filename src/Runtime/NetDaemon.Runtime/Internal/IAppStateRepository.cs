namespace NetDaemon.Runtime.Internal;

internal interface IAppStateRepository
{
    Task<bool> GetOrCreateAsync(string applicationId, CancellationToken token);
    Task UpdateAsync(string applicationId, bool enabled, CancellationToken token);
    Task RemoveNotUsedStatesAsync(IReadOnlyCollection<string> applicationIds, CancellationToken token);
}