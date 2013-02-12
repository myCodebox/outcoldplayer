// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    public interface ISettingsCommands
    {
        void Register();

        void Unregister();

        void ActivateSettings(string name);
    }
}