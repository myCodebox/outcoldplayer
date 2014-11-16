// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    public interface ISettingsService
    {
        void SetApplicationValue<T>(string key, T value);

        T GetApplicationValue<T>(string key, T defaultValue = default(T));

        void RemoveApplicationValue(string key);

        void SetValue<T>(string containerName, string key, T value);

        T GetValue<T>(string containerName, string key, T defaultValue = default(T));

        bool TryGetValue<T>(string containerName, string key, out T value);

        void RemoveValue(string containerName, string key);

        void SetRoamingValue<T>(string key, T value);

        T GetRoamingValue<T>(string key, T defaultValue = default(T));

        void RemoveRoamingValue(string key);
    }
}