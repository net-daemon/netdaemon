using System.Reflection;

namespace NetDaemon.AppModel.Internal;

static class AwaitableHelper
{
    /// <summary>
    /// Returns a Task that will await the inner object if it is awaitable, or return the inner object directly if not.
    /// </summary>
    public static async Task<object?> AwaitIfNeeded(object? inner)
    {
        var getAwaiterMethodInfo = inner?.GetType().GetMethod("GetAwaiter", BindingFlags.Instance | BindingFlags.Public);

        if (getAwaiterMethodInfo is null || !IsValidGetAwaiterMethod(getAwaiterMethodInfo))
        {
            // Not awaitable, return as is
            return inner;
        }

        // Signature matches, now get the actual awaiter
        var awaiter = getAwaiterMethodInfo.Invoke(inner, []);

        return await new AwaitableWrapper(awaiter!);
    }

    private static bool IsValidGetAwaiterMethod(MethodInfo getAwaiterMethodInfo)
    {
        if (getAwaiterMethodInfo.GetParameters().Length != 0)
            return false;

        var awaiterType = getAwaiterMethodInfo.ReturnType;

        var hasIsCompleted = awaiterType.GetProperty("IsCompleted", BindingFlags.Instance | BindingFlags.Public)?.PropertyType == typeof(bool);
        if (!hasIsCompleted) return false;

        var onCompletedInfo = awaiterType.GetMethod("OnCompleted", BindingFlags.Instance | BindingFlags.Public);
        var validOnCompleted = onCompletedInfo?.GetParameters().Length == 1 && onCompletedInfo.GetParameters()[0].ParameterType == typeof(Action);
        if (!validOnCompleted) return false;

        var getResultInfo = awaiterType.GetMethod("GetResult", BindingFlags.Instance | BindingFlags.Public);
        var validGetResult = getResultInfo?.GetParameters().Length == 0;

        return validGetResult;
    }

    /// <summary>
    /// Wraps any awaiter in a type that is known to the compiler to be awaitable.
    /// </summary>
    private class AwaitableWrapper (object inner) : INotifyCompletion
    {
        public AwaitableWrapper GetAwaiter() => this;

        public bool IsCompleted => ((dynamic)inner).IsCompleted;

        public void OnCompleted(Action continuation) => ((dynamic)inner).OnCompleted(continuation);

        public object? GetResult() => ((dynamic)inner).GetResult();
    }
}
