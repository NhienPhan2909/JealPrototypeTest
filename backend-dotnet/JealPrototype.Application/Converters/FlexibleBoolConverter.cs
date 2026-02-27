using System.Text.Json;
using System.Text.Json.Serialization;

namespace JealPrototype.Application.Converters;

/// <summary>
/// Handles EasyCars API boolean fields that are returned as strings (e.g., "", "Yes", "No")
/// instead of JSON true/false values.
/// </summary>
public class FlexibleBoolConverter : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.True) return true;
        if (reader.TokenType == JsonTokenType.False) return false;

        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();
            return value?.Trim().ToUpperInvariant() is "YES" or "TRUE" or "1" or "Y";
        }

        if (reader.TokenType == JsonTokenType.Number)
            return reader.GetInt32() != 0;

        if (reader.TokenType == JsonTokenType.Null)
            return false;

        throw new JsonException($"Cannot convert token type {reader.TokenType} to bool.");
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        => writer.WriteBooleanValue(value);
}
