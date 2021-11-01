using System;
using System.Linq;
using System.Reactive.Subjects;
using FluentAssertions;
using Moq;
using NetDaemon.HassModel.Common;
using NetDaemon.HassModel.Entities;
using Xunit;

namespace NetDaemon.HassModel.Tests.Entities
{
    public class EnumerableEntityExtensionsTest
    {
        [Fact]
        public void TestStateChanges()
        {
            var observerMock = new Mock<IObserver<StateChange>>();

            Subject<StateChange> stateChangesSubject  = new();
            var haMock = new Mock<IHaContext>();
            haMock.Setup(h => h.StateAllChanges).Returns(stateChangesSubject);
            
            var switch1 = new Entity(haMock.Object, "switch.Living1");
            var switch2 = new Entity(haMock.Object, "switch.Living2");

            // Act: Subscribe to both entities
            using var _ = new[] { switch1, switch2 }.StateChanges().Subscribe(observerMock.Object);
            
            stateChangesSubject.OnNext(new StateChange(switch1, new EntityState {State = "OldState1"}, new EntityState { State = "NewState1"}));
            
            observerMock.Verify(m => m.OnNext(It.Is<StateChange>(s => s.Entity == switch1 && s.New.State == "NewState1")), Times.Once);
            observerMock.VerifyNoOtherCalls();
            
            stateChangesSubject.OnNext(new StateChange(switch2, new EntityState { State = "OldState2"}, new EntityState{ State = "NewState2"}));
            
            observerMock.Verify(m => m.OnNext(It.Is<StateChange>(s => s.Entity == switch2 && s.New.State == "NewState2")), Times.Once);
            observerMock.VerifyNoOtherCalls();
        }
        
        [Fact]
        public void TestTypedStateChanges()
        {
            var observerMock = new Mock<IObserver<StateChange>>();

            Subject<StateChange> stateChangesSubject  = new();
            var haMock = new Mock<IHaContext>();
            haMock.Setup(h => h.StateAllChanges).Returns(stateChangesSubject);
            
            var switch1 = new TestTypedEntity(haMock.Object, "switch.Living1");
            var switch2 = new TestTypedEntity(haMock.Object, "switch.Living2");

            // Act: Subscribe to both entities
            using var _ = new[] { switch1, switch2 }.StateChanges().Subscribe(observerMock.Object);
            
            stateChangesSubject.OnNext(new StateChange(switch1, new EntityState {State = "OldState1"}, new EntityState { State = "NewState1"}));
            
            observerMock.Verify(m => m.OnNext(It.Is<StateChange>(s => s.Entity == switch1 && s.New.State == "NewState1")), Times.Once);
            observerMock.VerifyNoOtherCalls();
            
            stateChangesSubject.OnNext(new StateChange(switch2, new EntityState { State = "OldState2"}, new EntityState{ State = "NewState2"}));
            
            observerMock.Verify(m => m.OnNext(It.Is<StateChange>(s => s.Entity == switch2 && s.New.State == "NewState2")), Times.Once);
            observerMock.VerifyNoOtherCalls();
        }
        
        
        [Fact]
        public void TestCallService()
        {
            Subject<StateChange> stateChangesSubject  = new();
            var haMock = new Mock<IHaContext>();
            haMock.Setup(h => h.StateAllChanges).Returns(stateChangesSubject);
            
            var switch1 = new Entity(haMock.Object, "switch.Living1");
            var switch2 = new Entity(haMock.Object, "switch.Living2");

            // Act: Subscribe to both entities
            var data = new { Name = "John", Age = 12 };
            new[] { switch1, switch2 }.CallService("switch", "set_state", data);
            
            haMock.Verify(m => m.CallService("switch", "set_state", It.IsAny<ServiceTarget>(), data));

            haMock.Invocations.First().Arguments[2].As<ServiceTarget>().EntityIds
                .Should().BeEquivalentTo("switch.Living1", "switch.Living2");
        }        
    }

    record TestTypedAttributes(string Name, string Value);

    record TestTypedEntity : Entity<TestTypedEntity, EntityState<TestTypedAttributes>, TestTypedAttributes>
    {
        public TestTypedEntity(IHaContext haContext, string entityId) : base(haContext, entityId) { }
    }
}