//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using System.Diagnostics;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Presenters;
    using OutcoldSolutions.Views;

    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    public interface IStartPageView : IPageView
    {
    }

    public sealed partial class StartPageView : PageViewBase, IStartPageView
    {
        public StartPageView()
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
        
        private void NavigateTo(object sender, RoutedEventArgs e)
        {
            var frameworkElement = sender as FrameworkElement;
            if (frameworkElement != null)
            {
                var groupBindingModel = frameworkElement.DataContext as PlaylistsGroupBindingModel;
                if (groupBindingModel != null)
                {
                    if (groupBindingModel.Request == PlaylistType.UserPlaylist)
                    {
                        this.NavigationService.NavigateTo<IUserPlaylistsPageView>(PlaylistType.UserPlaylist);
                    }
                    else
                    {
                        this.NavigationService.NavigateTo<IPlaylistsPageView>(groupBindingModel.Request);
                    }
                }
            }
        }
    }
}
