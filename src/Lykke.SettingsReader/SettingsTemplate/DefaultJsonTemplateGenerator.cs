using System;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Lykke.SettingsReader.SettingsTemplate;
/// <summary>
///  Default implementation of IJsonSettingsTemplateGenerator. Its using IConfiguration interface and converts it into json settings template.
/// </summary>
internal class DefaultJsonTemplateGenerator : IJsonSettingsTemplateGenerator
{
    private readonly IConfiguration _configuration;

    public DefaultJsonTemplateGenerator(IConfiguration configuration)
    {
        _configuration = configuration  ?? throw new ArgumentNullException(nameof(configuration));
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