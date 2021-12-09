using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.Client;
using JoySoftware.HomeAssistant.Model;
using Moq;
using NetDaemon.HassModel.Internal;
using Xunit;

namespace NetDaemon.HassModel.Tests.Internal;

public class EntityAreaCacheTests
{
    [Fact]
    public async Task EntityIdWithArea_Returns_HassArea()
    {
        // Arrange
        using var testSubject = new Subject<HassEvent>();
        var hassClientMock = new Mock<IHassClient>();


        hassClientMock.Setup(m => m.GetDevices()).ReturnsAsync(Array.Empty<HassDevice>());
        hassClientMock.Setup(m => m.GetAreas()).ReturnsAsync(new HassArea[]
        {
            new() { Id = "AreaId", Name = "Area Name" }
        });
        hassClientMock.Setup(m => m.GetEntities()).ReturnsAsync(new HassEntity[]
        {
            new() { EntityId = "sensor.sensor1", AreaId = "AreaId" }
        });

        using var cache = new EntityAreaCache(hassClientMock.Object, testSubject);

        // Act
        await cache.InitializeAsync();

        // Assert
        var area = cache.GetArea("sensor.sensor1");
        Assert.NotNull(area);
        Assert.Equal("Area Name", area!.Name);
    }

    [Fact]
    public async Task EntityIdWithOutArea_ButDeviceArea_Returns_HassArea()
    {
        // Arrange
        using var testSubject = new Subject<HassEvent>();
        var hassClientMock = new Mock<IHassClient>();


        hassClientMock.Setup(m => m.GetDevices()).ReturnsAsync(new HassDevice[]
        {
            new() { Id = "DeviceId", AreaId = "AreaId" }
        });
        hassClientMock.Setup(m => m.GetAreas()).ReturnsAsync(new HassArea[]
        {
            new() { Id = "AreaId", Name = "Area Name" }
        });
        hassClientMock.Setup(m => m.GetEntities()).ReturnsAsync(new HassEntity[]
        {
            new() { EntityId = "sensor.sensor1", DeviceId = "DeviceId"}
        });

        using var cache = new EntityAreaCache(hassClientMock.Object, testSubject);

        // Act
        await cache.InitializeAsync();

        // Assert
        var area = cache.GetArea("sensor.sensor1");
        Assert.NotNull(area);
        Assert.Equal("Area Name", area!.Name);
    }
    
    [Fact]
    public async Task EntityIdWithArea_AndDeviceArea_Returns_EntityHassArea()
    {
        // Arrange
        using var testSubject = new Subject<HassEvent>();
        var hassClientMock = new Mock<IHassClient>();


        hassClientMock.Setup(m => m.GetDevices()).ReturnsAsync(new HassDevice[]
        {
            new() { Id = "DeviceId", AreaId = "AreaId" }
        });
        hassClientMock.Setup(m => m.GetAreas()).ReturnsAsync(new HassArea[]
        {
            new() { Id = "AreaId", Name = "Area Name" },
            new() { Id = "AreaId2", Name = "Area2 Name" }
        });
        hassClientMock.Setup(m => m.GetEntities()).ReturnsAsync(new HassEntity[]
        {
            new() { EntityId = "sensor.sensor1", DeviceId = "DeviceId", AreaId = "AreaId2"}
        });

        using var cache = new EntityAreaCache(hassClientMock.Object, testSubject);

        // Act
        await cache.InitializeAsync();

        // Assert
        var area = cache.GetArea("sensor.sensor1");
        Assert.NotNull(area);
        Assert.Equal("Area2 Name", area!.Name);
    }
    
    [Fact]
    public async Task EntityArea_Updates()
    {
        // Arrange
        using var testSubject = new Subject<HassEvent>();
        var hassClientMock = new Mock<IHassClient>();

        hassClientMock.Setup(m => m.GetDevices()).ReturnsAsync(Array.Empty<HassDevice>());
        hassClientMock.Setup(m => m.GetAreas()).ReturnsAsync(new HassArea[]
        {
            new() { Id = "AreaId", Name = "Area Name" },
            new() { Id = "AreaId2", Name = "Area2 Name" }
        });
        hassClientMock.Setup(m => m.GetEntities()).ReturnsAsync(new HassEntity[]
        {
            new() { EntityId = "sensor.sensor1", AreaId = "AreaId" }
        });

        using var cache = new EntityAreaCache(hassClientMock.Object, testSubject);
        
        // Act 1: Init
        await cache.InitializeAsync();

        // Act/Rearrage
        hassClientMock.Setup(m => m.GetEntities()).ReturnsAsync(new HassEntity[]
        {
            new() { EntityId = "sensor.sensor1", AreaId = "AreaId2" }
        });
        
        // Act 3: now fire a area registry update
        testSubject.OnNext(new HassEvent() {EventType = "area_registry_updated"});
        
        // Assert
        var area = cache.GetArea("sensor.sensor1");
        Assert.NotNull(area);
        Assert.Equal("Area2 Name", area!.Name);
    }
}