using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MentraAI.API.Modules.Onboarding.DTOs.Requests;

/// <summary>
/// A custom System.Text.Json converter that allows deserializing different JSON types 
/// (arrays, numbers, booleans, objects, and strings) into a single serialized string.
/// </summary>
public class FlexibleStringConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using (JsonDocument document = JsonDocument.ParseValue(ref reader))
        {
            JsonElement root = document.RootElement;
            return root.ValueKind switch
            {
                JsonValueKind.String => root.GetString(),
                JsonValueKind.Number => root.GetRawText(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Null => null,
                JsonValueKind.Array => root.GetRawText(),
                JsonValueKind.Object => root.GetRawText(),
                _ => root.GetRawText()
            };
        }
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}
