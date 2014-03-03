// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views.Popups
{
    public interface IPlayerMorePopupView : IPopupView
    {
    }

    public sealed partial class PlayerMorePopupView : PopupViewBase, IPlayerMorePopupView
    {
        public PlayerMorePopupView()
        {
            this.InitializeComponent();
        }
    }
}
