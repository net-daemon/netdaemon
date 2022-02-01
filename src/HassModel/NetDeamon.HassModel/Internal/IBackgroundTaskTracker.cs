namespace NetDaemon.HassModel.Internal;

internal interface IBackgroundTaskTracker
{
    public void TrackBackgroundTask(Task? task, string? description = null);
}