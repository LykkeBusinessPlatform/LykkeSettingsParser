using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.SettingsReader.SettingsTemplate;

/// <summary>
/// Extensions for adding endpoint which exposes settings template generated at runtime.
/// </summary>
public static class SettingsTemplateEndpoint
{
    /// <summary>
    /// Adding settings template endpoint at default route /settings/template
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="path">optional route on which this endpoint can be exposed</param>
    /// <returns>settings template in json format</returns>
    public static IEndpointRouteBuilder AddSettingsTemplateEndpoint(this IEndpointRouteBuilder builder,
        string path = "/settings/template")
    {
        builder.MapGet(path, async (context) =>
        {
            var generator = context.RequestServices.GetRequiredService<IJsonSettingsTemplateGenerator>();
            var json = generator.GenerateJsonSettingsTemplate();
            context.Response.Headers.ContentType = "application/json";
            await context.Response.WriteAsync(json);
        });
        return builder;
    }
}