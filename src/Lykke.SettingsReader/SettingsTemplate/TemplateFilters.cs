using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Lykke.SettingsReader.SettingsTemplate;
/// <summary>
/// Class which allow add and remove template filters.
/// </summary>
public class TemplateFilters
{
    public const string ASPNETCORE_URLS = "ASPNETCORE_URLS";
    public const string HOME = "HOME";
    public const string HOSTNAME = "HOSTNAME";
    public const string PATH = "PATH";
    public const string RUNNING_IN_CONTAINER = "RUNNING_IN_CONTAINER";
    public const string DOTNET_RUNNING_IN_CONTAINER = "DOTNET_RUNNING_IN_CONTAINER";
    public const string DOTNET_SYSTEM_GLOBALIZATION_INVARIANT = "DOTNET_SYSTEM_GLOBALIZATION_INVARIANT";
    public const string DOTNET_VERSION = "DOTNET_VERSION";
    public const string SYSTEM_GLOBALIZATION_INVARIANT = "SYSTEM_GLOBALIZATION_INVARIANT";
    public const string APPLICATIONNAME = "applicationName";
    public const string URLS = "URLS";
    public const string VERSION = "VERSION";
    public const string CONTENT_ROOT = "contentRoot";
    public const string SERVICE_HOST = "_SERVICE_HOST";
    public const string PORT = "_PORT";
    public const string SETTINGS_URL = "SettingsUrl";
 
    private List<string> Filters { get; } = new()
    {
        ASPNETCORE_URLS,
        HOME,
        HOSTNAME,
        PATH,
        RUNNING_IN_CONTAINER,
        DOTNET_RUNNING_IN_CONTAINER,
        DOTNET_SYSTEM_GLOBALIZATION_INVARIANT,
        DOTNET_VERSION,
        SYSTEM_GLOBALIZATION_INVARIANT,
        APPLICATIONNAME,
        URLS,
        VERSION,
        CONTENT_ROOT,
        SERVICE_HOST,
        PORT,
        SETTINGS_URL
    };

    /// <summary>
    /// Checks is input matching any filter using Regex.Match
    /// </summary>
    /// <param name="variableKey"></param>
    /// <returns>true or false</returns>
    public bool Matching(string variableKey)
    {
       return Filters.Any(pattern => Regex.Match(variableKey, pattern).Success);
    }

    /// <summary>
    /// Adds regex pattern filter
    /// </summary>
    /// <param name="regexPattern"></param>
    public void AddFilter(string regexPattern)
        => Filters.Add(regexPattern);
    
    /// <summary>
    /// Removing filter
    /// </summary>
    /// <param name="regexPattern"></param>
    public void RemoveFilter(string regexPattern)
        => Filters.Add(regexPattern);
    
}