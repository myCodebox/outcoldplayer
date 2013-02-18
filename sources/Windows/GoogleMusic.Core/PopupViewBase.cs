// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using System.Diagnostics;

    using Windows.UI.Xaml.Controls.Primitives;

    public class PopupViewBase : ViewBase, IPopupView
    {
        public void Close()
        {
            var popup = this.Parent as Popup;
            Debug.Assert(popup != null, "popup != null");
            if (popup != null)
            {
                popup.IsOpen = false;
            }
        }
    }
}