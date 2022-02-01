using System.Collections.Concurrent;

namespace NetDaemon.HassModel.Internal;

internal class BackgroundTaskTracker : IBackgroundTaskTracker
{
    private readonly ILogger<BackgroundTaskTracker> _logger;
    internal readonly ConcurrentDictionary<Task, object?> BackgroundTasks = new();

    public BackgroundTaskTracker(ILogger<BackgroundTaskTracker> logger)
    {
        _logger = logger;
    }

    public void TrackBackgroundTask(Task? task, string? description = null)
    {
        ArgumentNullException.ThrowIfNull(task, nameof(task));

        BackgroundTasks.TryAdd(task, null);

        [SuppressMessage("", "CA1031")]
        async Task Wrap()
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception processing Home Assistant event: {Description}", description ?? "");
            }
            finally
            {
                BackgroundTasks.TryRemove(task, out var _);
            }
        }

        // We do not handle task here cause exceptions
        // are handled in the Wrap local functions and
        // all tasks should be cancelable
        _ = Wrap();
    }

    public async ValueTask DisposeAsync()
    {
        // Wait for the tasks to complete max 5 seconds
        if (!BackgroundTasks.IsEmpty)
        {
            await Task.WhenAny( Task.WhenAll(BackgroundTasks.Keys), Task.Delay(TimeSpan.FromSeconds(5))).ConfigureAwait(false);
        }
    }
}
