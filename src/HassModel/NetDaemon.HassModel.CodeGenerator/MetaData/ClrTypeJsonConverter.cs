using System.Text.Json.Serialization;

namespace NetDaemon.HassModel.CodeGenerator;

/// <summary>
/// Json(De)Serializes a System.Type using a 'friendly name'
/// </summary>
internal class ClrTypeJsonConverter : JsonConverter<Type>
{
    public override Type Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var typeName = reader.GetString();
        if (typeName == null) return typeof(object);

        return Type.GetType(typeName) ?? throw new InvalidOperationException($@"Type {typeName} is not found when deserializing");
    }

    public override void Write(Utf8JsonWriter writer, Type value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}