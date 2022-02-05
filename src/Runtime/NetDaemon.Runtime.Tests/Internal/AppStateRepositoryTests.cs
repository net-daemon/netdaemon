using System.Net;
using NetDaemon.Client.Internal.Exceptions;
using NetDaemon.Runtime.Internal;
using NetDaemon.Runtime.Internal.Model;

namespace NetDaemon.Runtime.Tests.Internal;

public class AppStateRepositoryTests
{
    private readonly Mock<IHomeAssistantConnection> _connectionMock = new();
    private readonly AppStateRepository _repository;
    private readonly Mock<IHomeAssistantRunner> _runnerMock = new();

    public AppStateRepositoryTests()
    {
        _runnerMock.SetupGet(x => x.CurrentConnection).Returns(_connectionMock.Object);
        _repository = new AppStateRepository(_runnerMock.Object);
    }

    [Fact]
    public async Task GetOrCreateAsyncShouldSendCreateInputBooleanCommandIfEntityNotExists()
    {
        _connectionMock
            .Setup(n => n.GetApiCallAsync<HassState>("states/input_boolean.netdaemon_some_app_id",
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HomeAssistantApiCallException("", HttpStatusCode.NotFound));

        var result = await _repository.GetOrCreateAsync("some_app_id", CancellationToken.None);

        _connectionMock.Verify(
            n => n.SendCommandAndReturnResponseAsync<CreateInputBooleanHelperCommand, InputBooleanHelper>(
                It.IsAny<CreateInputBooleanHelperCommand>(), It.IsAny<CancellationToken>()), Times.Once);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetOrCreateAsyncShouldNotSendCreateInputBooleanCommandIfEntityExists()
    {
        _connectionMock
            .Setup(n => n.GetApiCallAsync<HassState>("states/input_boolean.netdaemon_some_app_id",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HassState
            {
                EntityId = "input_boolean.netdaemon_some_app_id",
                State = "on"
            });

        _ = await _repository.GetOrCreateAsync("some_app_id", CancellationToken.None);
        _connectionMock.Verify(
            n => n.SendCommandAndReturnResponseAsync<CreateInputBooleanHelperCommand, InputBooleanHelper>(
                It.IsAny<CreateInputBooleanHelperCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("on", true)]
    [InlineData("off", false)]
    public async Task GetOrCreateAsyncShouldReturnCorrectEnabledStateForAppDependingOnStateOfInputBooleanHelper(
        string entityState, bool isEnabled)
    {
        _connectionMock
            .Setup(n => n.GetApiCallAsync<HassState>("states/input_boolean.netdaemon_some_app_id",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HassState
            {
                EntityId = "input_boolean.netdaemon_some_app_id",
                State = entityState
            });


        var result = await _repository.GetOrCreateAsync("some_app_id", CancellationToken.None);

        result.Should().Be(isEnabled);
    }

    [Fact]
    public async Task RemoveNotUsedStatesAsyncShouldRemoveEntitiesThatDoesNotCorrespondToAnAppId()
    {
        var resultList = new[]
        {
            new InputBooleanHelper {Id = "netdaemon_some_app_id", Name = "netdaemon_some_app_id"},
            new InputBooleanHelper {Id = "netdaemon_some_app_id2", Name = "netdaemon_some_app_id2"}
        };
        _connectionMock.Setup(n =>
            n.SendCommandAndReturnResponseAsync<ListInputBooleanHelperCommand,
                IReadOnlyCollection<InputBooleanHelper>>(It.IsAny<ListInputBooleanHelperCommand>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(resultList.ToList());

        var applicationIds = new[] {"some_app_id"};
        await _repository.RemoveNotUsedStatesAsync(applicationIds, CancellationToken.None);

        // var command = new DeleteInputBooleanHelperCommand() {InputBooleanId = "some_app_id2", Type = "input_boolean/list""};
        _connectionMock.Verify(
            n => n.SendCommandAndReturnResponseAsync<DeleteInputBooleanHelperCommand, object?>(
                It.Is<DeleteInputBooleanHelperCommand>(n => n.InputBooleanId == "netdaemon_some_app_id2"),
                It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveNotUsedStatesAsyncShouldRemoveAllHelpersIfNoAppsPresent()
    {
        var resultList = new[]
        {
            new InputBooleanHelper {Id = "netdaemon_some_app_id", Name = "netdaemon_some_app_id"},
            new InputBooleanHelper {Id = "netdaemon_some_app_id2", Name = "netdaemon_some_app_id2"}
        };
        _connectionMock.Setup(n =>
            n.SendCommandAndReturnResponseAsync<ListInputBooleanHelperCommand,
                IReadOnlyCollection<InputBooleanHelper>>(It.IsAny<ListInputBooleanHelperCommand>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(resultList.ToList());

        var applicationIds = new List<string>();
        await _repository.RemoveNotUsedStatesAsync(applicationIds, CancellationToken.None);

        // var command = new DeleteInputBooleanHelperCommand() {InputBooleanId = "some_app_id2", Type = "input_boolean/list""};
        _connectionMock.Verify(
            n => n.SendCommandAndReturnResponseAsync<DeleteInputBooleanHelperCommand, object?>(
                It.Is<DeleteInputBooleanHelperCommand>(n => n.InputBooleanId == "netdaemon_some_app_id"),
                It.IsAny<CancellationToken>()), Times.Once);

        _connectionMock.Verify(
            n => n.SendCommandAndReturnResponseAsync<DeleteInputBooleanHelperCommand, object?>(
                It.Is<DeleteInputBooleanHelperCommand>(n => n.InputBooleanId == "netdaemon_some_app_id2"),
                It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveNotUsedStatesAsyncShouldNotRemoveNonNetDaemonInputBooleans()
    {
        var resultList = new[]
        {
            new InputBooleanHelper {Id = "netdaemon_some_app_id", Name = "netdaemon_some_app_id"},
            new InputBooleanHelper {Id = "netdaemon_some_app_id2", Name = "netdaemon_some_app_id2"},
            new InputBooleanHelper {Id = "non_netdaemon_input_boolean", Name = "non_netdaemon_input_boolean"}
        };
        _connectionMock.Setup(n =>
            n.SendCommandAndReturnResponseAsync<ListInputBooleanHelperCommand,
                IReadOnlyCollection<InputBooleanHelper>>(It.IsAny<ListInputBooleanHelperCommand>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(resultList.ToList());

        var applicationIds = new[] {"some_app_id"};
        await _repository.RemoveNotUsedStatesAsync(applicationIds, CancellationToken.None);

        // var command = new DeleteInputBooleanHelperCommand() {InputBooleanId = "some_app_id2", Type = "input_boolean/list""};
        _connectionMock.Verify(
            n => n.SendCommandAndReturnResponseAsync<DeleteInputBooleanHelperCommand, object?>(
                It.Is<DeleteInputBooleanHelperCommand>(n => n.InputBooleanId == "non_netdaemon_input_boolean"),
                It.IsAny<CancellationToken>()), Times.Never);
    }
}
