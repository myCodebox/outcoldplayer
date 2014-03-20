// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views.Popups
{
    using Windows.UI.Xaml;

    public interface ITutorialPopupView : IPopupView
    {
    }

    public sealed partial class TutorialPopupView : PopupViewBase, ITutorialPopupView
    {
        public TutorialPopupView()
        {
            this.InitializeComponent();
        }

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
