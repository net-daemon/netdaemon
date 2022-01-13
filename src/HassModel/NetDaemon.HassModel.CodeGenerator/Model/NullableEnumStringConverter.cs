using System.ComponentModel;
using System.Text.Json.Serialization;

namespace NetDaemon.HassModel.CodeGenerator.Model;

internal class NullableEnumStringConverter<TEnum> : JsonConverter<TEnum>
{
    private readonly bool _isNullable;
    private readonly Type _enumType;

    public NullableEnumStringConverter() {
        _isNullable = Nullable.GetUnderlyingType(typeof(TEnum)) is not null;

        _enumType = _isNullable ?
            Nullable.GetUnderlyingType(typeof(TEnum))! :
            typeof(TEnum);
    }

    public override TEnum? Read(ref Utf8JsonReader reader,
        Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();

        if (_isNullable && string.IsNullOrEmpty(value))
            return default;

        if (string.IsNullOrEmpty(value))
        {
            throw new InvalidEnumArgumentException(
                $"A value must be provided for non-nullable enum property of type \"{_enumType.FullName}\"");
        }

        if (Enum.TryParse(_enumType, value, true, out var result))
        {
            return (TEnum) result!;
        }

        return default;
    }

    public override void Write(Utf8JsonWriter writer,
        TEnum value, JsonSerializerOptions options)
    {
        throw new NotSupportedException($"Serialization not supported for the enum \"{_enumType.Name}\"");
    }
}