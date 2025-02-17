using System;

using Lykke.SettingsReader.ConfigurationProvider;

using Microsoft.Extensions.Configuration;

namespace Lykke.SettingsReader.Extensions;

/// <summary>
/// Extensions which allows to add http endpoint or json file as a configuration source
/// </summary>
public static class ConfigurationBuilderExtensions
{
    /// <summary>
    /// Method for adding configuration source
    /// </summary>
    /// <param name="configurationBuilder">IConfigurationBuilder interface.</param>
    /// <param name="source">
    /// An output parameter that indicates the type of configuration source that was added.
    /// If the method adds an HTTP-based configuration source, this will be set to <see cref="ConfigurationSource.Http"/>.
    /// Otherwise, it will be set to <see cref="ConfigurationSource.Json"/>.
    /// </param>
    /// <param name="configurationUrlOrPath">Url for http endpoint from which configuration will be retrieved or path to json file. If null SettingsUrl env variable has to be set.</param>
    /// <returns>IConfigurationBuilder</returns>
    public static IConfigurationBuilder TryAddConfigurationSource(this IConfigurationBuilder configurationBuilder, out ConfigurationSource source, string configurationUrlOrPath = null)
    {
        var settingsUrl = configurationUrlOrPath ?? Environment.GetEnvironmentVariable("SettingsUrl");

        if (string.IsNullOrWhiteSpace(settingsUrl))
        {
            throw new ArgumentException("configurationUrlOrPath is not specified and environment variable 'SettingsUrl' is not defined");
        }

        if (settingsUrl.StartsWith("http"))
        {
            configurationBuilder.Add(new HttpConfigurationSource(new Uri(settingsUrl)));
            source = ConfigurationSource.Http;
        }
        else
        {
            configurationBuilder.AddJsonFile(settingsUrl, true);
            source = ConfigurationSource.Json;
        }

        return configurationBuilder;
    }

    /// <summary>
    /// Method for adding configuration source
    /// </summary>
    /// <param name="configurationBuilder">IConfigurationBuilder interface.</param>
    /// <param name="configurationUrlOrPath">Url for http endpoint from which configuration will be retrieved or path to json file. If null SettingsUrl env variable has to be set.</param>
    /// <returns>IConfigurationBuilder</returns>
    public static IConfigurationBuilder TryAddConfigurationSource(this IConfigurationBuilder configurationBuilder, string configurationUrlOrPath = null)
    {
        return TryAddConfigurationSource(configurationBuilder, out _, configurationUrlOrPath);
    }
}