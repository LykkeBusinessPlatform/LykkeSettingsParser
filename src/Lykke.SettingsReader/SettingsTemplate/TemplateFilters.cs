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
    /// <summary>
    /// This regex will match variables added by k8s service discovery https://kubernetes.io/docs/concepts/services-networking/service/#environment-variables
    /// Examples: SOMESERVICE_PORT","SOMESERVICE_PORT_5012_TCP","SOMESERVICE_PORT_5012_TCP_ADDR","SOMESERVICE_PORT_5012_TCP_PORT","SOMESERVICE_PORT_5012_TCP_PROTO","SOMESERVICE_SERVICE_HOST","SOMESERVICE_SERVICE_PORT_HTTP"
    /// </summary>
    public const string KubernetesServiceDiscoveryVariablesPattern =
        @"^(\w+)_PORT(?:_(\w+)_TCP(?:_(ADDR|PORT|PROTO))?)?$|^(\w+)_SERVICE_(HOST|PORT)(?:_(HTTP))?$|^(\w+)_SERVICE_PORT_TCP$";

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

    private List<string> Filters { get; } = new()
    {
        KubernetesServiceDiscoveryVariablesPattern,
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
        VERSION
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