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

    [Fact]
    public void TestToObjectFromElementShouldReturnCorrectObject()
    {
        var jsonDoc = JsonDocument.Parse(
            @"
            {
                ""id"": 3,
                ""type"": ""result"",
                ""success"": true,
                ""result"": null
                }
            "
        );
        var msg = jsonDoc.RootElement.ToObject<HassMessage>();

        msg!.Id
            .Should()
            .Be(3);
    }
}
