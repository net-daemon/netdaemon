using System.Collections.Concurrent;
using System.Threading;
using NetDaemon.Client.Common.HomeAssistant.Model;

namespace NetDaemon.HassModel.Internal;

/// <summary>
/// Manages trigger subscriptions
/// Main responsibility is to make sure triggers get unsubscribed when Disposed, so apps will not need to worry
/// about unsubscribing 
/// </summary>
internal class TriggerManager : IAsyncDisposable, ITriggerManager
{
    private readonly IHomeAssistantRunner _runner;
    private readonly IHomeAssistantHassMessages _hassMessages;
    private readonly ConcurrentBag<int> _triggerIds = new();
    private readonly ConcurrentBag<IDisposable> _subscriptions = new();
    private bool _disposed;

    public TriggerManager(IHomeAssistantRunner runner)
    {
        _runner = runner;
        _hassMessages = (IHomeAssistantHassMessages)runner.CurrentConnection!;
    }
    
    public async Task<IObservable<JsonElement>> RegisterTrigger<T>(T triggerParams) where T : TriggerBase
    {
        var id = await _runner.CurrentConnection!.SubscribeToTriggerAsync(triggerParams, CancellationToken.None).ConfigureAwait(false);
        _triggerIds.Add(id);
        
        // We create a new subject and forward all messages for this trigger subscription to that.
        // This makes it possible to unsubscribe when Disposing this manager and avoid
        // memory leaks when apps are stopped and started repeatedly  
        var sub = new Subject<JsonElement>();
        
        _subscriptions.Add(_hassMessages.OnHassMessage
            .Where(m => m.Id == id)
            .Select(n => n.Event?.Variables?.TriggerElement)
            .Where(m => m.HasValue).Subscribe(m => sub.OnNext(m!.Value)));
        return sub;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        // Unsubscribe to this trigger in HA (ignore if not connected anymore, we will not ge new events anyway)
        var tasks = _triggerIds.Select(id => _runner.CurrentConnection?.UnsubscribeFromTriggerAsync(id, CancellationToken.None) ?? Task.CompletedTask).ToArray();
        
        // Also unsubscribe any Observers we dont get memory leaks
        foreach (var subscription in _subscriptions)
        {
            subscription.Dispose();
        }
        
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }
}