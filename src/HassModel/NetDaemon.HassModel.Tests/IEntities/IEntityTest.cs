using System.Globalization;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using NetDaemon.Client.HomeAssistant.Model;
using NetDaemon.HassModel.Entities;
using NetDaemon.HassModel.Tests.TestHelpers;
using NetDaemon.HassModel.Tests.TestHelpers.HassClient;

namespace NetDaemon.HassModel.Tests.Entities;

public class IEntityTest
{
    [Fact]
    public void ShouldGetStateFromHaContext()
    {
        // ARRANGE
        var haContextMock = new Mock<IHaContext>();
        var entityId = "domain.test_entity";
        var mapper = DefaultEntityStateMappers.TypedAttributes<AttributesWithName>();

        var hassState = new HassState
            {
                EntityId = entityId,
                State = "state initial",
                AttributesJson = new { name = "name initial" }.AsJsonElement()
            };

        haContextMock.Setup(t => t.GetState(It.IsAny<string>(), It.IsAny<Func<HassState?, IEntityState<string?, AttributesWithName>?>>())).Returns(mapper.MapHassState(hassState));
        
        // ACT
        var target = mapper.Entity(haContextMock.Object, entityId);
       
        // ASSERT
        target.State.Should().Be("state initial");
        target.Attributes!.Name.Should().Be("name initial");

        target.EntityState!.State.Should().Be("state initial");
        target.EntityState!.Attributes!.Name.Should().Be("name initial");
    }

    record AttributesWithName([property:JsonPropertyName("name")]string? Name);

    // [Fact]
    // public void ShouldShowStateChangesFromContext()
    // {
    //     var haContextMock = new Mock<IHaContext>();
    //     var stateChangesSubject = new Subject<IStateChange<string, AttributesWithName>>();
    //     var entityId = "domain.test_entity";
    //     var mapper = DefaultEntityStateMappers.TypedAttributes<AttributesWithName>();

    //     var hassState = new HassState
    //         {
    //             EntityId = entityId,
    //             State = "state initial",
    //             AttributesJson = new { name = "name initial" }.AsJsonElement()
    //         };

    //     haContextMock.Setup(t => t.GetState(It.IsAny<string>(), It.IsAny<Func<HassState?, IEntityState<string?, AttributesWithName>?>>())).Returns(mapper.MapHassState(hassState));

    //     haContextMock.Setup(h => h.StateAllChangesGeneric(It.IsAny<Func<HassStateChangedEventData, IStateChange<string?, AttributesWithName>>>)).Returns(stateChangesSubject);
    //     var entityFactory = new EntityGenericFactory(haContextMock.Object);

    //     var target = entityFactory.CreateIEntity(entityId, DefaultEntityStateMappers.TypedAttributes<AttributesWithName>());
    //     var stateChangeObserverMock = new Mock<IObserver<IStateChange>>();
    //     var stateAllChangeObserverMock = new Mock<IObserver<IStateChange>>();

    //     target.StateAllChanges().Subscribe(stateAllChangeObserverMock.Object);
    //     target.StateChanges().Subscribe(stateChangeObserverMock.Object);

    //     stateChangesSubject.OnNext(
    //         new StateChangeGeneric
    //         (
    //             target,
    //             new EntityStateGeneric(entityId, "old"),
    //             new EntityStateGeneric(entityId, "new")
    //         ));

    //     stateChangesSubject.OnNext(
    //         new StateChangeGeneric
    //         (
    //             target,
    //             new EntityStateGeneric(entityId, "same"),
    //             new EntityStateGeneric(entityId, "same")
    //         ));

    //     stateChangeObserverMock.Verify(o => o.OnNext(It.IsAny<IStateChange>()), Times.Once);
    //     stateAllChangeObserverMock.Verify(o => o.OnNext(It.IsAny<IStateChange>()), Times.Exactly(2));
    // }
}