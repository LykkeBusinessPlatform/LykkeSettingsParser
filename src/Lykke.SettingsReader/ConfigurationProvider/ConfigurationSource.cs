namespace Lykke.SettingsReader.ConfigurationProvider;

/// <summary>
/// Specifies the type of configuration source that was added to IConfigurationRoot
/// </summary>
public enum ConfigurationSource
{
    /// <summary>
    /// Indicates that the configuration source is an HTTP endpoint.
    /// </summary>
    Http,
    /// <summary>
    /// Indicates that the configuration source is a JSON file.
    /// </summary>
    Json
}