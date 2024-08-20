using System;
using System.IO;
using System.Threading.Tasks;
using Lykke.SettingsReader.ReloadingManager.Configuration;

namespace Lykke.SettingsReader
{
    public class LocalSettingsReloadingManager<TSettings> : ReloadingManagerWithConfigurationBase<TSettings>
    {
        private readonly string _path;
        private readonly Action<SlackNotificationOptions<TSettings>> _slackNotificationOptions;
        private readonly Action<TSettings> _configure;
        private readonly bool _throwExceptionOnCheckError;

        public LocalSettingsReloadingManager(string path,
            Action<SlackNotificationOptions<TSettings>> slackNotificationOptions,
            Action<TSettings> configure = null,
            bool throwExceptionOnCheckError = false)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("Path not specified.", nameof(path));

            _path = path;
            _slackNotificationOptions = slackNotificationOptions;
            _configure = configure;
            _throwExceptionOnCheckError = throwExceptionOnCheckError;
        }

        protected override async Task<TSettings> Load()
        {
            using (var reader = File.OpenText(_path))
            {
                var content = await reader.ReadToEndAsync();
                var processingResult = await SettingsProcessor.ProcessForConfigurationAsync<TSettings>(content);
                SetSettingsConfigurationRoot(processingResult.Item2);

                if (!_throwExceptionOnCheckError)
                {
                    Task.Run(() => SettingsProcessor.CheckDependenciesAsync(processingResult.Item1, _slackNotificationOptions));
                }
                else
                {
                    var errorMessages = await SettingsProcessor.CheckDependenciesAsync(processingResult.Item1,
                        _slackNotificationOptions);

                    if (!string.IsNullOrEmpty(errorMessages))
                    {
                        throw new Exception($"Services check failed:{Environment.NewLine}{errorMessages} ");
                    }
                }

                _configure?.Invoke(processingResult.Item1);
                return processingResult.Item1;
            }
        }
    }
}
