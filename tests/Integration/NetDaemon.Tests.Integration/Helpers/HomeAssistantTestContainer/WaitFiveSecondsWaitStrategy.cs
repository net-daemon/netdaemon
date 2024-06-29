using System.ComponentModel;
using DotNet.Testcontainers.Configurations;

namespace NetDaemon.Tests.Integration.Helpers.HomeAssistantTestContainer;

/// <summary>
/// Make sure the Home Assistant is up and running before starting tests
/// </summary>
public sealed class WaitFiveSecondsWaitStrategy : IWaitUntil

{
    private readonly long _timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

    public Task<bool> UntilAsync(DotNet.Testcontainers.Containers.IContainer container)
    {
        return Task.FromResult(new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() > _timestamp + 5);
    }
}
