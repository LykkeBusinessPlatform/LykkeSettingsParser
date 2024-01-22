using System;
using Microsoft.Extensions.Configuration;

namespace Lykke.SettingsReader.ConfigurationProvider;

public class HttpConfigurationSource : IConfigurationSource
{
    private readonly Uri _configurationUrl;

    public HttpConfigurationSource(Uri configurationUrl = null)
    {
        _configurationUrl = configurationUrl;
    }
   
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        var settingsUrl  = Environment.GetEnvironmentVariable("SettingsUrl");
        if (_configurationUrl is null && string.IsNullOrEmpty(settingsUrl))
        {
            throw new SettingsSourceException("settingsUrl not specified and environment variable 'SettingsUrl' is not defined");
        }
        return new HttpConfigurationProvider(_configurationUrl ?? new Uri(settingsUrl));
    }
}