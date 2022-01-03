using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading;
using FluentAssertions;
using Moq;
using NetDaemon.HassModel.Entities;
using NetDaemon.HassModel.Tests.TestHelpers;
using NetDaemon.HassModel.Tests.TestHelpers.HassClient;
using Xunit;

namespace NetDaemon.HassModel.Tests.Entities
{
    public class NumericEntityTest
    {
        [Fact]
        public void AsNumeric_Than_WithAttributesAs()
        {
            var entityId = "sensor.temperature";

            var haContextMock = new HaContextMock();
            haContextMock.Setup(m => m.GetState(entityId)).Returns(
                new EntityState
                {
                    EntityId = entityId,
                    State = "12.3",
                    AttributesJson = JsonSerializer.Deserialize<JsonElement>(@"{""setPoint"": 21.5, ""units"": ""Celcius""}")
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
            withAttributes.EntityState!.State!.Value!.Should().Be(12.3d);

            withAttributes.Attributes!.units.Should().Be("Celcius");
            withAttributes.Attributes!.setPoint.Should().Be(21.5);
            withAttributes.EntityState!.Attributes!.units.Should().Be("Celcius");
            withAttributes.EntityState!.Attributes!.setPoint.Should().Be(21.5);
            withAttributes.StateAllChanges().Where(e => e.New?.State > 1.2 && e.Entity != null);

        }

        [Fact]
        public void WithAttributesAs_Than_AsNumeric()
        {
            var entityId = "sensor.temperature";

            var haContextMock = new HaContextMock();
            haContextMock.Setup(m => m.GetState(entityId)).Returns(
                new EntityState
                {
                    EntityId = entityId,
                    State = "12.3",
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
            numericWithAttributes.EntityState!.State!.Value!.Should().Be(12.3d);

            numericWithAttributes.Attributes!.units.Should().Be("Celcius");
            numericWithAttributes.Attributes!.setPoint.Should().Be(21.5);
            numericWithAttributes.EntityState!.Attributes!.units.Should().Be("Celcius");
            numericWithAttributes.EntityState!.Attributes!.setPoint.Should().Be(21.5);

            haContextMock.StateAllChangeSubject.OnNext(new StateChange(entity, new EntityState(), new EntityState()));
            numericWithAttributes.StateAllChanges().Where(e => e.New?.State > 1.2 && e.Entity != null).Subscribe();

        }

        record TestSensorAttributes(double setPoint, string units);

        [Fact]
        public void ShouldShowStateChangesFromContext()
        {
            var haContextMock = new HaContextMock();

            var entity = new Entity(haContextMock.Object, "domain.testEntity");
            var target = new NumericEntity(entity);

            haContextMock.Setup(m => m.GetState(entity.EntityId)).Returns(new EntityState() { State = "3.14" });

            var stateAllChangeObserverMock = target.StateAllChanges().SubscribeMock();
            var stateChangeObserverMock = target.StateChanges().SubscribeMock();

            haContextMock.StateAllChangeSubject.OnNext(new StateChange(entity,
                                                                       old: new EntityState { State = "1" },
                                                                       @new: new EntityState { State = "1" }));

            haContextMock.StateAllChangeSubject.OnNext(new StateChange(entity,
                                                                       old: new EntityState { State = "1" },
                                                                       @new: new EntityState { State = "2" }));

            // Assert
            stateChangeObserverMock.Verify(o => o.OnNext(It.Is<NumericStateChange>
            (e => e.Entity.State.Equals(3.14) &&
                  e.Old!.State.Equals(1.0) &&
                  e.New!.State.Equals(2.0))), Times.Once);
            stateChangeObserverMock.VerifyNoOtherCalls();

            stateAllChangeObserverMock.Verify(o => o.OnNext(It.Is<NumericStateChange>
            (e => e.Entity.State.Equals(3.14) &&
                  e.Old!.State.Equals(1.0) &&
                  e.New!.State.Equals(2.0))), Times.Once);

            stateAllChangeObserverMock.Verify(o => o.OnNext(It.Is<NumericStateChange>
            (e => e.Entity.State.Equals(3.14) &&
                  e.Old!.State.Equals(1.0) &&
                  e.New!.State.Equals(1.0))), Times.Once);
            stateAllChangeObserverMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void GenericNumericEntityShouldShowStateChangesFromContext()
        {
            // Arrange
            var haContextMock = new HaContextMock();

            var entity = new Entity(haContextMock.Object, "domain.testEntity");
            var target = new NumericTestEntity(entity);

            var stateChangeObserverMock = target.StateChanges().SubscribeMock();
            var stateAllChangeObserverMock = target.StateAllChanges().SubscribeMock();

            haContextMock.Setup(m => m.GetState(entity.EntityId)).Returns(new EntityState() { State = "3.14" });

            // Act
            haContextMock.StateAllChangeSubject.OnNext(new StateChange(entity,
                old: new EntityState { State = "1" },
                @new: new EntityState { State = "1" }));

            haContextMock.StateAllChangeSubject.OnNext(new StateChange(entity,
                old: new EntityState { State = "1" },
                @new: new EntityState { State = "2" }));

            // Assert
            stateChangeObserverMock.Verify(o => o.OnNext(It.Is<NumericStateChange<NumericTestEntity, NumericEntityState<TestEntityAttributes>>>
                (e => e.Entity.State.Equals(3.14) &&
                      e.Old!.State.Equals(1.0) &&
                      e.New!.State.Equals(2.0))), Times.Once);
            stateChangeObserverMock.VerifyNoOtherCalls();

            stateAllChangeObserverMock.Verify(o => o.OnNext(It.Is<NumericStateChange<NumericTestEntity, NumericEntityState<TestEntityAttributes>>>
                (e => e.Entity.State.Equals(3.14) &&
                      e.Old!.State.Equals(1.0) &&
                      e.New!.State.Equals(2.0))), Times.Once);

            stateAllChangeObserverMock.Verify(o => o.OnNext(It.Is<NumericStateChange<NumericTestEntity, NumericEntityState<TestEntityAttributes>>>
                (e => e.Entity.State.Equals(3.14) &&
                      e.Old!.State.Equals(1.0) &&
                      e.New!.State.Equals(1.0))), Times.Once);
            stateAllChangeObserverMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void StatePropertyShouldBeCultureUnaware()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("De-de");

            var entityId = "sensor.temperature";

            var haContextMock = new HaContextMock();
            haContextMock.Setup(m => m.GetState(entityId)).Returns(new EntityState { EntityId = entityId, State = "12.5" });

            var entity = new Entity(haContextMock.Object, entityId);

            var numericEntity = entity.AsNumeric();
            numericEntity.State.Should().Be(12.5);
            numericEntity.EntityState!.State.Should().Be(12.5);

            var withAttributesAs = numericEntity.WithAttributesAs<TestEntityAttributes>();
            withAttributesAs.State.Should().Be(12.5);
            withAttributesAs.EntityState!.State.Should().Be(12.5);
        }
    }
}