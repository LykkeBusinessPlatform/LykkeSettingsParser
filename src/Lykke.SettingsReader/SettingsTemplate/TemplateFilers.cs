using System.Collections;
using System.Collections.Generic;

namespace Lykke.SettingsReader.SettingsTemplate;
/// <summary>
/// Class which allow add and remove template filters.
/// </summary>
public class TemplateFilers : IEnumerable<string>
{
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
    public const string applicationName = "applicationName";
    public const string URLS = "URLS";
    public const string VERSION = "VERSION";

    public List<string> Filters { get; } = new()
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
        applicationName,
        URLS,
        VERSION
    };

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
    {
        return Filters.GetEnumerator();
    }

    public IEnumerator GetEnumerator()
    {
        return Filters.GetEnumerator();
    }
}