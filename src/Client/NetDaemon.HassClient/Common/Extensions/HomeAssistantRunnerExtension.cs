namespace NetDaemon.Client.Extensions;

public static class HomeAssistantRunnerExtension
{
    /// <summary>
    /// Observable that emits when a (new) connection is established. If the runner is connected upon subscribing, it will immediately emit the current connection.
    /// </summary>
    public static IObservable<IHomeAssistantConnection> OnConnectWithCurrent(this IHomeAssistantRunner homeAssistantRunner)
    {
        // Generate a one-time observable for CurrentConnection if it’s non-null. Using Defer ensures the observable is created when subscribed to, not beforehand, avoiding potential race conditions.
        var currentConnectionObservable = Observable.Defer(() =>
            homeAssistantRunner.CurrentConnection != null
                ? Observable.Return(homeAssistantRunner.CurrentConnection)
                : Observable.Empty<IHomeAssistantConnection>());

        // Combine CurrentConnection and OnConnect, taking the first valid connection
        return currentConnectionObservable
            .Merge(homeAssistantRunner.OnConnect);
    }
}
