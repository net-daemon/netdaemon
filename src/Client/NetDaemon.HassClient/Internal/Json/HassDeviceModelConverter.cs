namespace NetDaemon.Client.Internal.Json;
internal class HassDeviceModelConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Number:
            {
                var stringValue = reader.GetInt32();
                return stringValue.ToString(CultureInfo.InvariantCulture);
            }
            case JsonTokenType.String:
                return reader.GetString();
            default:
                throw new System.Text.Json.JsonException();
        }
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value);
}