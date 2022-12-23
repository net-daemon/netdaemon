using System.Globalization;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using System.Threading;
using NetDaemon.HassModel.Entities;
using NetDaemon.HassModel.Tests.TestHelpers;
using NetDaemon.HassModel.Tests.TestHelpers.HassClient;

namespace NetDaemon.HassModel.Tests.Entities;

public class IEntityTest
{
    [Fact]
    public void ShouldWrapStateFromHaContext()
    {
        // Arrange
        var entityId = "domain.testEntity";

        var haContextMock = new Mock<IHaContext>();

        var entityState =
            new EntityStateGeneric
            (
                entityId,
                "CurrentState",
                new { name = "FirstName" }.AsJsonElement()
            );

        haContextMock.Setup(t => t.GetStateGeneric(entityId)).Returns(entityState);

        var entityFactory = new EntityGenericFactory(haContextMock.Object);

        // Act
        var target = entityFactory.CreateIEntity(entityId, DefaultEntityStateMappers.TypedAttributes<TestEntityAttributes>());

        // Assert
        target.State.Should().Be("CurrentState");
        target.Attributes!.Name.Should().Be("FirstName");

        target.EntityState!.State.Should().Be("CurrentState");
        target.EntityState!.Attributes!.Name.Should().Be("FirstName");

        // Act2: update the state
        var newEntityState =
            new EntityStateGeneric
            (
                entityId,
                "NewState",
                new { name = "SecondName" }.AsJsonElement()
            );

        haContextMock.Setup(t => t.GetStateGeneric(entityId)).Returns(newEntityState);

        // Assert
        target.State.Should().Be("NewState");
        target.Attributes!.Name.Should().Be("SecondName");

        target.EntityState!.State.Should().Be("NewState");
        target.EntityState!.Attributes!.Name.Should().Be("SecondName");
    }

    [Fact]
    public void ShouldShowStateChangesFromContext()
    {
        var entityId = "domain.testEntity";
        var stateChangesSubject = new Subject<IStateChange>();
        var haContextMock = new Mock<IHaContext>();
        haContextMock.Setup(h => h.StateAllChangesGeneric()).Returns(stateChangesSubject);
        var entityFactory = new EntityGenericFactory(haContextMock.Object);

        var target = entityFactory.CreateIEntity(entityId, DefaultEntityStateMappers.TypedAttributes<TestEntityAttributes>());
        var stateChangeObserverMock = new Mock<IObserver<IStateChange>>();
        var stateAllChangeObserverMock = new Mock<IObserver<IStateChange>>();

        target.StateAllChanges().Subscribe(stateAllChangeObserverMock.Object);
        target.StateChanges().Subscribe(stateChangeObserverMock.Object);

        stateChangesSubject.OnNext(
            new StateChangeGeneric
            (
                target,
                new EntityStateGeneric(entityId, "old"),
                new EntityStateGeneric(entityId, "new")
            ));

        stateChangesSubject.OnNext(
            new StateChangeGeneric
            (
                target,
                new EntityStateGeneric(entityId, "same"),
                new EntityStateGeneric(entityId, "same")
            ));

        stateChangeObserverMock.Verify(o => o.OnNext(It.IsAny<IStateChange>()), Times.Once);
        stateAllChangeObserverMock.Verify(o => o.OnNext(It.IsAny<IStateChange>()), Times.Exactly(2));
    }

    [Fact]
    public void ShouldCallServiceOnContext()
    {
        var entityId = "domain.testEntity";
        var haContextMock = new Mock<IHaContext>();
        var entityFactory = new EntityGenericFactory(haContextMock.Object);

        var entity = entityFactory.CreateIEntity(entityId, DefaultEntityStateMappers.TypedAttributes<TestEntityAttributes>());
        var data = "payload";

        entity.CallService("service", data);

        haContextMock.Verify(h => h.CallService("domain", "service", It.Is<ServiceTarget>(t => t.EntityIds!.Single() == entity.EntityId), data), Times.Once);
    }

    [Fact]
    public void AsNumeric_Than_WithAttributesAs()
    {
        var entityId = "sensor.temperature";

        var haContextMock = new HaContextMock();
        haContextMock.Setup(m => m.GetStateGeneric(entityId)).Returns(
            new EntityStateGeneric(entityId, "12.3", JsonSerializer.Deserialize<JsonElement>(@"{""setPoint"": 21.5, ""units"": ""Celcius""}"))
        );

        var entityFactory = new EntityGenericFactory(haContextMock.Object);

        var entity = entityFactory.CreateIEntity(entityId, DefaultEntityStateMappers.TypedAttributes<TestEntityAttributes>());
        entity.State.Should().Be("12.3");

        // Act: AsNumeric
        var numericEntity = entity.AsNumeric();

        // Assert
        numericEntity.State!.Value!.Should().Be(12.3d);
        numericEntity.EntityState!.State!.Value!.Should().Be(12.3d);
        numericEntity.StateAllChanges().Where(e => e.New?.State > 1.2);
        // Act: WithNewAttributesAs
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
        haContextMock.Setup(m => m.GetStateGeneric(entityId)).Returns(
            new EntityStateGeneric(entityId, "12.3", JsonSerializer.Deserialize<JsonElement>(@"{""setPoint"": 21.5, ""units"": ""Celcius""}"))
        );

        var entityFactory = new EntityGenericFactory(haContextMock.Object);
        var entity = entityFactory.CreateIEntity(entityId, DefaultEntityStateMappers.Base);

        // Act: WithAttributesAs
        IEntity<string?, TestSensorAttributes> withAttributes = entity.WithAttributesAs<TestSensorAttributes>();
        IEntity<double?, TestSensorAttributes> numericEntity = withAttributes.AsNumeric();

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

        haContextMock.StateAllChangeGenericSubject.OnNext(new StateChangeGeneric(entity, new EntityStateGeneric(entity.EntityId), new EntityStateGeneric(entity.EntityId)));
        numericWithAttributes.StateAllChanges().Where(e => e.New?.State > 1.2 && e.Entity != null).Subscribe();

    }

    record TestSensorAttributes(double setPoint, string units);

    [Fact]
    public void NumericShouldShowStateChangesFromContext()
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
        haContextMock.Setup(m => m.GetStateGeneric(entityId)).Returns(new EntityStateGeneric(entityId, "12.5"));

        var entity = new EntityGenericFactory(haContextMock.Object).CreateIEntity(entityId, DefaultEntityStateMappers.Base);

        var numericEntity = entity.AsNumeric();
        numericEntity.State.Should().Be(12.5);
        numericEntity.EntityState!.State.Should().Be(12.5);

        var withAttributesAs = numericEntity.WithAttributesAs<TestEntityAttributes>();
        withAttributesAs.State.Should().Be(12.5);
        withAttributesAs.EntityState!.State.Should().Be(12.5);
    }
}