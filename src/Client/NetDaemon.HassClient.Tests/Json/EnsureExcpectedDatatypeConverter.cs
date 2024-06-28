using System.Globalization;
using NetDaemon.Client.Internal.Json;

namespace NetDaemon.HassClient.Tests.Json;

public class EnsureExpectedDatatypeConverterTest
{
    private readonly JsonSerializerOptions _defaultSerializerOptions = DefaultSerializerOptions.DeserializationOptions;

    [Fact]
    public void TestConvertAllSupportedTypesConvertsCorrectly()
    {
        var json = @"
        {
            ""string"": ""string"",
            ""int"": 10000,
            ""short"": 2000,
            ""float"": 1.1,
            ""bool"": true,
            ""datetime"": ""2019-02-16T18:11:44.183673+00:00""
        }
        ";

        var record = JsonSerializer.Deserialize<SupportedTypesTestRecord>(json, _defaultSerializerOptions);
        record!.SomeString.Should().Be("string");
        record!.SomeInt.Should().Be(10000);
        record!.SomeShort.Should().Be(2000);
        record!.SomeFloat.Should().Be(1.1f);
        record!.SomeBool.Should().BeTrue();
        record!.SomeDateTime.Should().Be(DateTime.Parse("2019-02-16T18:11:44.183673+00:00", CultureInfo.InvariantCulture));
    }

    [Fact]
    public void TestConvertAllSupportedTypesConvertsToNullWhenWrongDatatypeCorrectly()
    {
        var json = @"
        {
            ""string"": 1,
            ""int"": ""10000"",
            ""short"": ""2000"",
            ""float"": {""property"": ""100""},
            ""bool"": ""hello"",
            ""datetime"": ""test""
        }
        ";

        var record = JsonSerializer.Deserialize<SupportedTypesTestRecord>(json, _defaultSerializerOptions);
        record!.SomeString.Should().BeNull();
        record!.SomeInt.Should().BeNull();
        record!.SomeShort.Should().BeNull();
        record!.SomeFloat.Should().BeNull();
        record!.SomeBool.Should().BeNull();
        record!.SomeDateTime.Should().BeNull();
    }

    [Fact]
    public void TestConvertAllSupportedTypesConvertsToNullWhenNullJsonCorrectly()
    {
        var json = @"
        {
            ""string"": null,
            ""int"": null,
            ""short"": null,
            ""float"": null,
            ""bool"": null,
            ""datetime"": null
        }
        ";

        var record = JsonSerializer.Deserialize<SupportedTypesTestRecord>(json, _defaultSerializerOptions);
        record!.SomeString.Should().BeNull();
        record!.SomeInt.Should().BeNull();
        record!.SomeShort.Should().BeNull();
        record!.SomeFloat.Should().BeNull();
        record!.SomeBool.Should().BeNull();
        record!.SomeDateTime.Should().BeNull();
    }

    [Fact]
    public void TestConvertAllNonNullShouldThrowExcptionIfThereAreADatatypeError()
    {
        var json = @"
        {
            ""string"": 1
        }
        ";
        var result = JsonSerializer.Deserialize<SupportedTypesNonNullTestRecord>(json, _defaultSerializerOptions);
        // The string can be null even if not nullable so it will not throw.
        result!.SomeString.Should().BeNull();
        json = @"
        {
            ""int"": ""10000""
        }
        ";
        FluentActions.Invoking(() => JsonSerializer.Deserialize<SupportedTypesNonNullTestRecord>(json, _defaultSerializerOptions))
            .Should().Throw<JsonException>();
        json = @"
        {
            ""short"": ""2000""
        }
        ";
        FluentActions.Invoking(() => JsonSerializer.Deserialize<SupportedTypesNonNullTestRecord>(json, _defaultSerializerOptions))
            .Should().Throw<JsonException>();
        json = @"
        {
            ""float"": {""property"": ""100""}
        }
        ";
        FluentActions.Invoking(() => JsonSerializer.Deserialize<SupportedTypesNonNullTestRecord>(json, _defaultSerializerOptions))
            .Should().Throw<JsonException>();
        json = @"
        {
            ""bool"": ""hello""
        }
        ";
        FluentActions.Invoking(() => JsonSerializer.Deserialize<SupportedTypesNonNullTestRecord>(json, _defaultSerializerOptions))
            .Should().Throw<JsonException>();
        json = @"
        {
            ""datetime"": ""test""
        }
        ";
        FluentActions.Invoking(() => JsonSerializer.Deserialize<SupportedTypesNonNullTestRecord>(json, _defaultSerializerOptions))
            .Should().Throw<JsonException>();
    }
}

public record SupportedTypesTestRecord
{
    [JsonPropertyName("string")] public string? SomeString { get; init; }
    [JsonPropertyName("int")] public int? SomeInt { get; init; }
    [JsonPropertyName("short")] public short? SomeShort { get; init; }
    [JsonPropertyName("float")] public float? SomeFloat { get; init; }
    [JsonPropertyName("bool")] public bool? SomeBool { get; init; }
    [JsonPropertyName("datetime")] public DateTime? SomeDateTime { get; init; }
}

public record SupportedTypesNonNullTestRecord
{
    [JsonPropertyName("string")] public string SomeString { get; init; } = string.Empty;
    [JsonPropertyName("int")] public int SomeInt { get; init; }
    [JsonPropertyName("short")] public short SomeShort { get; init; }
    [JsonPropertyName("float")] public float SomeFloat { get; init; }
    [JsonPropertyName("bool")] public bool SomeBool { get; init; }
    [JsonPropertyName("datetime")] public DateTime SomeDateTime { get; init; }
}
