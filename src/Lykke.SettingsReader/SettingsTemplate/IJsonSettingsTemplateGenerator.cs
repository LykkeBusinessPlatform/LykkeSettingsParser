namespace Lykke.SettingsReader.SettingsTemplate;

/// <summary>
/// Interface for IJsonSettingsTemplateGenerator
/// </summary>
public interface IJsonSettingsTemplateGenerator
{
    /// <summary>
    /// This method will generate JsonSettingsTemplate in json.
    /// </summary>
    /// <returns>string as json</returns>
    string GenerateJsonSettingsTemplate();
}