using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace Lykke.SettingsReader.SettingsTemplate;

internal class SettingsTemplateConverter : JsonConverter<IConfiguration>
{
    private readonly Regex _arrayElementKeyPattern = new Regex(@"^\d+$"); //numbers only  e.g 0

    public override IConfiguration Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException("Deserializing settings template into IConfiguration its not supported.");
    }

    public override void Write(Utf8JsonWriter writer, IConfiguration value, JsonSerializerOptions options)
    {
        var isArray = false;
        var children = value.GetChildren().ToList();
        if (value is IConfigurationSection section)
        {
            isArray = children.Any() && children.All(configurationSection => _arrayElementKeyPattern.Match(configurationSection.Key).Success);
            var isArrayElement = _arrayElementKeyPattern.Match(section.Key).Success;
            var isObject = section.Value is null;
            if (isArray)
            {
                writer.WriteStartArray(section.Key);
            }
            else if (isObject)
            {
                if (isArrayElement)
                {
                    writer.WriteStartObject();
                }
                else
                {
                    writer.WriteStartObject(section.Key);
                }
            }
            else //its not array or object so its simple value
            {
                SetTypeOfValue(writer, section, isArrayElement);
                return;
            }
        }
        else //its not section so its root
        {
            writer.WriteStartObject();
        }

        if (isArray) //take only first element of array for template
        {
            children = new List<IConfigurationSection>() { children.First() };
        }

        foreach (var child in children)
        {
            Write(writer, child, options);
        }

        if (isArray)
        {
            writer.WriteEndArray();
        }
        else
        {
            writer.WriteEndObject();
        }
    }

    private static void SetTypeOfValue(Utf8JsonWriter writer, IConfigurationSection section, bool isArrayElement)
    {
        if (int.TryParse(section.Value, out _))
        {
            WriteString("Integer");
        }
        else if (double.TryParse(section.Value, out _))
        {
            WriteString(nameof(Double));
        }
        else if (bool.TryParse(section.Value, out _))
        {
            WriteString(nameof(Boolean));
        }
        else if (Guid.TryParse(section.Value, out _))
        {
            WriteString(nameof(Guid));
        }
        else if (DateTime.TryParse(section.Value, out _))
        {
            WriteString(nameof(DateTime));
        }
        else
        {
            WriteString(nameof(String));
        }

        return;

        void WriteString(string value)
        {
            if (isArrayElement)
            {
                writer.WriteStringValue(value);
            }
            else
            {
                writer.WriteString(section.Key, value);
            }
        }
    }
}