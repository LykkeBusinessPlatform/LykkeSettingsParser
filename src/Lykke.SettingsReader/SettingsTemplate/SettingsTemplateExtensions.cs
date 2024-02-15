using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
    /// <param name="configureFilters">configuration action which allows to add/remove filters for settings template</param>
    /// <returns>IServiceCollection</returns>
    public static IServiceCollection AddSettingsTemplateGenerator(this IServiceCollection services, Action<TemplateFilers> configureFilters = null)
    {
        var filters = new TemplateFilers();
        if (configureFilters != null)
        {
            configureFilters(filters);
        }

        services.AddTransient<IJsonSettingsTemplateGenerator>(provider =>
            new DefaultJsonTemplateGenerator(provider.GetRequiredService<ILoggerFactory>(),
                provider.GetRequiredService<IConfiguration>(), filters));
        return services;
    }
}