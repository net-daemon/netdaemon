using System.Text.Json.Serialization;

namespace NetDaemon.HassModel.CodeGenerator;

internal class NullableBoolJsonConverter : JsonConverter<bool?>
{
    public override void Write(Utf8JsonWriter writer, bool? value, JsonSerializerOptions options) =>
        writer.WriteBooleanValue(value ?? false);

    public override bool? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.String => bool.TryParse(reader.GetString(), out var b) ? b : throw new JsonException(),
            JsonTokenType.Number => reader.TryGetInt64(out long l) ? Convert.ToBoolean(l) : reader.TryGetDouble(out double d) && Convert.ToBoolean(d),
            _ => throw new JsonException(),
        };
}