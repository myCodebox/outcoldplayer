// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;

    using OutcoldSolutions.GoogleMusic.Diagnostics;

    using Windows.Storage;

    public class SettingsService : ISettingsService
    {
        private const string SettingsContainerKey = "Settings";
        private readonly ApplicationDataContainer settingsContainer;
        
        private readonly ILogger logger;

        public SettingsService(ILogManager logManager)
        {
            this.logger = logManager.CreateLogger("SettingsService");
            var localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Containers.ContainsKey(SettingsContainerKey))
            {
                this.settingsContainer = localSettings.Containers[SettingsContainerKey];
            }
            else
            {
                this.settingsContainer = localSettings.CreateContainer(
                    SettingsContainerKey, ApplicationDataCreateDisposition.Always);
            }
        }

        public event EventHandler<SettingsValueChangedEventArgs> ValueChanged;

        public void SetValue<T>(string key, T value)
        {
            this.logger.Debug("Setting value of key '{0}' to '{1}.'", key, value);
            this.settingsContainer.Values[key] = value;
            this.RaiseValueChanged(key);
        }

        public T GetValue<T>(string key, T defaultValue = default(T))
        {
            this.logger.Debug("Getting value of key '{0}'", key);
            if (this.settingsContainer.Values.ContainsKey(key))
            {
                try
                {
                    return (T)this.settingsContainer.Values[key];
                }
                catch (Exception e)
                {
                    this.logger.LogErrorException(e);
                }
            }

            return defaultValue;
        }

        protected virtual void RaiseValueChanged(string key)
        {
            var handler = this.ValueChanged;
            if (handler != null)
            {
                handler(this, new SettingsValueChangedEventArgs(key));
            }
        }
    }
}