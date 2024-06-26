using System.Diagnostics;

namespace NetDaemon.Client.Internal.Json;

/// <summary>
/// Converts a Json element that can be a string or a string array
/// </summary>
class EnsureStringConverter : JsonConverter<string?>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return reader.GetString() ?? throw new UnreachableException("Token is expected to be a string");
        }

        reader.Skip();
        return null;
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options) => throw new NotSupportedException();
}
