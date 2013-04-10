// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Shell
{
    public interface ISearchService
    {
        void Activate();

        void SetShowOnKeyboardInput(bool value);

        void Register();

        void Unregister();
    }
}