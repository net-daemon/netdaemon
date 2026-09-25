using Xunit;

namespace NetDaemon.Tests.Integration.Helpers;

/// <summary>
/// Defines the shared Home Assistant integration test collection.
/// </summary>
[CollectionDefinition("HomeAssistant collection")]
public class HomeAssistantCollection :  ICollectionFixture<HomeAssistantLifetime>
{

}
