using System;
using Lykke.SettingsReader.ReloadingManager.Configuration;
using Microsoft.Extensions.Configuration;

namespace Lykke.SettingsReader
{
    /// <summary>
    /// Extensions for IConfiguration
    /// </summary>
    public static class SettingsConfiguratorExtensions
    {
        public static readonly string DefaultConfigurationKey = "SettingsUrl";

        /// <summary>
        /// Loads settings
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="key">key name in the configuration</param>
        /// <param name="configure">action to configure settings</param>
        /// <param name="throwExceptionOnCheckError">if services check will fail, throw exception</param>
        /// <typeparam name="TSettings">model for settings</typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        [Obsolete("Use LoadSettings method with Action")]
        public static IReloadingManagerWithConfiguration<TSettings> LoadSettings<TSettings>(
            this IConfiguration configuration,
            string key = null,
            Action<TSettings> configure = null,
            bool throwExceptionOnCheckError = false
        )
            where TSettings : class
        {
            key = key ?? DefaultConfigurationKey;
            var settingsUrl = configuration[key];

            if (string.IsNullOrEmpty(settingsUrl))
            {
                throw new InvalidOperationException($"The connection string to the settings was not found by configuration key '{key}'");
            }

            if (settingsUrl.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
            {
                return new SettingsServiceReloadingManager<TSettings>(settingsUrl, null, configure, throwExceptionOnCheckError);
            }

            return new LocalSettingsReloadingManager<TSettings>(settingsUrl, null, configure, throwExceptionOnCheckError);
        }

        /// <summary>
        /// Loads settings
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="slackNotificationOptions">action to configure connection string, queue name and sender name for slack notifications</param>
        /// <param name="key">key name in the configuration</param>
        /// <param name="configure">action to configure settings</param>
        /// <param name="throwExceptionOnCheckError">if services check will fail, throw exception</param>
        /// <typeparam name="TSettings">model for settings</typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IReloadingManagerWithConfiguration<TSettings> LoadSettings<TSettings>(
            this IConfiguration configuration,
            Action<SlackNotificationOptions<TSettings>> slackNotificationOptions,
            string key = null,
            Action<TSettings> configure = null,
            bool throwExceptionOnCheckError = false
        )
            where TSettings : class
        {
            key = key ?? DefaultConfigurationKey;
            var settingsUrl = configuration[key];

            if (string.IsNullOrEmpty(settingsUrl))
            {
                throw new InvalidOperationException($"The connection string to the settings was not found by configuration key '{key}'");
            }
            
            if (slackNotificationOptions == null)
                throw new ArgumentNullException(nameof(slackNotificationOptions));

            if (settingsUrl.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
            {
                return new SettingsServiceReloadingManager<TSettings>(settingsUrl, slackNotificationOptions, configure, throwExceptionOnCheckError);
            }

            return new LocalSettingsReloadingManager<TSettings>(settingsUrl, slackNotificationOptions, configure, throwExceptionOnCheckError);
        }
    }
}
