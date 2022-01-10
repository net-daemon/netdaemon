using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NetDaemon.HassModel.Common;
using Xunit;

namespace NetDaemon.HassModel.Tests.Common;

public class ObservableExtensionsTest
{

    [Fact]
    public void TestSubscribeAsyncShouldCallAsyncFunction()
    {
        var testSubject = new Subject<string>();
        var observerMock = new Mock<IObserver<string>>();
        var str = string.Empty;
        
        testSubject.SubscribeAsync((a) =>
        {
            str = a;
            return Task.CompletedTask;
        });
        
        testSubject.OnNext("hello");
        str.Should().Be("hello");
    }    
    
    [Fact]
    public void TestSubscribeAsyncConcurrentShouldCallAsyncFunction()
    {
        var testSubject = new Subject<string>();
        var observerMock = new Mock<IObserver<string>>();
        var str = string.Empty;
        
        testSubject.SubscribeAsyncConcurrent((a) =>
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
        bool errorCalled = false;
        var subscriber = testSubject.SubscribeAsync((a) => throw new InvalidOperationException(),
            e => errorCalled = true);
        
        testSubject.OnNext("hello");
        errorCalled.Should().BeTrue();
    }    
    
    [Fact]
    public void TestSubscribeAsyncConcurrentExceptionShouldCallErrorCallback()
    {
        var testSubject = new Subject<string>();
        bool errorCalled = false;
        var subscriber = testSubject.SubscribeAsyncConcurrent((a) => throw new InvalidOperationException(),
            e => errorCalled = true);
        
        testSubject.OnNext("hello");
        errorCalled.Should().BeTrue();
    }
}
