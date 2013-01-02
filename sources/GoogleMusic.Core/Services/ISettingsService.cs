// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;

    public interface ISettingsService
    {
        event EventHandler<SettingsValueChangedEventArgs> ValueChanged;

        void SetValue<T>(string key, T value);

        T GetValue<T>(string key, T defaultValue = default(T));
    }
}