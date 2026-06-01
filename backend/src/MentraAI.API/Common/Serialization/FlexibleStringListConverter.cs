using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MentraAI.API.Common.Serialization;

public class FlexibleStringListConverter : JsonConverter<List<string>>
{
    public override List<string> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var list = new List<string>();

        if (reader.TokenType == JsonTokenType.StartArray)
        {
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    list.Add(reader.GetString() ?? string.Empty);
                }
                else
                {
                    reader.Skip();
                }
            }
        }
        else if (reader.TokenType == JsonTokenType.StartObject)
        {
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string propertyName = reader.GetString() ?? string.Empty;
                    
                    // Move to property value
                    if (reader.Read())
                    {
                        string propertyValue = string.Empty;
                        if (reader.TokenType == JsonTokenType.String)
                        {
                            propertyValue = reader.GetString() ?? string.Empty;
                        }
                        else
                        {
                            reader.Skip();
                        }

                        if (!string.IsNullOrEmpty(propertyName))
                        {
                            if (!string.IsNullOrEmpty(propertyValue))
                            {
                                list.Add($"{propertyName}: {propertyValue}");
                            }
                            else
                            {
                                list.Add(propertyName);
                            }
                        }
                    }
                }
            }
        }
        else if (reader.TokenType == JsonTokenType.String)
        {
            list.Add(reader.GetString() ?? string.Empty);
        }
        else
        {
            reader.Skip();
        }

        return list;
    }

    public override void Write(Utf8JsonWriter writer, List<string> value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}
