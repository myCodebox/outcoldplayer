// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views.Popups
{
    using OutcoldSolutions.Views;

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
