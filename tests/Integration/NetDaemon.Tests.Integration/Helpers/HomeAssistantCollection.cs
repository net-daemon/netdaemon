using Xunit;

namespace NetDaemon.Tests.Integration.Helpers;

[CollectionDefinition("HomeAssistant collection")]
public class HomeAssistantCollection :  ICollectionFixture<HomeAssistantLifetime>
{

}