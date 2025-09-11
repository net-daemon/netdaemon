using System.Reflection;

namespace NetDaemon.AppModel.Internal;

static class AwaitableHelper
{
    /// <summary>
    /// Checks if the passed possibleAwaitable is awaitable,
    ///     if so it awaits it and returns its result asynchronously,
    ///     if not it will return return a Task that will yield the possibleAwaitable itself
    /// </summary>
    public static async Task<object?> AwaitIfNeeded(object? possibleAwaitable)
    {
        if (possibleAwaitable is null || !IsAwaitable(possibleAwaitable))
            return possibleAwaitable;

        return await new AwaitableWrapper(possibleAwaitable);
    }

    private static bool IsAwaitable(object target)
    {
        // an object is awaitable if it has a public instance method GetAwaiter with no parameters
        // and the return type of GetAwaiter has
        // - a public instance property IsCompleted of type bool
        // - a public instance method OnCompleted with one parameter of type Action
        // - a public instance method GetResult with no parameters

        var getAwaiterMethodInfo = target.GetType().GetMethod("GetAwaiter", BindingFlags.Instance | BindingFlags.Public);

        if (getAwaiterMethodInfo is null || getAwaiterMethodInfo.GetParameters().Length != 0)
            return false;

        var awaiterType = getAwaiterMethodInfo.ReturnType;

        var isCompletedInfo = awaiterType.GetProperty("IsCompleted", BindingFlags.Instance | BindingFlags.Public);
        var validIsCompleted = isCompletedInfo?.PropertyType == typeof(bool);

        var onCompletedInfo = awaiterType.GetMethod("OnCompleted", BindingFlags.Instance | BindingFlags.Public);
        var validOnCompleted = onCompletedInfo?.GetParameters().Length == 1 && onCompletedInfo.GetParameters()[0].ParameterType == typeof(Action);

        var getResultInfo = awaiterType.GetMethod("GetResult", BindingFlags.Instance | BindingFlags.Public);

        var validGetResult = getResultInfo?.GetParameters().Length == 0;

        return validIsCompleted && validOnCompleted && validGetResult;
    }

    /// <summary>
    /// Wraps any awaiter in a type that is known to the compiler to be awaitable.
    /// </summary>
    private class AwaitableWrapper(object inner)
    {
        public AwaiterWrapper GetAwaiter() => new AwaiterWrapper(((dynamic)inner).GetAwaiter());
    }

    private class AwaiterWrapper(object inner) : INotifyCompletion
    {
        public bool IsCompleted => ((dynamic)inner).IsCompleted;

        public void OnCompleted(Action continuation) => ((dynamic)inner).OnCompleted(continuation);

        public object? GetResult()
        {
            // Can't use dynamic here because GetResult might return void, in which case we want to return null here
            return inner.GetType().GetMethod("GetResult", BindingFlags.Instance | BindingFlags.Public)?.Invoke(inner, []);
        }
    }
}
