// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System.Collections.Generic;

    using Windows.UI.Xaml.Controls.Primitives;

    public interface ICurrentContextCommands
    {
        void SetCommands(IEnumerable<ButtonBase> buttons);

        void ClearContext();
    }
}