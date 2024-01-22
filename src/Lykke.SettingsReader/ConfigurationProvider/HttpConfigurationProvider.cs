using System;
using System.Text.Json;
using Lykke.SettingsReader.Helpers;
using Newtonsoft.Json.Linq;

namespace Lykke.SettingsReader.ConfigurationProvider;

public class HttpConfigurationProvider : Microsoft.Extensions.Configuration.ConfigurationProvider
{
    private readonly Uri _configurationUri;

    private static readonly JsonDocumentOptions JsonDocumentOptions = new JsonDocumentOptions
    {
        CommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public HttpConfigurationProvider(Uri configurationUri)
    {
        _configurationUri = configurationUri;
    }

    public override void Load()
    {
        var config = HttpClientProvider.Client.GetStringAsync(_configurationUri).GetAwaiter().GetResult();
        var jsonDocument = JsonDocument.Parse(config, JsonDocumentOptions);
        var configDictionary = JsonParser.Parse(jsonDocument);

        foreach (var keyValuePair in configDictionary)
        {
            Data.Add(keyValuePair.Key, keyValuePair.Value);
        }
    }
}