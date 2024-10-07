using Microsoft.Extensions.Hosting;

namespace Lykke.SettingsReader.SettingsTemplate;

internal static class EnvironmentConstants
{
    private const string DeploymentEnvironment = "Deployment";
    private const string DevEnvironment = "Dev";
    private const string TestEnvironment = "Test";
    public static readonly string[] NonProdEnvs =
    {
        Environments.Development, DeploymentEnvironment, DevEnvironment, TestEnvironment
    };
}