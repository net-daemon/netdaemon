namespace NetDaemon.Client.Internal.Json;

//<summary>
// Default options for serialization when serializing and deserializing json
// </summary>
internal static class DefaultSerializerOptions
{
    public static JsonSerializerOptions DeserializationOptions => new()
    {
        Converters =
        {
            new EnsureStringConverter(),
            new EnsureIntConverter(),
            new EnsureShortConverter(),
            new EnsureBooleanConverter(),
            new EnsureFloatConverter(),
            new EnsureDateTimeConverter()
        }
    };

    public static JsonSerializerOptions SerializationOptions => new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}
