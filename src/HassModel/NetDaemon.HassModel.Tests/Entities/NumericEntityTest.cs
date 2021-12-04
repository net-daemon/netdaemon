using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text.Json;
using FluentAssertions;
using Moq;
using NetDaemon.HassModel.Common;
using NetDaemon.HassModel.Entities;
using NetDaemon.HassModel.Tests.TestHelpers;
using Xunit;

namespace NetDaemon.HassModel.Tests.Entities
{
    public class NumericEntityTest
    {

        public void WithTestEntity()
        {
            TestEntity testEntity = new TestEntity(Mock.Of<IHaContext>(), "sensor.dummy");
            var numericEntity = testEntity.AsNumeric();
            numericEntity.WithAttributesAs<TestSensorAttributes>().AsNumeric();
            
            double? state = numericEntity.State;
            
            numericEntity.StateAllChanges().Where(s => s.New?.State == 0.0);
            numericEntity.StateAllChanges().Where(s => s.New?.Attributes?.Name == "LivingRoom");
        }
        
        [Fact]
        public void AsNumeric_Than_WithAttributesAs()
        {
            var entityId = "sensor.temperature";

            var haContextMock = new HaContextMock();
            haContextMock.Setup(m => m.GetState(entityId)).Returns(
                new EntityState { EntityId = entityId, State = "12.3",
                    AttributesJson =  JsonSerializer.Deserialize<JsonElement>(@"{""setPoint"": 21.5, ""units"": ""Celcius""}")
                        });
            
            var entity = new Entity(haContextMock.Object, entityId);
            entity.State.Should().Be("12.3");
            
            // Act: AsNumeric
            var numericEntity = entity.AsNumeric();
            
            // Assert
            numericEntity.State!.Value!.Should().Be(12.3d);
            numericEntity.EntityState!.State!.Value!.Should().Be(12.3d);
            numericEntity.StateAllChanges().Where(e => e.New?.State > 1.2);
            // Act: WithAttributesAs 
            var withAttributes = numericEntity.WithAttributesAs<TestSensorAttributes>();
            
            // Assert
            withAttributes.State!.Value!.Should().Be(12.3d);
            withAttributes.EntityState.State!.Value!.Should().Be(12.3d);

            withAttributes.Attributes!.units.Should().Be("Celcius");
            withAttributes.Attributes!.setPoint.Should().Be(21.5);
            withAttributes.EntityState!.Attributes!.units.Should().Be("Celcius");
            withAttributes.EntityState!.Attributes!.setPoint.Should().Be(21.5);
        }

        [Fact]
        public void WithAttributesAs_Than_AsNumeric()
        {
            var entityId = "sensor.temperature";

            var haContextMock = new Mock<IHaContext>();
            haContextMock.Setup(m => m.GetState(entityId)).Returns(
                new EntityState { EntityId = entityId, State = "12.3",
                    AttributesJson = JsonSerializer.Deserialize<JsonElement>(@"{""setPoint"": 21.5, ""units"": ""Celcius""}")
                });
            
            var entity = new Entity(haContextMock.Object, entityId);
            
            // Act: WithAttributesAs
            Entity<TestSensorAttributes> withAttributes = entity.WithAttributesAs<TestSensorAttributes>();
            NumericEntity<TestSensorAttributes> numericEntity = withAttributes.AsNumeric();

            // Assert
            withAttributes.State.Should().Be("12.3", because: "State  is still a string");

            withAttributes.Attributes!.units.Should().Be("Celcius");
            withAttributes.Attributes!.setPoint.Should().Be(21.5);
            withAttributes.EntityState!.Attributes!.units.Should().Be("Celcius");
            withAttributes.EntityState!.Attributes!.setPoint.Should().Be(21.5);

            // Act: AsNumeric() 
            var numericWithAttributes = withAttributes.AsNumeric();
            
            numericWithAttributes.State!.Value!.Should().Be(12.3d);
            numericWithAttributes.EntityState.State!.Value!.Should().Be(12.3d);
            
            numericWithAttributes.Attributes!.units.Should().Be("Celcius");
            numericWithAttributes.Attributes!.setPoint.Should().Be(21.5);
            numericWithAttributes.EntityState!.Attributes!.units.Should().Be("Celcius");
            numericWithAttributes.EntityState!.Attributes!.setPoint.Should().Be(21.5);
        }

        record TestSensorAttributes(double setPoint, string units);
        
        [Fact]
        public void ShouldShowStateChangesFromContext()
        {
            var haContextMock = new HaContextMock();
            
            var target = new NumericEntity(haContextMock.Object, "domain.testEntity");
            var stateChangeObserverMock = new Mock<IObserver<NumericStateChange>>();
            var stateAllChangeObserverMock = new Mock<IObserver<NumericStateChange>>();

            target.StateAllChanges().Subscribe(stateAllChangeObserverMock.Object);
            target.StateChanges().Subscribe(stateChangeObserverMock.Object);

            haContextMock.StateAllChangeSubject.OnNext(new StateChange(target,
                                                                       old:  new EntityState { State = "1" },
                                                                       @new: new EntityState { State = "1"}));

            haContextMock.StateAllChangeSubject.OnNext(new StateChange(target, 
                                                                       old:  new EntityState { State = "1" },
                                                                       @new: new EntityState { State = "2"}));

            stateChangeObserverMock.Verify(o => o.OnNext(It.Is<NumericStateChange>(e => e.New!.State!.Equals(1.0)) ), Times.Never);
            stateChangeObserverMock.Verify(o => o.OnNext(It.Is<NumericStateChange>(e => e.New!.State!.Equals(2.0)) ), Times.Once);
            
            stateAllChangeObserverMock.Verify(o => o.OnNext(It.Is<NumericStateChange>(e => e.Old!.State!.Equals(1.0)) ), Times.Exactly(2));
            stateAllChangeObserverMock.Verify(o => o.OnNext(It.IsAny<NumericStateChange>() ), Times.Exactly(2));
        }
    }
}