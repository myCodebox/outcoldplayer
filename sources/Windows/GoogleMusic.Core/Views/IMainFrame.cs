// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using System.Collections.Generic;
    using Windows.Foundation;

    /// <summary>
    /// The MainFrame interface.
    /// </summary>
    public interface IMainFrame : IView
    {
        string Title { get; set; }
        string Subtitle { get; set; }

        bool IsCurretView(IPageView view);

        /// <summary>
        /// Set view commands.
        /// </summary>
        /// <param name="commands">
        /// The commands.
        /// </param>
        void SetViewCommands(IEnumerable<CommandMetadata> commands);

        /// <summary>
        /// The clear view commands.
        /// </summary>
        void ClearViewCommands();

        /// <summary>
        /// Set context commands.
        /// </summary>
        /// <param name="commands">
        /// The commands.
        /// </param>
        void SetContextCommands(IEnumerable<CommandMetadata> commands);

        /// <summary>
        /// Clear context commands.
        /// </summary>
        void ClearContextCommands();

        /// <summary>
        /// Show popup.
        /// </summary>
        /// <param name="popupRegion">
        /// The popup region.
        /// </param>
        /// <param name="injections">
        /// The injections arguments.
        /// </param>
        /// <typeparam name="TPopup">
        /// The type of popup view.
        /// </typeparam>
        /// <returns>
        /// The <see cref="TPopup"/>.
        /// </returns>
        TPopup ShowPopup<TPopup>(PopupRegion popupRegion, params object[] injections) where TPopup : IPopupView;

        void ShowMessage(string text);

        Rect GetRectForSecondaryTileRequest();
    }
}