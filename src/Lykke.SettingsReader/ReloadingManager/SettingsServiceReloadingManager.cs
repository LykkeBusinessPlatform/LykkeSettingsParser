using System;
using System.Threading.Tasks;
using Lykke.SettingsReader.Helpers;
using Lykke.SettingsReader.ReloadingManager.Configuration;

namespace Lykke.SettingsReader
{
    public class SettingsServiceReloadingManager<TSettings> : ReloadingManagerWithConfigurationBase<TSettings>
    {
        private readonly string _settingsUrl;
        private readonly Action<SlackNotificationOptions<TSettings>> _slackNotificationOptions;
        private readonly Action<TSettings> _configure;
        private readonly bool _throwExceptionOnCheckError;

        public SettingsServiceReloadingManager(string settingsUrl, 
            Action<SlackNotificationOptions<TSettings>> slackNotificationOptions, 
            Action<TSettings> configure = null,
            bool throwExceptionOnCheckError = false)
        {
            if (string.IsNullOrEmpty(settingsUrl))
                throw new ArgumentException("Url not specified.", nameof(settingsUrl));

            _settingsUrl = settingsUrl;
            _slackNotificationOptions = slackNotificationOptions;
            _configure = configure;
            _throwExceptionOnCheckError = throwExceptionOnCheckError;
        }

        protected override async Task<TSettings> Load()
        {
            var content = await HttpClientProvider.Client.GetStringAsync(_settingsUrl);
            var processingResult = await SettingsProcessor.ProcessForConfigurationAsync<TSettings>(content);
            var settings = processingResult.Item1;
            SetSettingsConfigurationRoot(processingResult.Item2);

            if (!_throwExceptionOnCheckError)
            {
                Task.Run(() => SettingsProcessor.CheckDependenciesAsync(settings, _slackNotificationOptions));
            }
            else
            {
                var errorMessages = await SettingsProcessor.CheckDependenciesAsync(settings,
                    _slackNotificationOptions);

                if (!string.IsNullOrEmpty(errorMessages))
                {
                    throw new Exception($"Services check failed:{Environment.NewLine}{errorMessages} ");
                }
            }
            
            _configure?.Invoke(settings);
            return settings;
        }
    }
}
