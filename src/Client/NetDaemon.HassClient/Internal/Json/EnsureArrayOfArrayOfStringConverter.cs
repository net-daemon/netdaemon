namespace NetDaemon.Client.Internal.Json;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Converts a Json element that can be an array of arrays of strings to a list of lists of strings.
/// </summary>
class EnsureArrayOfArrayOfStringConverter : JsonConverter<IReadOnlyList<IReadOnlyList<string>>>
{
    private readonly EnsureStringConverter _ensureStringConverter = new();

    public override IReadOnlyList<IReadOnlyList<string>> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var list = new List<IReadOnlyList<string>>();
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType == JsonTokenType.StartArray)
                {
                    var innerList = new List<string>();
                    while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                    {
                        string? item = _ensureStringConverter.Read(ref reader, typeof(string), options);
                        if (item != null)
                        {
                            innerList.Add(item);
                        }
                    }
                    list.Add(innerList);
                }
            }
            return list;
        }

        reader.Skip();
        return [];
    }

    public override void Write(Utf8JsonWriter writer, IReadOnlyList<IReadOnlyList<string>> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var innerList in value)
        {
            writer.WriteStartArray();
            foreach (var item in innerList)
            {
                writer.WriteStringValue(item);
            }
            writer.WriteEndArray();
        }
        writer.WriteEndArray();
    }
}
