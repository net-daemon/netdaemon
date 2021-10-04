using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using System.Text.Json.Serialization;
using Moq;
using NetDaemon.Model3.Common;
using NetDaemon.Model3.Entities;
using FluentAssertions;
using Xunit;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace NetDaemon.Model3.Tests.Entities
{
    public class EntityTest
    {
        record TestEntity : Entity<TestEntity, EntityState<string, TestEntityAttributes>, string, TestEntityAttributes>
        {
            public TestEntity(IHaContext haContext, string entityId) : base(haContext, entityId) { }
        }

        record TestEntityAttributes
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }
        }

        [Fact]
        public void ShouldWrapStateFromHaContext()
        {
            var haContextMock = new Mock<IHaContext>();

            var entityState =
                new EntityState()
                {
                    State = "CurrentState",
                    AttributesJson = JsonSerializer.Deserialize<JsonElement>("{\"name\": \"value\"}")
                };
            
            haContextMock.Setup(t => t.GetState("domain.testEntity")).Returns(entityState);
            
            var target = new TestEntity(haContextMock.Object, "domain.testEntity");

            target.State.Should().Be("CurrentState");
            target.Attributes!.Name.Should().Be("value");
            
            target.EntityState!.State.Should().Be("CurrentState");
            target.EntityState!.Attributes!.Name.Should().Be("value");
        }

        [Fact]
        public void ShouldShowStateChangesFromContext()
        {
            var stateChangesSubject = new Subject<StateChange>();
            var haContextMock = new Mock<IHaContext>();
            haContextMock.Setup(h => h.StateAllChanges).Returns(stateChangesSubject);
            
            var target = new TestEntity(haContextMock.Object, "domain.testEntity");
            var stateChangeObserverMock = new Mock<IObserver<StateChange>>();
            var stateAllChangeObserverMock = new Mock<IObserver<StateChange>>();

            target.StateAllChanges.Subscribe(stateAllChangeObserverMock.Object);
            target.StateChanges.Subscribe(stateChangeObserverMock.Object);

            stateChangesSubject.OnNext(
                new StateChange(target, new EntityState {State = "old"}, 
                    new EntityState {State = "new"}));

            stateChangesSubject.OnNext(
                new StateChange(target, new EntityState(){State = "same"}, 
                    new EntityState {State = "same"}));

            stateChangeObserverMock.Verify(o => o.OnNext(It.IsAny<StateChange>() ), Times.Once);
            stateAllChangeObserverMock.Verify(o => o.OnNext(It.IsAny<StateChange>() ), Times.Exactly(2));
        }

        [Fact]
        public void ShouldCallServiceOnContext()
        {
            var haContextMock = new Mock<IHaContext>();

            var entity = new TestEntity(haContextMock.Object, "domain.testEntity");
            var data = "payload";
            
            entity.CallService("service", data);
            
            // TODO: should we always use the domain of the entity?
            haContextMock.Verify(h => h.CallService("domain", "service", It.Is<Target>(t => t.EntityIds.Single() == entity.EntityId), data), Times.Once);

        }
    }
}