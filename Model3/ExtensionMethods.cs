using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Globalization;
using System.Text;
using System.Text.Json;
using NetDaemon.Common;

namespace Model3
{
    /// <summary>
    ///     Useful extension methods used
    /// </summary>
    public static class NetDaemonExtensions
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
            var firstDot = entityId.IndexOf('.');
            return (entityId[.. firstDot ], entityId[ firstDot .. ]);
        }
     
    }
}