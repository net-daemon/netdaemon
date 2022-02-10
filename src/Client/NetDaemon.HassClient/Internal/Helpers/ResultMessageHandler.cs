using System.Collections.Concurrent;

namespace NetDaemon.Client.Internal.Helpers;

internal class ResultMessageHandler : IResultMessageHandler, IAsyncDisposable
{
    internal int WaitForResultTimeout = 20000;
    private readonly CancellationTokenSource _tokenSource = new();
    private readonly ConcurrentDictionary<Task<HassMessage>, object?> _backgroundTasks = new();
    private readonly ILogger<ResultMessageHandler> _logger;

    public ResultMessageHandler(ILogger<ResultMessageHandler> logger)
    {
        _logger = logger;
    }

    public void HandleResult(Task<HassMessage> returnMessageTask, CommandMessage originalCommand)
    {
        TrackBackgroundTask(returnMessageTask, originalCommand);
    }

    private void TrackBackgroundTask(Task<HassMessage> task, CommandMessage command)
    {
        _backgroundTasks.TryAdd(task, null);

        [SuppressMessage("", "CA1031")]
        async Task Wrap()
        {
            try
            {
                var awaitedTask = await Task.WhenAny(task, Task.Delay(WaitForResultTimeout, _tokenSource.Token)).ConfigureAwait(false);

                if (awaitedTask != task)
                {
                    // We have a timeout
                    _logger.LogWarning(
                        "Command ({CommandType}) did not get response in timely fashion.  Sent command is {CommandMessage}",
                        command.Type, command);
                }
                // We wait for the task even if there was a timeout so we make sure
                // we catch the original error
                var result = await task.ConfigureAwait(false);
                if (!result.Success ?? false)
                {
                    _logger.LogWarning(
                        "Failed command ({CommandType}) error: {ErrorResult}.  Sent command is {CommandMessage}",
                        command.Type, result.Error, command);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception waiting for result message  Sent command is {CommandMessage}", command);
            }
            finally
            {
                _backgroundTasks.TryRemove(task, out var _);
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
        if (!_backgroundTasks.IsEmpty)
        {
            await Task.WhenAny( Task.WhenAll(_backgroundTasks.Keys), Task.Delay(TimeSpan.FromSeconds(5))).ConfigureAwait(false);
        }
    }
}
