using System.Collections.Generic;
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

// Sample code to create with code generator
//
// This was put together in a rush to demonstrate the technique. I didn't reference the format required for the data parameter in CallService,
// so this is almost certainly an invalid call, but it should demonstrate the technique. I'll clean up and broaden all these test in the coming
// days as my time frees up.
//
// The basic idea for the code generator is to create extension methods on Attributes type. I believe that this is very similar to the current behavior.
public record LightAttributes([property:JsonPropertyName("brightness")]double? Brightness);

public static class LightServiceExtensions
{
    public static void TurnOn<TState>(this IEntity<TState, LightAttributes> light, double? brightness = null) => light.CallService("turn_on", brightness);
}

public class IEntityDomainServiceExtensionsTest
{
    public IEntityStateMapper<double?, LightAttributes> LightMapper = DefaultEntityStateMappers.NumericTypedAttributes<LightAttributes>();
    
    [Fact]
    public void CanCallDomainServiceOnStateChange()
    {
        // Arrange
        var entityId = "light.one";
        var haContextMock = new Mock<IHaContext>();
        var hassStateChangesSubject = new Subject<HassStateChangedEventData>();
        haContextMock.Setup(h => h.HassStateAllChanges()).Returns(hassStateChangesSubject);

        var lightEntity = LightMapper.Entity(haContextMock.Object, entityId);
        var brightness = 50d;

        // Act
        lightEntity.StateChanges().Subscribe(e => e.Entity.TurnOn(brightness: brightness));

        hassStateChangesSubject.OnNext(
            new HassStateChangedEventData
            {
                EntityId = entityId,
                NewState = new HassState
                    {
                        EntityId = entityId,
                        State = "off"
                    },
                OldState = new HassState
                    {
                        EntityId = entityId,
                        State = "on"
                    }
            });

        // Assert
        haContextMock.Verify(h => h.CallService("light", "turn_on", It.Is<ServiceTarget>(t => t.EntityIds!.Single() == lightEntity.EntityId), brightness), Times.Once);
    }
}