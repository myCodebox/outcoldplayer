// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    public interface ISettingsService
    {
        void SetValue<T>(string key, T value);

        T GetValue<T>(string key, T defaultValue = default(T));

        void RemoveValue(string key);

        void SetRoamingValue<T>(string key, T value);

        T GetRoamingValue<T>(string key, T defaultValue = default(T));

        void RemoveRoamingValue(string key);
    }
}