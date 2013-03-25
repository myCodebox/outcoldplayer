// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    using System.Diagnostics;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.Views;

    using Windows.UI.Xaml.Controls;

    public interface IArtistPageView : IPageView
    {
    }

    public sealed partial class ArtistPageView : PageViewBase, IArtistPageView
    {
        public ArtistPageView()
        {
            this.InitializeComponent();
            this.TrackItemsControl(this.GridView);
        }

        private void PlaylistItemClick(object sender, ItemClickEventArgs e)
        {
            var album = e.ClickedItem as PlaylistBindingModel;

            Debug.Assert(album != null, "album != null");
            if (album != null)
            {
                this.NavigationService.NavigateToPlaylist(album.Playlist);
            }
        }
    }
}
