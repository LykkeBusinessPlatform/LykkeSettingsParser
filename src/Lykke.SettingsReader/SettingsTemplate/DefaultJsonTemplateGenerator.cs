using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Lykke.SettingsReader.SettingsTemplate;
/// <summary>
///  Default implementation of IJsonSettingsTemplateGenerator. Its using collection of IConfigurationSections interface and converts it into json settings template.
/// </summary>
internal class DefaultJsonTemplateGenerator : IJsonSettingsTemplateGenerator
{
    private readonly IEnumerable<IConfigurationSection>  _configurationSections;

    public DefaultJsonTemplateGenerator(IEnumerable<IConfigurationSection> sections)
    {
        _configurationSections = sections  ?? throw new ArgumentNullException(nameof(sections));
    }

    public string GenerateJsonSettingsTemplate()
    {
        var json = JsonSerializer.Serialize(_configurationSections, new JsonSerializerOptions
        {
            Converters = { new SettingsTemplateConverter() },
            WriteIndented = true
        });

        return json;
    }
}