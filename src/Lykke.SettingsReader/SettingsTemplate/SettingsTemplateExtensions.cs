using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.SettingsReader.SettingsTemplate;

/// <summary>
/// Ioc extensions for registering IJsonSettingsTemplateGenerator
/// </summary>
public static class SettingsTemplateExtensions
{
    /// <summary>
    /// Register IJsonSettingsTemplateGenerator implementation in transient scope
    /// </summary>
    /// <param name="services"></param>
    /// <returns>IServiceCollection</returns>
    public static IServiceCollection AddSettingsTemplateGenerator(this IServiceCollection services)
    {
        services.AddSingleton(provider => provider.GetRequiredService<IConfiguration>().GetChildren());
        services.AddTransient<IJsonSettingsTemplateGenerator, DefaultJsonTemplateGenerator>();
        return services;
    }
}