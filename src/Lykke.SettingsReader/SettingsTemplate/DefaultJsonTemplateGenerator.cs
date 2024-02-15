using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Lykke.SettingsReader.SettingsTemplate;
/// <summary>
///  Default implementation of IJsonSettingsTemplateGenerator. Its using IConfiguration interface and converts it into json settings template.
/// </summary>
internal class DefaultJsonTemplateGenerator : IJsonSettingsTemplateGenerator
{
    private readonly ILoggerFactory  _loggerFactory;
    private readonly IConfiguration _configuration;
    private readonly TemplateFilers _regexFilters;

    public DefaultJsonTemplateGenerator(ILoggerFactory  loggerFactory,IConfiguration configuration, TemplateFilers regexFilters)
    {
        _loggerFactory = loggerFactory;
        _configuration = configuration  ?? throw new ArgumentNullException(nameof(configuration));
        _regexFilters = regexFilters;
    }

    public string GenerateJsonSettingsTemplate()
    {
        var json = JsonSerializer.Serialize(_configuration, new JsonSerializerOptions
        {
            Converters = { new SettingsTemplateConverter(_loggerFactory,_regexFilters) },
            WriteIndented = true
        });

        return json;
    }
}