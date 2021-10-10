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

        public static (string? Left, string Right) SplitAtDot(this string id)
        {
            var firstDot = id.IndexOf('.', System.StringComparison.InvariantCulture);
            if (firstDot == -1) return (null, id);
            
            return (id[.. firstDot ], id[ firstDot .. ]);
        }
     
    }
}