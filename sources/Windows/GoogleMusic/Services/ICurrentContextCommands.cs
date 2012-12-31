// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System.Collections.Generic;

    using Windows.UI.Xaml;

    public interface ICurrentContextCommands
    {
        void SetCommands(IEnumerable<UIElement> buttons);

        void ClearContext();
    }
}