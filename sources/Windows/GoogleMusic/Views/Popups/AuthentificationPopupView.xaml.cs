// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views.Popups
{
    public interface IAuthentificationPopupView : IPopupView
    {
    }

    public sealed partial class AuthentificationPopupView : PopupViewBase, IAuthentificationPopupView
    {
        public AuthentificationPopupView()
        {
            this.InitializeComponent();
        }
    }
}
