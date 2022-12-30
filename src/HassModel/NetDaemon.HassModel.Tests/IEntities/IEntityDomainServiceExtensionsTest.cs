
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
    public static void TurnOn<TState>(this IEntity<TState, LightAttributes> light, double? brightness = null) => light.CallService("turn_on_generic", brightness);

    // TestLightEntity is defined in ./GlobalIEntityUsing.cs with 
    // `global using TestLightEntity = NetDaemon.HassModel.Entities.IEntity<string?, NetDaemon.HassModel.Tests.Entities.LightAttributes>;
    public static void TurnOn(this TestLightEntity light, double? brightness, bool extra) => light.CallService("turn_on_testlightentity", brightness + (extra ? 10d : 0d));
}

public class IEntityDomainServiceExtensionsTest
{
    public IEntityStateMapper<double?, LightAttributes> DoubleLightMapper = DefaultEntityStateMappers.NumericTypedAttributes<LightAttributes>();
    public IEntityStateMapper<string?, LightAttributes> LightMapper = DefaultEntityStateMappers.TypedAttributes<LightAttributes>();
    
    [Fact]
    public void CanCallDomainServiceOnStateChange()
    {
        // Arrange
        var entityId = "light.one";
        var haContextMock = new Mock<IHaContext>();
        var hassStateChangesSubject = new Subject<HassStateChangedEventData>();
        haContextMock.Setup(h => h.HassStateAllChanges()).Returns(hassStateChangesSubject);

        var target1 = LightMapper.Entity(haContextMock.Object, entityId);
        var target2 = DoubleLightMapper.Entity(haContextMock.Object, entityId);
        var brightness = 50d;

        // Act
        target1.StateChanges().Subscribe(e => e.Entity.TurnOn(brightness: brightness)); // Uses the generic extension method, because of the parameter set
        target1.StateChanges().Subscribe(e => e.Entity.TurnOn(brightness: brightness, true));   // Uses the TestLightEntity specific extension method
        target2.StateChanges().Subscribe(e => e.Entity.TurnOn(brightness: brightness)); // Uses the generic extension method

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
        haContextMock.Verify(h => h.CallService("light", "turn_on_generic", It.Is<ServiceTarget>(t => t.EntityIds!.Single() == target1.EntityId), brightness), Times.Exactly(2));
        haContextMock.Verify(h => h.CallService("light", "turn_on_testlightentity", It.Is<ServiceTarget>(t => t.EntityIds!.Single() == target1.EntityId), brightness + 10d), Times.Once);
    }
}