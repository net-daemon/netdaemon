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
            catch (OperationCanceledException)
            {
                _logger.LogTrace("Task was canceled processing Home Assistant event: {Description}", description ?? "");
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

    public async ValueTask Flush()
    {
        var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5));

        while (!BackgroundTasks.IsEmpty)
        {
            var task = await Task.WhenAny( Task.WhenAll(BackgroundTasks.Keys), Task.Delay(TimeSpan.FromSeconds(5))).ConfigureAwait(false);
            if (task == timeoutTask)
                break;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await Flush();
    }
}
