namespace NetDaemon.Client.Internal.Json;

class EnsureArrayOfStringConverter : JsonConverter<IReadOnlyList<string>>
{
    private readonly EnsureStringConverter _ensureStringConverter = new();

    public override IReadOnlyList<string> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var list = new List<string>();
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                string? item = _ensureStringConverter.Read(ref reader, typeof(string), options);
                if (item != null)
                {
                    list.Add(item);
                }
            }
            return list;
        }

        string? singleItem = _ensureStringConverter.Read(ref reader, typeof(string), options);
        if (singleItem != null)
        {
            return new List<string> { singleItem };
        }

        return [];
    }

    public override void Write(Utf8JsonWriter writer, IReadOnlyList<string> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var item in value)
        {
            writer.WriteStringValue(item);
        }
        writer.WriteEndArray();
    }
}
