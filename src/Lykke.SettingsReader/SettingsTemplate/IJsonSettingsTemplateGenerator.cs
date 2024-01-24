namespace Lykke.SettingsReader.SettingsTemplate;

/// <summary>
/// Interface for IJsonSettingsTemplateGenerator
/// </summary>
public interface IJsonSettingsTemplateGenerator
{
    /// <summary>
    /// This method will generate JsonSettingsTemplate in json. Default implementation is taking IConfiguration interface and converts it into json settings template.
    /// </summary>
    /// <returns>string as json</returns>
    string GenerateJsonSettingsTemplate();
}