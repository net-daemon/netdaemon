using System.Buffers;
using System.Text.Json;

namespace NetDaemon.Model3
{
    /// <summary>
    ///     Useful extension methods used
    /// </summary>
    internal static class NetDaemonExtensions
    {
        public static T ToObject<T>(this JsonElement element, JsonSerializerOptions? options = null)
        {
            var bufferWriter = new ArrayBufferWriter<byte>();
            using (var writer = new Utf8JsonWriter(bufferWriter))
            {
                element.WriteTo(writer);
            }

            return JsonSerializer.Deserialize<T>(bufferWriter.WrittenSpan, options) ?? default!;
        } 

        public static (string Domain, string Entity) SplitEntityId(this string entityId)
        {
            var firstDot = entityId.IndexOf('.', System.StringComparison.InvariantCulture);
            return (entityId[.. firstDot ], entityId[ firstDot .. ]);
        }
     
    }
}