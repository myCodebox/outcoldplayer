// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    public interface ISearchService
    {
        void Register();

        void Unregister();

        void SetShowOnKeyboardInput(bool value);
    }
}