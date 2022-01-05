using System.Reactive.Subjects;

namespace NetDaemon.Runtime.Tests.Helpers;

public static class ObservableExtensions
{
    public static async Task WaitForObservers<T>(this Subject<T> subject)
    {
        var cancelSource = new CancellationTokenSource(5000);
        await Task.Run(
            async () =>
            {
                while (!subject.HasObservers) { await Task.Delay(10); }
            }
            , cancelSource.Token
        );
    }
}