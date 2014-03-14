// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views.Popups
{
    public interface IReadMorePopup : IPopupView
    {
    }

    public sealed partial class ReadMorePopup : PopupViewBase, IReadMorePopup
    {
        public ReadMorePopup()
        {
            this.InitializeComponent();
        }
    }
}
