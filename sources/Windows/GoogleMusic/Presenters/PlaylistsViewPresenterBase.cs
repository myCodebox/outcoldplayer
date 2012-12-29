// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    public abstract class PlaylistsViewPresenterBase<TView> : ViewPresenterBase<TView> where TView : IView
    {
        private readonly INavigationService navigationService;
        private readonly ICurrentPlaylistService currentPlaylistService;

        protected PlaylistsViewPresenterBase(
            IDependencyResolverContainer container, 
            TView view)
            : base(container, view)
        {
            this.navigationService = container.Resolve<INavigationService>();
            this.currentPlaylistService = container.Resolve<ICurrentPlaylistService>();
        }

        public void ItemClick(PlaylistBindingModel playlistBindingModel)
        {
            this.Logger.Debug("ItemClick.");
            if (playlistBindingModel != null)
            {
                this.Logger.Debug("ItemClick. Playlist '{0}'.", playlistBindingModel.Playlist.Title);
                this.navigationService.NavigateTo<IPlaylistView>(playlistBindingModel.Playlist);
            }
        }

        public void StartPlaylist(PlaylistBindingModel playlistBindingModel)
        {
            if (playlistBindingModel != null)
            {
                var playlist = playlistBindingModel.Playlist;

                this.currentPlaylistService.ClearPlaylist();
                if (playlist.Songs.Count > 0)
                {
                    this.currentPlaylistService.AddSongs(playlist.Songs);
                    this.currentPlaylistService.PlayAsync(0);
                }

                this.navigationService.NavigateTo<IPlaylistView>(playlist);
            }
        }
    }
}