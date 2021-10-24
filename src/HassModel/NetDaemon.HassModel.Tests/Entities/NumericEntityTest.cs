using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;
using Moq;
using NetDaemon.HassModel.Common;
using NetDaemon.HassModel.Entities;
using Xunit;

namespace NetDaemon.HassModel.Tests.Entities
{
    public class NumericEntityTest
    {
        [Fact]
        public void AsNumeric_Than_WithAttributesAs()
        {
            var entityId = "sensor.temperature";

            var haContextMock = new Mock<IHaContext>();
            haContextMock.Setup(m => m.GetState(entityId)).Returns(
                new EntityState { EntityId = entityId, State = "12.3",
                    AttributesJson = JsonSerializer.Deserialize<JsonElement>(@"{""setPoint"": 21.5, ""units"": ""Celcius""}")
                        });
            
            var entity = new Entity(haContextMock.Object, entityId);
            entity.State.Should().Be("12.3");
            
            // Act: AsNumeric
            var numericEntity = entity.AsNumeric();
            
            // Assert
            numericEntity.State!.Value!.Should().Be(12.3d);
            numericEntity.EntityState!.State!.Value!.Should().Be(12.3d);

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
            var withAttributes = entity.WithAttributesAs<TestSensorAttributes>();

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
    }
}