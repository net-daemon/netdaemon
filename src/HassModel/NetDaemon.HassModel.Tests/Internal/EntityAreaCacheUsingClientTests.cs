﻿using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;

using NetDaemon.Client.Common;
using NetDaemon.Client.Common.HomeAssistant.Model;
using NetDaemon.Client.Internal.HomeAssistant.Commands;
using NetDaemon.HassModel.Internal.Client;

namespace NetDaemon.HassModel.Tests.Internal;

public class EntityAreaCacheUsingClientTests
{
    [Fact]
    public async Task EntityIdWithArea_Returns_HassArea()
    {
        // Arrange
        using var testSubject = new Subject<HassEvent>();
        var _hassConnectionMock = new Mock<IHomeAssistantConnection>();
        var haRunnerMock = new Mock<IHomeAssistantRunner>();

        haRunnerMock.SetupGet(n => n.CurrentConnection).Returns(_hassConnectionMock.Object);

        _hassConnectionMock.Setup(
            m => m.SendCommandAndReturnResponseAsync<SimpleCommand, IReadOnlyCollection<HassDevice>>(
                It.IsAny<SimpleCommand>(), It.IsAny<CancellationToken>()
            )).ReturnsAsync(Array.Empty<HassDevice>());

        _hassConnectionMock.Setup(
                m => m.SendCommandAndReturnResponseAsync<SimpleCommand, IReadOnlyCollection<HassArea>>
                (
                    It.IsAny<SimpleCommand>(), It.IsAny<CancellationToken>()
                ))
            .ReturnsAsync(new HassArea[]
            {
                new() {Id = "AreaId", Name = "Area Name"}
            });

        _hassConnectionMock.Setup(
                m => m.SendCommandAndReturnResponseAsync<SimpleCommand, IReadOnlyCollection<HassEntity>>
                (
                    It.IsAny<SimpleCommand>(), It.IsAny<CancellationToken>()
                ))
            .ReturnsAsync(new List<HassEntity>
            {
                new() {EntityId = "sensor.sensor1", AreaId = "AreaId"}
            });

        using var cache = new EntityAreaCache(haRunnerMock.Object, testSubject);

        // Act
        await cache.InitializeAsync(CancellationToken.None);
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
        var _hassConnectionMock = new Mock<IHomeAssistantConnection>();
        var haRunnerMock = new Mock<IHomeAssistantRunner>();

        haRunnerMock.SetupGet(n => n.CurrentConnection).Returns(_hassConnectionMock.Object);


        _hassConnectionMock.Setup(
            m => m.SendCommandAndReturnResponseAsync<SimpleCommand, IReadOnlyCollection<HassDevice>>(
                It.IsAny<SimpleCommand>(), It.IsAny<CancellationToken>()
            )).ReturnsAsync(
            new HassDevice[]
            {
                new() {Id = "DeviceId", AreaId = "AreaId"}
            });

        _hassConnectionMock.Setup(
                m => m.SendCommandAndReturnResponseAsync<SimpleCommand, IReadOnlyCollection<HassArea>>
                (
                    It.IsAny<SimpleCommand>(), It.IsAny<CancellationToken>()
                ))
            .ReturnsAsync(new HassArea[]
            {
                new() {Id = "AreaId", Name = "Area Name"}
            });

        _hassConnectionMock.Setup(
                m => m.SendCommandAndReturnResponseAsync<SimpleCommand, IReadOnlyCollection<HassEntity>>
                (
                    It.IsAny<SimpleCommand>(), It.IsAny<CancellationToken>()
                ))
            .ReturnsAsync(new List<HassEntity>
            {
                new() {EntityId = "sensor.sensor1", AreaId = "AreaId"}
            });

        using var cache = new EntityAreaCache(haRunnerMock.Object, testSubject);

        // Act
        await cache.InitializeAsync(CancellationToken.None);

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
        var _hassConnectionMock = new Mock<IHomeAssistantConnection>();
        var haRunnerMock = new Mock<IHomeAssistantRunner>();

        haRunnerMock.SetupGet(n => n.CurrentConnection).Returns(_hassConnectionMock.Object);

        _hassConnectionMock.Setup(
            m => m.SendCommandAndReturnResponseAsync<SimpleCommand, IReadOnlyCollection<HassDevice>>(
                It.IsAny<SimpleCommand>(), It.IsAny<CancellationToken>()
            )).ReturnsAsync(
            new HassDevice[]
            {
                new() {Id = "DeviceId", AreaId = "AreaId"}
            });

        _hassConnectionMock.Setup(
                m => m.SendCommandAndReturnResponseAsync<SimpleCommand, IReadOnlyCollection<HassArea>>
                (
                    It.IsAny<SimpleCommand>(), It.IsAny<CancellationToken>()
                ))
            .ReturnsAsync(new HassArea[]
            {
                new() {Id = "AreaId", Name = "Area Name"},
                new() {Id = "AreaId2", Name = "Area2 Name"}
            });

        _hassConnectionMock.Setup(
                m => m.SendCommandAndReturnResponseAsync<SimpleCommand, IReadOnlyCollection<HassEntity>>
                (
                    It.IsAny<SimpleCommand>(), It.IsAny<CancellationToken>()
                ))
            .ReturnsAsync(new List<HassEntity>
            {
                new() {EntityId = "sensor.sensor1", DeviceId = "DeviceId", AreaId = "AreaId2"}
            });

        using var cache = new EntityAreaCache(haRunnerMock.Object, testSubject);

        // Act
        await cache.InitializeAsync(CancellationToken.None);

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
        var _hassConnectionMock = new Mock<IHomeAssistantConnection>();
        var haRunnerMock = new Mock<IHomeAssistantRunner>();

        haRunnerMock.SetupGet(n => n.CurrentConnection).Returns(_hassConnectionMock.Object);

        _hassConnectionMock.Setup(
            m => m.SendCommandAndReturnResponseAsync<SimpleCommand, IReadOnlyCollection<HassDevice>>(
                It.IsAny<SimpleCommand>(), It.IsAny<CancellationToken>()
            )).ReturnsAsync(Array.Empty<HassDevice>());

        _hassConnectionMock.Setup(
                m => m.SendCommandAndReturnResponseAsync<SimpleCommand, IReadOnlyCollection<HassArea>>
                (
                    It.IsAny<SimpleCommand>(), It.IsAny<CancellationToken>()
                ))
            .ReturnsAsync(new HassArea[]
            {
                new() {Id = "AreaId", Name = "Area Name"},
                new() {Id = "AreaId2", Name = "Area2 Name"}
            });

        _hassConnectionMock.Setup(
                m => m.SendCommandAndReturnResponseAsync<SimpleCommand, IReadOnlyCollection<HassEntity>>
                (
                    It.IsAny<SimpleCommand>(), It.IsAny<CancellationToken>()
                ))
            .ReturnsAsync(new List<HassEntity>
            {
                new() {EntityId = "sensor.sensor1", AreaId = "AreaId"}
            });

        using var cache = new EntityAreaCache(haRunnerMock.Object, testSubject);

        // Act 1: Init
        await cache.InitializeAsync(CancellationToken.None);

        // Act/Rearrage
        _hassConnectionMock.Setup(
                m => m.SendCommandAndReturnResponseAsync<SimpleCommand, IReadOnlyCollection<HassEntity>>
                (
                    It.IsAny<SimpleCommand>(), It.IsAny<CancellationToken>()
                ))
            .ReturnsAsync(new List<HassEntity>
            {
                new() {EntityId = "sensor.sensor1", AreaId = "AreaId2"}
            });

        // Act 3: now fire a area registry update
        testSubject.OnNext(new HassEvent { EventType = "area_registry_updated" });

        // Assert
        var area = cache.GetArea("sensor.sensor1");
        Assert.NotNull(area);
        Assert.Equal("Area2 Name", area!.Name);
    }
}