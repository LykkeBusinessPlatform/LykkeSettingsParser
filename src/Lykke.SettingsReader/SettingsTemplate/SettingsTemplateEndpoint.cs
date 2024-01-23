using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.SettingsReader.SettingsTemplate;

public static class SettingsTemplateEndpoint
{
    public static IEndpointRouteBuilder AddSettingsTemplateEndpoint<TSettings>(this IEndpointRouteBuilder builder, string path = "/settings/template")
    {
        builder.MapGet(path, (IConfiguration config) =>
        {
            var envVariables = Environment.GetEnvironmentVariables();
            var configInstance = Activator.CreateInstance(typeof(TSettings));
            config.Bind(configInstance);
          
            var json = JsonSerializer.Serialize(configInstance, new JsonSerializerOptions
            {
                Converters = { new ConfigurationConverter() },
                WriteIndented = true
            });


            return Task.FromResult(Results.Text(json, contentType: "application/json"));
        });
        return builder;
    }
}

internal class ConfigurationConverter : JsonConverter<IConfiguration>
{
    public override IConfiguration Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var root = new ConfigurationRoot(new List<IConfigurationProvider>(new[]
            { new MemoryConfigurationProvider(new MemoryConfigurationSource()) }));

        var pathParts = new Stack<string>();
        string currentProperty = null;
        string currentPath = null;
        while (reader.Read() && (reader.TokenType != JsonTokenType.EndObject || pathParts.Count > 0))
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.PropertyName:
                    currentProperty = reader.GetString();
                    break;
                case JsonTokenType.String:
                    if (pathParts.Count == 0)
                        root[currentProperty] = reader.GetString();
                    else
                        root[ConfigurationPath.Combine(currentPath, currentProperty)] = reader.GetString();
                    break;
                case JsonTokenType.StartObject:
                    pathParts.Push(currentProperty);
                    currentPath = ConfigurationPath.Combine(pathParts);
                    break;
                case JsonTokenType.EndObject:
                    pathParts.Pop();
                    currentPath = ConfigurationPath.Combine(pathParts);
                    break;
            }
        }

        return root;
    }

    public override void Write(Utf8JsonWriter writer, IConfiguration value, JsonSerializerOptions options)
    {
        if (value is IConfigurationSection section)
        {
            if (section.Value is null)
                writer.WriteStartObject(section.Key);
            else
            {
                writer.WriteString(section.Key, section.Value);
                return;
            }
        }
        else
            writer.WriteStartObject();

        foreach (var child in value.GetChildren())
            Write(writer, child, options);

        writer.WriteEndObject();
    }
}