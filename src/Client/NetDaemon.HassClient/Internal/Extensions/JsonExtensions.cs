namespace NetDaemon.Client.Internal.Extensions;

internal static class JsonExtensions
{
    public static JsonElement? ToJsonElement<T>(this T source, JsonSerializerOptions? options = null)
    {
        if (source == null) return null;
        return JsonSerializer.SerializeToElement(source, options);
    }
}