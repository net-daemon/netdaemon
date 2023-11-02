using System.Text.Json.Serialization;

namespace NetDaemon.HassModel.CodeGenerator.Model;

class StringAsDoubleConverter : JsonConverter<double?>
{
    public override double? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Some fields (step) can have a string or a numeric value. If it is a string we will try to parse it to a decimal
        return reader.TokenType switch
        {
            JsonTokenType.Number => reader.GetDouble(),
            JsonTokenType.String => double.TryParse(reader.GetString(), out var d) ? d : null,
            _ => null,
        };
    }

    public override void Write(Utf8JsonWriter writer, double? value, JsonSerializerOptions options) => throw new NotSupportedException();
}
