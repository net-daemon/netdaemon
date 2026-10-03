namespace NetDaemon.Tests.Integration.Helpers;

internal static class WaitForConditionHelper
{
    public static async Task WaitUntilAsync(
        Func<CancellationToken, Task<bool>> condition,
        TimeSpan timeout,
        TimeSpan pollInterval,
        string timeoutMessage)
    {
        using var timeoutSource = new CancellationTokenSource(timeout);

        while (true)
        {
            try
            {
                if (await condition(timeoutSource.Token).ConfigureAwait(false))
                {
                    return;
                }

                await Task.Delay(pollInterval, timeoutSource.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (timeoutSource.IsCancellationRequested)
            {
                throw new TimeoutException(timeoutMessage);
            }
        }
    }
}
