using System.Reflection;
using System.Text.Json.Serialization;

namespace NetDaemon.HassModel.CodeGenerator.Model;

class SelectorConverter : JsonConverter<Selector>
{
    public override Selector? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var element = JsonSerializer.Deserialize<JsonElement>(ref reader);
        var property = element.EnumerateObject().FirstOrDefault();
        return getSelector(property.Name, property.Value);
    }

    public override void Write(Utf8JsonWriter writer, Selector value, JsonSerializerOptions options) => throw new NotSupportedException();

    private static Selector? getSelector(string selectorName, JsonElement element)
    {
        // Find a matching Type for this selector
        Type[] executingAssemblyTypes = Assembly.GetExecutingAssembly().GetTypes();
        var selectorType = executingAssemblyTypes.FirstOrDefault(x => string.Equals($"{selectorName}Selector", x.Name, StringComparison.OrdinalIgnoreCase));

        if (selectorType is null)
        {
            return new Selector { Type = selectorName};
        }

        var deserialize = (Selector?)element.Deserialize(selectorType, ServiceMetaDataParser.SerializerOptions);
        deserialize ??= (Selector)Activator.CreateInstance(selectorType)!;

        return deserialize with { Type = selectorName };
    }
}