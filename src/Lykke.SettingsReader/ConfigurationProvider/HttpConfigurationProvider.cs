﻿using System;
using System.Text.Json;
using Lykke.SettingsReader.Helpers;
using Newtonsoft.Json.Linq;

namespace Lykke.SettingsReader.ConfigurationProvider;

internal class HttpConfigurationProvider : Microsoft.Extensions.Configuration.ConfigurationProvider
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
        Data = JsonParser.Parse(jsonDocument);
    }
}