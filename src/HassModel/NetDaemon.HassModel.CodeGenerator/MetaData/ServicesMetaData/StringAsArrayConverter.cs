using System.Diagnostics;
using System.Text.Json.Serialization;

namespace NetDaemon.HassModel.CodeGenerator.Model;

/// <summary>
/// Converts a Json element that can be a string or a string array
/// </summary>
class StringAsArrayConverter : JsonConverter<string[]>
{
    public override string[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return [reader.GetString() ?? throw new UnreachableException("Token is expected to be a string")];
        }

        return JsonSerializer.Deserialize<string[]>(ref reader, options);
    }

    public override void Write(Utf8JsonWriter writer, string[] value, JsonSerializerOptions options) => throw new NotSupportedException();
}