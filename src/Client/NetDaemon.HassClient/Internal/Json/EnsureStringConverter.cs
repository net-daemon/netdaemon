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
                var stringValue = reader.GetInt32();
                return stringValue.ToString(CultureInfo.InvariantCulture);

            default:
                reader.Skip();
                return null;
        }
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options) => writer.WriteStringValue(value);
}
