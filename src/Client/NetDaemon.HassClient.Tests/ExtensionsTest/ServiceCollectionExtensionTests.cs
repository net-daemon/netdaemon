using NetDaemon.Client.Extensions;

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
        var apiManager = serviceProvider.GetService<IHomeAssistantApiManager>();
        var connection = serviceProvider.GetService<IHomeAssistantConnection>();
        hassClient.Should().NotBeNull();
        hassRunner.Should().NotBeNull();
        apiManager.Should().NotBeNull();

        Assert.Null(connection);
        Assert.Throws<NullReferenceException>(() => serviceProvider.GetService<IObservable<HassEvent>>());
    }
}