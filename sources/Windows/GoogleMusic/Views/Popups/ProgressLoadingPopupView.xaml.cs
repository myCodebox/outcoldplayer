// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views.Popups
{
    using OutcoldSolutions.Views;

    public interface IProgressLoadingPopupView : IPopupView
    {
    }

    public sealed partial class ProgressLoadingPopupView : PopupViewBase, IProgressLoadingPopupView
    {
        public ProgressLoadingPopupView()
        {
            this.InitializeComponent();
        }
    }
}
