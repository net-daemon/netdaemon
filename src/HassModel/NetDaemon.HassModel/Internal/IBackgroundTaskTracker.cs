namespace NetDaemon.HassModel.Internal;

internal interface IBackgroundTaskTracker : IAsyncDisposable
{
    /// <summary>
    ///    Tracks a background task and logs exceptions
    /// </summary>
    public void TrackBackgroundTask(Task? task, string? description = null);
}
