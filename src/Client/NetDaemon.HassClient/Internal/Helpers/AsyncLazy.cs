namespace NetDaemon.Client.Internal.Helpers;

public class AsyncLazy<T>(Func<Task<T>> taskFactory) : Lazy<Task<T>>(()
    => Task.Factory.StartNew(taskFactory).Unwrap());
