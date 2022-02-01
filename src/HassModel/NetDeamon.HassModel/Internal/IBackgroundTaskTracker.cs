namespace NetDaemon.HassModel.Internal;

internal interface IBackgroundTaskTracker : IAsyncDisposable
{
    public void TrackBackgroundTask(Task? task, string? description = null);
}
