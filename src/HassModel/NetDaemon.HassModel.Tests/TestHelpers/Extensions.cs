using System.Text.Json;

namespace NetDaemon.HassModel.Tests.TestHelpers
{
    internal static class Extensions
    {
        public static JsonElement AsJsonElement(this object value)
        {
            var jsonString = JsonSerializer.Serialize(value);
            return JsonSerializer.Deserialize<JsonElement>(jsonString);
        }
    }
}