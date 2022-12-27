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

        haContextMock.Setup(t => t.GetHassState(It.IsAny<string>())).Returns(hassState);
        
        // ACT
        var target = mapper.Entity(haContextMock.Object, entityId);
       
        // ASSERT
        target.State.Should().Be("state initial");
        target.Attributes!.Name.Should().Be("name initial");

        target.EntityState!.State.Should().Be("state initial");
        target.EntityState!.Attributes!.Name.Should().Be("name initial");
    }

    public record AttributesWithName([property:JsonPropertyName("name")]string? Name);

    [Fact]
    public void ShouldShowStateChangesFromContext()
    {
        var haContextMock = new Mock<IHaContext>();
        var hassStateChangesSubject = new Subject<HassStateChangedEventData>();
        var entityId = "domain.test_entity";
        var mapper = DefaultEntityStateMappers.TypedAttributes<AttributesWithName>();

        var hassState = new HassState
            {
                EntityId = entityId,
                State = "state initial",
                AttributesJson = new { name = "name initial" }.AsJsonElement()
            };

        haContextMock.Setup(t => t.GetHassState(It.IsAny<string>())).Returns(hassState);

        haContextMock.Setup(h => h.HassStateAllChanges()).Returns(hassStateChangesSubject);
        var entityFactory = new EntityGenericFactory(haContextMock.Object);

        var target = mapper.Entity(haContextMock.Object, entityId);
        var stateChangeObserverMock = new Mock<IObserver<IStateChange<string?, AttributesWithName>>>();
        var stateAllChangeObserverMock = new Mock<IObserver<IStateChange<string?, AttributesWithName>>>();

        target.StateAllChanges().Subscribe(stateAllChangeObserverMock.Object);
        target.StateChanges().Subscribe(stateChangeObserverMock.Object);

        hassStateChangesSubject.OnNext(
            new HassStateChangedEventData
            {
                EntityId = entityId,
                NewState = new HassState
                    {
                        EntityId = entityId,
                        State = "old"
                    },
                OldState = new HassState
                    {
                        EntityId = entityId,
                        State = "new"
                    }
            });

        hassStateChangesSubject.OnNext(
            new HassStateChangedEventData
            {
                EntityId = entityId,
                NewState = new HassState
                    {
                        EntityId = entityId,
                        State = "same"
                    },
                OldState = new HassState
                    {
                        EntityId = entityId,
                        State = "same"
                    }
            });

        stateChangeObserverMock.Verify(o => o.OnNext(It.IsAny<IStateChange<string?, AttributesWithName>>()), Times.Once);
        stateAllChangeObserverMock.Verify(o => o.OnNext(It.IsAny<IStateChange<string?, AttributesWithName>>()), Times.Exactly(2));
    }
}