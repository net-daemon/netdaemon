using NetDaemon.AppModel.Internal;

namespace NetDaemon.AppModel.Tests.Context;

public class AwaitableHelperTests
{
    [Fact]
    public async Task AwaitIfNeeded_ReturnsNull_WhenNull()
    {
        var result = await AwaitableHelper.AwaitIfNeeded(null);

        result.Should().BeNull();
    }

    [Fact]
    public async Task AwaitIfNeeded_ReturnsInput_WhenNotAwaitable()
    {
        var notAwaitable = new NotAwaitable();

        var result = await AwaitableHelper.AwaitIfNeeded(notAwaitable);

        result.Should().BeSameAs(notAwaitable);
    }

    [Fact]
    public async Task AwaitIfNeeded_ReturnsInput_WhenNotValidAwaitable()
    {
        var notAwaitable = new InvalidAwaitable();

        var result = await AwaitableHelper.AwaitIfNeeded(notAwaitable);

        result.Should().BeSameAs(notAwaitable);
    }

    [Fact]
    public async Task AwaitIfNeeded_ReturnsNull_WhenCompletedTaskNotOfT()
    {
        var awaitable = Task.CompletedTask;

        var result = await AwaitableHelper.AwaitIfNeeded(awaitable);

        result.Should().BeNull();
    }

    [Fact]
    public async Task AwaitIfNeeded_AwaitsAndReturnsResult_WhenTaskOfT()
    {
        var inner = new object();
        var awaitable = Task.FromResult((object?)inner);

        var result = await AwaitableHelper.AwaitIfNeeded(awaitable);

        result.Should().BeSameAs(inner);
    }

    [Fact]
    public async Task AwaitIfNeeded_AwaitsAndReturnsResult_WhenValueTaskOfT()
    {
        var inner = new object();
        var awaitable = new ValueTask<object?>(inner);

        var result = await AwaitableHelper.AwaitIfNeeded(awaitable);

        result.Should().BeSameAs(inner);
    }

    [Fact]
    public async Task AwaitIfNeeded_Awaits_WhenNonCompletedTask()
    {
        var tcs = new TaskCompletionSource();
        var action = async () => { await tcs.Task; };

        var resultTask = AwaitableHelper.AwaitIfNeeded(action());

        resultTask.IsCompleted.Should().BeFalse();
        tcs.SetResult();
        var result = await resultTask;
        resultTask.IsCompleted.Should().BeTrue();

        result.Should().BeNull();
    }

    [Fact]
    public async Task AwaitIfNeeded_AwaitsAndReturnsResult_WhenNonCompletedTaskOfT()
    {
        var tcs = new TaskCompletionSource<int>();
        var action = async () => await tcs.Task;

        var resultTask = AwaitableHelper.AwaitIfNeeded(action());

        resultTask.IsCompleted.Should().BeFalse();
        tcs.SetResult(42);
        var result = await resultTask;
        resultTask.IsCompleted.Should().BeTrue();

        result.Should().Be(42);
    }

    class NotAwaitable
    {
        // No GetAwaiter method
    }

    class InvalidAwaitable
    {
        // GetAwaiter exists but does not return a valid awaiter
        public NotAwaitable GetAwaiter() => new();
    }
}
