using System;
using Xunit;
using JoySoftware.HomeAssistant.NetDaemon.DaemonRunner.Service;

namespace DaemonRunner.Tests
{
    public class StringExtensionsTests
    {
        [Theory]
        [InlineData("ThisIsAField", "this_is_a_field")]
        [InlineData("KitchenLight", "kitchen_light")]
        [InlineData("kitchenlight", "kitchenlight")]
        public void AllToPythonConversionShouldBeOk(string camelCase, string pythonStyle)
        {
            Assert.Equal(pythonStyle, camelCase.ToPythonStyle());
        }

        [Theory]
        [InlineData("this_is_a_field", "ThisIsAField")]
        [InlineData("kitchen_light", "KitchenLight")]
        [InlineData("kitchenlight", "Kitchenlight")]
        [InlineData("kitchen__light", "KitchenLight")]
        public void AllToCamelCaseConversionShouldBeOk(string camelCase, string pythonStyle)
        {
            Assert.Equal(pythonStyle, camelCase.ToCamelCase());
        }
    }
}
