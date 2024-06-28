namespace NetDaemon.Client.Internal.Json;

/// <summary>
/// Base class for converters that ensures the expected suported datatyps
/// </summary>
/// <remarks>
/// This is a workaround to make the serializer to not throw exceptions when there are unexpected datatypes returning from Home Assistant json
/// This converter will only be used when deserializing json
///
/// Note: Tried to make a even smarter generic class but could not get it to avoid recursion
/// </remarks>
internal abstract class EnsureExcpectedDatatypeConverterBase<T> : JsonConverter<T?>
{
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        throw new NotImplementedException();

    public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, typeof(T), options);
    }

    protected static object? ReadTokenSuccessfullyOrNull(ref Utf8JsonReader reader, JsonTokenType[] tokenType)
    {
        if (!tokenType.Contains(reader.TokenType))
        {
            // Skip the children of current token if it is not the expected one
            reader.Skip();
            return null;
        }

        var type =  Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

        try
        {
            return Type.GetTypeCode(type) switch
            {
                TypeCode.String => reader.GetString(),
                TypeCode.Int32 => reader.GetInt32(),
                TypeCode.Int16 => reader.GetInt16(),
                TypeCode.Boolean => reader.GetBoolean(),
                TypeCode.Single => reader.GetSingle(),
                TypeCode.DateTime => reader.GetDateTime(),
                _ => throw new NotImplementedException($"Type {typeof(T)} with timecode {Type.GetTypeCode(type)} is not implemented")
            };
        }
        catch (JsonException)
        {
            // Skip the children of current token
            reader.Skip();
            return null;
        }
        catch (FormatException)
        {
            // We are getting this exception when for example there are a format error of dates etc
            // I am reluctant if this error really should just return null, codereview should discuss
            // Maybe trace log the error?
            reader.Skip();
            return null;
        }
    }
}

/// <summary>
/// Converts a Json element that can be a string or returns null if it is not a string
/// </summary>
internal class EnsureStringConverter : EnsureExcpectedDatatypeConverterBase<string?>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        ReadTokenSuccessfullyOrNull(ref reader, [JsonTokenType.String, JsonTokenType.Null]) as string;
}

/// <summary>
/// Converts a Json element that can be a int or returns null if it is not a int
/// </summary>
internal class EnsureIntConverter : EnsureExcpectedDatatypeConverterBase<int?>
{
    public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        (int?) ReadTokenSuccessfullyOrNull(ref reader, [JsonTokenType.Number, JsonTokenType.Null]);
}

/// <summary>
/// Converts a Json element that can be a short or returns null if it is not a short
/// </summary>
internal class EnsureShortConverter : EnsureExcpectedDatatypeConverterBase<short?>
{
    public override short? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        (short?) ReadTokenSuccessfullyOrNull(ref reader, [JsonTokenType.Number, JsonTokenType.Null]);
}

/// <summary>
/// Converts a Json element that can be a float or returns null if it is not afloat
/// </summary>
internal class EnsureFloatConverter : EnsureExcpectedDatatypeConverterBase<float?>
{
    public override float? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        (float?) ReadTokenSuccessfullyOrNull(ref reader, [JsonTokenType.Number, JsonTokenType.Null]);
}

/// <summary>
/// Converts a Json element that can be a boolean or returns null if it is not a boolean
/// </summary>
internal class EnsureBooleanConverter : EnsureExcpectedDatatypeConverterBase<bool?>
{
    public override bool? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        (bool?) ReadTokenSuccessfullyOrNull(ref reader, [JsonTokenType.True, JsonTokenType.False, JsonTokenType.Null]);
}

/// <summary>
/// Converts a Json element that can be a string or returns null if it is not a string
/// </summary>
internal class EnsureDateTimeConverter : EnsureExcpectedDatatypeConverterBase<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        (DateTime?) ReadTokenSuccessfullyOrNull(ref reader, [JsonTokenType.String, JsonTokenType.Null]);
}

/// <summary>
/// Return all the converters that should be used when deserializing
/// </summary>
internal static class EnsureExpectedDatatypeConverter
{
    public static IList<JsonConverter> Converters() =>
    [
        new EnsureStringConverter(),
        new EnsureIntConverter(),
        new EnsureShortConverter(),
        new EnsureFloatConverter(),
        new EnsureBooleanConverter(),
        new EnsureDateTimeConverter()
    ];
}
