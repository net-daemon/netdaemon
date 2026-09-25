using System.Collections.Concurrent;

namespace NetDaemon.Client.Internal.Helpers;

/// <summary>
/// Handles the result of a 'fire and forget` Message tp Home Assistant
/// We mainly make sure that any Errors from HA or technical exceptions will get logged correctly
/// We also track the tasks so we can await for all pending tasks to finish
/// </summary>
internal class ResultMessageHandler(ILogger logger) : IAsyncDisposable
{
    private readonly ConcurrentDictionary<Task, object?> _backgroundTasks = new();

    public void HandleResult(Task<HassMessage> returnMessageTask, CommandMessage originalCommand)
    {
        var withErrorHandling = HandleResultError(returnMessageTask, originalCommand);

        // Save the task in the dictionary and remove it when it is done
        // this allows us to wait for all pending background tasks to finish
        _backgroundTasks.TryAdd(withErrorHandling, null);
        withErrorHandling.ContinueWith(_ => _backgroundTasks.TryRemove(withErrorHandling, out var _));
    }

    private async Task HandleResultError(Task<HassMessage> returnMessageTask, CommandMessage originalCommand)
    {
        try
        {
            var result = await returnMessageTask.ConfigureAwait(false);
            if (!result.Success ?? false)
            {
                logger.LogError("Failed command ({CommandType}) error: {ErrorResult}.  Sent command is {CommandMessage}",
                    originalCommand.Type, result.Error, originalCommand.GetJsonString());
            }
        }
        catch (TimeoutException)
        {
            logger.LogWarning("Command ({CommandType}) did not get response in timely fashion.  Sent command is {CommandMessage}",
                originalCommand.Type, originalCommand.GetJsonString());
        }
        catch (OperationCanceledException)
        {
            // Expected during connection shutdown or caller cancellation.
        }
        catch (Exception e)
        {
            logger.LogError(e, "Exception waiting for result message.  Sent command is {CommandMessage}", originalCommand.GetJsonString());
        }
    }

    public async Task WaitPendingBackgroundTasksAsync()
    {
        if (!_backgroundTasks.IsEmpty)
        {
            await Task.WhenAll(_backgroundTasks.Keys).ConfigureAwait(false);
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await WaitPendingBackgroundTasksAsync().WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        }
        catch (TimeoutException e)
        {
            logger.LogError(e, "One or requests are still pending while closing connection to Home Assistant");
        }
    }
}
