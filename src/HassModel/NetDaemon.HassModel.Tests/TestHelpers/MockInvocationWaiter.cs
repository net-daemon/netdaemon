using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Moq;

namespace NetDaemon.HassModel.Tests.TestHelpers;

internal class MockInvocationWaiter : IAsyncDisposable
{
    private readonly Task _runningTask;

    public MockInvocationWaiter(Task task)
    {
        _runningTask = task;
    }

    public async ValueTask DisposeAsync()
    {
        var delayTask = Task.Delay(5000);
        var waitTask = await Task.WhenAny(_runningTask, delayTask);
        if (waitTask == delayTask)
            throw new TimeoutException("Wait for invocation timeout!");
    }

    public static MockInvocationWaiter Wait<T, TResult>(Mock<T> mock, Expression<Func<T, TResult>> expression)
        where T : class
    {
        var tcs = new TaskCompletionSource<bool>();
        mock.Setup(expression).Callback(() => tcs.SetResult(true));
        return new MockInvocationWaiter(tcs.Task);
    }

    public static MockInvocationWaiter Wait<T>(Mock<T> mock, Expression<Action<T>> expression) where T : class
    {
        var tcs = new TaskCompletionSource<bool>();
        mock.Setup(expression).Callback(() => tcs.SetResult(true));
        return new MockInvocationWaiter(tcs.Task);
    }

    public static MockInvocationWaiter WaitAll<T>(params Tuple<Mock<T>, Expression<Action<T>>>[] expressions)
        where T : class
    {
        var taskList = new List<Task>();
        foreach (var (mock, expr) in expressions)
        {
            var tcs = new TaskCompletionSource<bool>();
            mock.Setup(expr).Callback(() => tcs.SetResult(true));
            taskList.Add(tcs.Task);
        }

        return new MockInvocationWaiter(Task.WhenAll(taskList.ToArray()));
    }
}