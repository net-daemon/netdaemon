namespace NetDaemon.Client.Internal.Extensions;

internal static class CancellationTokenExtensions
{
    /// <summary>
    ///     Allows using a Cancellation Token as if it were a task.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that can be canceled, but never completed.</returns>
    public static Task AsTask(this CancellationToken cancellationToken)
    {
        return AsTask<object>(cancellationToken);
    }

    /// <summary>Allows using a Cancellation Token as if it were a task.</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that can be canceled, but never completed.</returns>
    private static Task<T> AsTask<T>(this CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<T>();
        cancellationToken.Register(() => tcs.TrySetCanceled(), false);
        return tcs.Task;
    }
}
