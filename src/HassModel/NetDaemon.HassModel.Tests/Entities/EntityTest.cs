using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Moq;
using NetDaemon.HassModel.Common;
using NetDaemon.HassModel.Entities;
using FluentAssertions;
using JoySoftware.HomeAssistant.Model;
using NetDaemon.HassModel.Tests.TestHelpers;
using NetDaemon.HassModel.Tests.TestHelpers.HassClient;
using Xunit;

namespace NetDaemon.HassModel.Tests.Entities
{
    public class EntityTest
    {
        [Fact]
        public void ShouldWrapStateFromHaContext()
        {
            // Arrange
            var haContextMock = new Mock<IHaContext>();

            var entityState =
                new EntityState()
                {
                    State = "CurrentState",
                    AttributesJson = new { name = "FirstName" }.AsJsonElement()
                };

            haContextMock.Setup(t => t.GetState("domain.testEntity")).Returns(entityState);

            // Act
            var target = new TestEntity(haContextMock.Object, "domain.testEntity");

            // Assert
            target.State.Should().Be("CurrentState");
            target.Attributes!.Name.Should().Be("FirstName");

            target.EntityState!.State.Should().Be("CurrentState");
            target.EntityState!.Attributes!.Name.Should().Be("FirstName");

            // Act2: update the state
            var newEntityState =
                new EntityState()
                {
                    State = "NewState",
                    AttributesJson = new { name = "SecondName" }.AsJsonElement()
                };

            haContextMock.Setup(t => t.GetState("domain.testEntity")).Returns(newEntityState);

            // Assert
            target.State.Should().Be("NewState");
            target.Attributes!.Name.Should().Be("SecondName");

            target.EntityState!.State.Should().Be("NewState");
            target.EntityState!.Attributes!.Name.Should().Be("SecondName");
        }

        [Fact]
        public void ShouldShowStateChangesFromContext()
        {
            var stateChangesSubject = new Subject<StateChange>();
            var haContextMock = new Mock<IHaContext>();
            haContextMock.Setup(h => h.StateAllChanges()).Returns(stateChangesSubject);

            var target = new TestEntity(haContextMock.Object, "domain.testEntity");
            var stateChangeObserverMock = new Mock<IObserver<StateChange>>();
            var stateAllChangeObserverMock = new Mock<IObserver<StateChange>>();

            target.StateAllChanges().Subscribe(stateAllChangeObserverMock.Object);
            target.StateChanges().Subscribe(stateChangeObserverMock.Object);

            stateChangesSubject.OnNext(
                new StateChange(target, new EntityState { State = "old" },
                    new EntityState { State = "new" }));

            stateChangesSubject.OnNext(
                new StateChange(target, new EntityState() { State = "same" },
                    new EntityState { State = "same" }));

            stateChangeObserverMock.Verify(o => o.OnNext(It.IsAny<StateChange>()), Times.Once);
            stateAllChangeObserverMock.Verify(o => o.OnNext(It.IsAny<StateChange>()), Times.Exactly(2));
        }

        [Fact]
        public void ShouldCallServiceOnContext()
        {
            var haContextMock = new Mock<IHaContext>();

            var entity = new TestEntity(haContextMock.Object, "domain.testEntity");
            var data = "payload";

            entity.CallService("service", data);

            haContextMock.Verify(h => h.CallService("domain", "service", It.Is<ServiceTarget>(t => t.EntityIds!.Single() == entity.EntityId), data), Times.Once);
        }

        [Fact]
        public void ShouldWrapAreaFromContext()
        {
            // Arrange
            var haContextMock = new Mock<IHaContext>();
            haContextMock.Setup(t => t.GetAreaFromEntityId("domain.testEntity")).Returns(new Area() { Name = "Area Name" });

            // Act
            var target = new TestEntity(haContextMock.Object, "domain.testEntity");

            // Assert
            Assert.Equal("Area Name", target.Area);
        }
    }
}