// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    using OutcoldSolutions.Views;

    using Windows.ApplicationModel.Search;
    using Windows.UI.Xaml;

    public sealed partial class LinksRegionView : ViewBase
    {
        public LinksRegionView()
        {
            this.InitializeComponent();
        }

        private void ShowSearch(object sender, RoutedEventArgs e)
        {
            // Should hide it when collection is not loaded yet.
            SearchPane.GetForCurrentView().Show();
        }
    }
}
