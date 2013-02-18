// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    using System.Diagnostics;

    using OutcoldSolutions.GoogleMusic.BindingModels;

    using Windows.UI.Xaml.Controls;

    public interface IArtistPageView : IDataPageView
    {
    }

    public sealed partial class ArtistPageView : DataPageViewBase, IArtistPageView
    {
        public ArtistPageView()
        {
            this.InitializeComponent();
            this.TrackListViewBase(this.GridView);
        }

        private void PlaylistItemClick(object sender, ItemClickEventArgs e)
        {
            var album = e.ClickedItem as PlaylistBindingModel;

            Debug.Assert(album != null, "album != null");
            if (album != null)
            {
                this.NavigationService.NavigateToView<PlaylistViewResolver>(album.Playlist);
            }
        }
    }
}
