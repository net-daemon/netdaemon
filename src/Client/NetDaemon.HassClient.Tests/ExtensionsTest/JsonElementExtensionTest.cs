namespace NetDaemon.HassClient.Tests.ExtensionsTest;

public class JsonElementExtensionTest
{
    [Fact]
    public void TestToJsonElementShouldReturnCorrectElement()
    {
        var cmd = new SimpleCommand("get_services");

        var element = cmd.ToJsonElement();

        element!.Value.GetProperty("type").ToString()
            .Should()
            .Be("get_services");
    }
}