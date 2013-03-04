// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Shell
{
    using System;

    public interface ISearchService
    {
        event EventHandler IsRegisteredChanged;

        bool IsRegistered { get; }

        void Activate();

        void SetShowOnKeyboardInput(bool value);
    }
}