using System;
using System.Linq;
using System.Reactive.Subjects;
using FluentAssertions;
using JoySoftware.HomeAssistant.Client;
using JoySoftware.HomeAssistant.Model;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NetDaemon.Model3.Common;
using NetDaemon.Model3.Entities;
using Xunit;

namespace NetDaemon.Model3.Tests.Internal
{
    public class AppScopedHaContextProviderTest
    {
        private Subject<HassEvent> _hassEventSubjectMock = new ();
        private readonly Mock<IHassClient> _hassClientMock = new Mock<IHassClient>();

        [Fact]
        public void TestCallService()
        {
            var haContext = CreateTarget();

            var target = new Target("domain.entity");
            var data = new { Name = "value" };
            haContext.CallService("domain", "service", target, data);
            
            _hassClientMock.Verify(c => c.CallService("domain", "service", data, It.Is<HassTarget>(t => t.EntityIds.Single() == "domain.entity"), false), Times.Once);
        }

        [Fact]
        public void TestStateChange()
        {
            var haContext = CreateTarget();
            var stateAllChangesObserverMock = new Mock<IObserver<StateChange>>();
            var stateChangesObserverMock = new Mock<IObserver<StateChange>>();

            haContext.StateAllChanges.Subscribe(stateAllChangesObserverMock.Object);
            haContext.StateChanges.Subscribe(stateChangesObserverMock.Object);
            
            _hassEventSubjectMock.OnNext(new HassEvent()
            {
                EventType = "state_change",
                Data = new HassStateChangedEventData()
                {
                   EntityId = "TestDomain.TestEntity",
                   NewState = new HassState(){State = "newstate"},
                   OldState = new HassState(){State = "oldstate"}
                }
            });

            stateAllChangesObserverMock.Verify(o => o.OnNext(It.Is<StateChange>(s => s.Entity.EntityId == "TestDomain.TestEntity")), Times.Once);
            stateChangesObserverMock.Verify(o => o.OnNext(It.Is<StateChange>(s => s.Entity.EntityId == "TestDomain.TestEntity")), Times.Once());
            
            haContext.GetState("TestDomain.TestEntity").State!.Should().Be("newstate");
            // the state should come from the state cache so we do not expect a call to HassClient.GetState 
            _hassClientMock.VerifyNoOtherCalls();
        }

        
        private IHaContext CreateTarget()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton(_hassClientMock.Object);
            serviceCollection.AddSingleton<IObservable<HassEvent>>(_hassEventSubjectMock);
            serviceCollection.AddScopedHaContext();
            var provider = serviceCollection.BuildServiceProvider();

            var haContext = provider.GetRequiredService<IHaContext>();
            return haContext;
        }
    }
}