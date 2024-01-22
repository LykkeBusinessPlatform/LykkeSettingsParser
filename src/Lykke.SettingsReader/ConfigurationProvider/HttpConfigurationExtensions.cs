using System;
using Microsoft.Extensions.Configuration;

namespace Lykke.SettingsReader.ConfigurationProvider;

public static class HttpConfigurationExtensions
{
    public static IConfigurationBuilder AddHttpConfiguration(this IConfigurationBuilder configurationBuilder, Uri configurationUrl = null)
    {
        configurationBuilder.Add(new HttpConfigurationSource(configurationUrl));
        return configurationBuilder;
    }
}