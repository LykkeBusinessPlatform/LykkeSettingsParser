using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Lykke.SettingsReader.SettingsTemplate;

internal class JsonSettingsTemplateGenerator : IJsonSettingsTemplateGenerator
{
    private readonly IConfiguration _configuration;

    public JsonSettingsTemplateGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateJsonSettingsTemplate()
    {
        var json = JsonSerializer.Serialize(_configuration, new JsonSerializerOptions
        {
            Converters = { new SettingsTemplateConverter() },
            WriteIndented = true
        });

        return json;
    }
}