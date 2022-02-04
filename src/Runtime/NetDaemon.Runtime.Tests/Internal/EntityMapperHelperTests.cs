using NetDaemon.Runtime.Internal;

namespace NetDaemon.Runtime.Tests.Internal;

public class EntityMapperHelperTests
{
    [Theory]
    [InlineData("lowercase", "lowercase")]
    [InlineData("lower.namespace.lowercase", "lower_namespace_lowercase")]
    [InlineData("Namespace.Class", "namespace_class")]
    [InlineData("Namespace.ClassNameWithUpperAndLower", "namespace_class_name_with_upper_and_lower")]
    [InlineData("ALLUPPERCASE", "alluppercase")]
    [InlineData("DIClass", "diclass")]
    [InlineData("DiClass", "di_class")]
    [InlineData("Di_Class", "di_class")]
    [InlineData("di_class", "di_class")]
    [InlineData("di__class", "di_class")]
    public void TestToSafeHomeAssistantEntityIdFromApplicationIdShouldGiveCorrectName(string fromId, string toId)
    {
        var expected = $"input_boolean.netdaemon_{toId}";
        EntityMapperHelper.ToEntityIdFromApplicationId(fromId).Should().Be(expected);
    }

    [Theory]
    [InlineData("lowercase", "lowercase")]
    [InlineData("lower.namespace.lowercase", "lower_namespace_lowercase")]
    [InlineData("Namespace.Class", "namespace_class")]
    [InlineData("Namespace.ClassNameWithUpperAndLower", "namespace_class_name_with_upper_and_lower")]
    [InlineData("ALLUPPERCASE", "alluppercase")]
    [InlineData("DIClass", "diclass")]
    [InlineData("DiClass", "di_class")]
    [InlineData("Di_Class", "di_class")]
    [InlineData("di_class", "di_class")]
    [InlineData("di__class", "di_class")]
    public void TestToSafeHomeAssistantEntityIdFromApplicationIdShouldGiveCorrectNameDevelopment(string fromId, string toId)
    {
        var expected = $"input_boolean.dev_netdaemon_{toId}";
        EntityMapperHelper.ToEntityIdFromApplicationId(fromId, true).Should().Be(expected);
    }
}
