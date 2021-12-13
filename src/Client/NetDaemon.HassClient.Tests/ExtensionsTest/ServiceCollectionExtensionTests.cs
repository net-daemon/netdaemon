using NetDaemon.Client.Common.Extensions;
namespace NetDaemon.HassClient.Tests.ExtensionsTest;
public class ServiceCollectionExtensionTests
{

    [Fact]
    public void TestServiceCollectionExtension()
    {
        var services = new ServiceCollection();
        services.AddHomeAssistantClient();
        var serviceProvider = services.BuildServiceProvider();
        var hassClient = serviceProvider.GetService<IHomeAssistantClient>();
        var hassRunner = serviceProvider.GetService<IHomeAssistantRunner>();
        hassClient.Should().NotBeNull();
        hassRunner.Should().NotBeNull();
    }
}