using System.Text.Json.Serialization;

namespace NetDaemon.HassModel.CodeGenerator.Model;

/// <summary>
/// Converts either a single object or an array to an array 
/// </summary>
class SingleObjectAsArrayConverter<T> : JsonConverter<T[]>
{
    public override T[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            return [JsonSerializer.Deserialize<T>(ref reader, options)!];
        }

        return JsonSerializer.Deserialize<T[]>(ref reader, options);
    }

    public override void Write(Utf8JsonWriter writer, T[] value, JsonSerializerOptions options)
        => throw new NotSupportedException();
}