namespace NetDaemon.Client.Internal.Extensions;
internal static class JsonExtensions
{
    public static T? ToObject<T>(this JsonElement element, JsonSerializerOptions? options = null)
    {
        var bufferWriter = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(bufferWriter))
        {
            element.WriteTo(writer);
        }

        return JsonSerializer.Deserialize<T?>(bufferWriter.WrittenSpan, options) ?? default!;
    }
    public static JsonElement? ToJsonElement<T>(this T source, JsonSerializerOptions? options = null)
    {
        if (source == null) return null;
        var json = JsonSerializer.Serialize<T>(source, options);
        return JsonDocument.Parse(json).RootElement;
    }
}