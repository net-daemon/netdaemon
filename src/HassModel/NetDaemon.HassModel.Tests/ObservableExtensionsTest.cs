using System.Reactive.Subjects;

namespace NetDaemon.HassModel.Tests;

public class ObservableExtensionsTest
{
    [Fact]
    public void TestSubscribeAsyncShouldCallAsyncFunction()
    {
        var testSubject = new Subject<string>();
        var str = string.Empty;

        testSubject.SubscribeAsync(a =>
        {
            str = a;
            return Task.CompletedTask;
        });

        testSubject.OnNext("hello");
        str.Should().Be("hello");
    }

    [Fact]
    public void TestSubscribeSafeShouldCallFunction()
    {
        var testSubject = new Subject<string>();
        var str = string.Empty;

        testSubject.SubscribeSafe(a => { str = a; });

        testSubject.OnNext("hello");
        str.Should().Be("hello");
    }

    [Fact]
    public void TestSubscribeAsyncConcurrentShouldCallAsyncFunction()
    {
        var testSubject = new Subject<string>();
        var observerMock = new Mock<IObserver<string>>();
        var str = string.Empty;

        testSubject.SubscribeAsyncConcurrent(a =>
        {
            str = a;
            return Task.CompletedTask;
        });

        testSubject.OnNext("hello");
        str.Should().Be("hello");
    }

    [Fact]
    public void TestSubscribeAsyncExceptionShouldCallErrorCallback()
    {
        var testSubject = new Subject<string>();
        var errorCalled = false;
        var subscriber = testSubject.SubscribeAsync(a => throw new InvalidOperationException(),
            e => errorCalled = true);

        testSubject.OnNext("hello");
        errorCalled.Should().BeTrue();
    }

    [Fact]
    public void TestSubscribeAsyncExceptionShouldNotStopAfterException()
    {
        var testSubject = new Subject<string>();
        var nrCalled = 0;
        var subscriber = testSubject.SubscribeAsync(a => throw new InvalidOperationException(),
            e => nrCalled++);

        testSubject.OnNext("hello");
        testSubject.OnNext("hello");

        nrCalled.Should().Be(2);
    }

    [Fact]
    public void TestSubscribeSafeExceptionShouldCallErrorCallback()
    {
        var testSubject = new Subject<string>();
        var errorCalled = false;
        var subscriber = testSubject.SubscribeSafe(a => throw new InvalidOperationException(),
            e => errorCalled = true);

        testSubject.OnNext("hello");
        errorCalled.Should().BeTrue();
    }

    [Fact]
    public void TestSubscribeSafeMultipleExceptionShouldNotStopAfterException()
    {
        var testSubject = new Subject<string>();
        var nrCalled = 0;
        var subscriber = testSubject.SubscribeSafe(a => throw new InvalidOperationException(),
            e => nrCalled++);

        testSubject.OnNext("hello");
        testSubject.OnNext("hello");

        nrCalled.Should().Be(2);
    }

    [Fact]
    public void TestSubscribeAsyncConcurrentExceptionShouldCallErrorCallback()
    {
        var testSubject = new Subject<string>();
        var errorCalled = false;
        var subscriber = testSubject.SubscribeAsyncConcurrent(a => throw new InvalidOperationException(),
            e => errorCalled = true);

        testSubject.OnNext("hello");
        errorCalled.Should().BeTrue();
    }
}
