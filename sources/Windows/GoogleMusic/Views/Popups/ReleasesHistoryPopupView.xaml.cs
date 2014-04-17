// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views.Popups
{
    public interface IReleasesHistoryPopupView : IPopupView
    {
    }

    public sealed partial class ReleasesHistoryPopupView : PopupViewBase, IReleasesHistoryPopupView
    {
        public ReleasesHistoryPopupView()
        {
            this.InitializeComponent();
        }
    }
}
