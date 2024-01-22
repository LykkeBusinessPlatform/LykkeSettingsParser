using System;
using Microsoft.Extensions.Configuration;

namespace Lykke.SettingsReader.ConfigurationProvider;

/// <summary>
/// Extensions which allows to add http endpoint as configuration source 
/// </summary>
public static class HttpConfigurationExtensions
{
    /// <summary>
    /// Method for adding http configuration source
    /// </summary>
    /// <param name="configurationBuilder">IConfigurationBuilder interface.</param>
    /// <param name="configurationUrl">Url for http endpoint from which configuration will be retrieved. If null SettingsUrl env variable has to be set.</param>
    /// <returns>IConfigurationBuilder</returns>
    public static IConfigurationBuilder AddHttpSourceConfiguration(this IConfigurationBuilder configurationBuilder, Uri configurationUrl = null)
    {
        configurationBuilder.Add(new HttpConfigurationSource(configurationUrl));
        return configurationBuilder;
    }
}