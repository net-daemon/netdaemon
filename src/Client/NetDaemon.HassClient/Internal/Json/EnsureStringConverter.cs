namespace NetDaemon.Client.Internal.Json;

/// <summary>
/// Converts a Json element that can be a string or a number to as string, and returns null if not
/// </summary>
class EnsureStringConverter : JsonConverter<string?>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                return reader.GetString();

            case JsonTokenType.Number:
                if (reader.TryGetInt32(out var intValue))
                    return intValue.ToString(CultureInfo.InvariantCulture);
                if (reader.TryGetInt64(out var longValue))
                    return longValue.ToString(CultureInfo.InvariantCulture);
                if (reader.TryGetDouble(out var doubleValue))
                    return doubleValue.ToString(CultureInfo.InvariantCulture);

                reader.Skip();
                return null;

            default:
                reader.Skip();
                return null;
        }
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options) => writer.WriteStringValue(value);
}
