// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views.Popups
{
    using Windows.UI.Xaml.Input;

    public sealed partial class FullScreenPlayerPopupView : PopupViewBase, IFullScreenPlayerPopupView
    {
        public FullScreenPlayerPopupView()
        {
            this.InitializeComponent();
        }

        private void FullScreenPlayerPopupView_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (!e.Handled)
            {
                this.Close();
            }
        }
    }
}
