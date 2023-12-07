using System.Collections.Concurrent;
using NetDaemon.Client.HomeAssistant.Extensions;
using NetDaemon.Client.Internal.HomeAssistant.Commands;
using NetDaemon.Infrastructure.ObservableHelpers;

namespace NetDaemon.HassModel.Internal;

/// <summary>
/// Manages trigger subscriptions
/// Main responsibility is to make sure triggers get unsubscribed when Disposed, so apps will not need to worry
/// about unsubscribing 
/// </summary>
internal class TriggerManager : IAsyncDisposable, ITriggerManager
{
    private readonly IHomeAssistantRunner _runner;
    private readonly IQueuedObservable<HassMessage> _queuedObservable;
    private readonly IBackgroundTaskTracker _tracker;

    private readonly ConcurrentBag<(int id, IDisposable disposable)> _subscriptions = new();
    private bool _disposed;

    public TriggerManager(IHomeAssistantRunner runner, IBackgroundTaskTracker tracker, ILogger<IHaContext> logger)
    {
        _runner = runner;
        _tracker = tracker;
            
        var hassMessages = (IHomeAssistantHassMessages)runner.CurrentConnection!;
        _queuedObservable = new QueuedObservable<HassMessage>(logger);
        _queuedObservable.Initialize(hassMessages.OnHassMessage);
    }
    
    public IObservable<JsonElement> RegisterTrigger(object triggerParams)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // We create a new subject that we can return directly. In the background we register the trigger in HA and 
        // forward messages to this Subject
        var subject = new Subject<JsonElement>();

        _tracker.TrackBackgroundTask(SubscribeToTrigger(triggerParams, subject));

        return subject;
    }

    private async Task SubscribeToTrigger(object triggerParams, Subject<JsonElement> subject)
    {
        var message = await _runner.CurrentConnection!.SubscribeToTriggerAsync(triggerParams, CancellationToken.None).ConfigureAwait(false);
        var id = message.Id;

        var subscribtion = _queuedObservable
            .Where(m => m.Id == id)
            .Select(n => n.Event?.Variables?.TriggerElement)
            .Where(m => m.HasValue)
            .Subscribe(m => subject.OnNext(m!.Value));
        
        _subscriptions.Add((id, subscribtion));
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        // Unsubscribe from all triggers in HA (ignore if not connected anymore, we will not get new events anyway)
        var tasks = _subscriptions.Select(s => _runner.CurrentConnection?.UnsubscribeEventsAsync(s.id, CancellationToken.None) ?? Task.CompletedTask).ToArray();
        
        // Also unsubscribe any Observers to avoid memory leaks
        foreach (var subscription in _subscriptions)
        {
            subscription.disposable.Dispose();
        }
        
        await Task.WhenAll(tasks).ConfigureAwait(false);
        await _queuedObservable.DisposeAsync().ConfigureAwait(false);
    }
}