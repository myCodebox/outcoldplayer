// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    using Windows.ApplicationModel.Search;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    public sealed partial class LinksRegionContent : UserControl
    {
        public LinksRegionContent()
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
