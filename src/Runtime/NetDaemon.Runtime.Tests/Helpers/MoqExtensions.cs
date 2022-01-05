using System.Linq.Expressions;

namespace NetDaemon.Runtime.Tests.Helpers;

internal static class MoqExtensions
{

    public static async Task WaitForInvocation<T, TResult>(this Mock<T> mock, Expression<Func<T, TResult>> expression) where T : class
    {
        var tcs = new TaskCompletionSource<bool>();
        mock.Setup(expression).Callback(() => tcs.SetResult(true));
        var task = await Task.WhenAny(tcs.Task, Task.Delay(5000));
        if (task != tcs.Task)
            throw new TimeoutException("Wait for invocation timeout!");
    }
}