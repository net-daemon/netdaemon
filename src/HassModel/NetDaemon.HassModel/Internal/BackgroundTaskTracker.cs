﻿using System.Collections.Concurrent;

namespace NetDaemon.HassModel.Internal;

internal class BackgroundTaskTracker(ILogger<BackgroundTaskTracker> logger) : IBackgroundTaskTracker
{
    private readonly ILogger<BackgroundTaskTracker> _logger = logger;
    private volatile bool _isDisposed;

    internal readonly ConcurrentDictionary<Task, object?> BackgroundTasks = new();

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

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5));

        // Using a while look here incase new tasks are added while we are waiting
        while (!BackgroundTasks.IsEmpty)
        {
            var task = await Task.WhenAny( Task.WhenAll(BackgroundTasks.Keys), timeoutTask).ConfigureAwait(false);
            if (task == timeoutTask)
                break;
        }
    }
}
