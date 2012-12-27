// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.WebServices;

    public class PlaylistsViewPresenter : ViewPresenterBase<IPlaylistsView>
    {
        private readonly IPlaylistsWebService playlistsWebService;
        private readonly INavigationService navigationService;

        public PlaylistsViewPresenter(
            IDependencyResolverContainer container,
            IPlaylistsView view,
            IPlaylistsWebService playlistsWebService,
            INavigationService navigationService)
            : base(container, view)
        {
            this.playlistsWebService = playlistsWebService;
            this.navigationService = navigationService;
            this.BindingModel = new PlaylistsViewBindingModel();

            this.Logger.Debug("Loading playlists.");
            this.playlistsWebService.GetAllPlaylistsAsync()
                .ContinueWith(
                task =>
                    {
                        var playlists = task.Result;
                        if (playlists.Playlists != null)
                        {
                            this.Logger.Debug("Playlists are not null. Count {0}.", playlists.Playlists.Count);
                            foreach (var playlist in playlists.Playlists)
                            {
                                this.BindingModel.Playlists.Add(new PlaylistBindingModel(playlist));
                            }
                        }

                        if (playlists.MagicPlaylists != null)
                        {
                            this.Logger.Debug("MagicPlaylists are not null. Count {0}.", playlists.MagicPlaylists.Count);
                            foreach (var playlist in playlists.MagicPlaylists)
                            {
                                this.BindingModel.Playlists.Add(new PlaylistBindingModel(playlist));
                            }
                        }

                        this.BindingModel.IsLoading = false;
                    }, 
                    TaskScheduler.FromCurrentSynchronizationContext());
        }

        public PlaylistsViewBindingModel BindingModel { get; private set; }

        public void ItemClick(PlaylistBindingModel playlistBindingModel)
        {
            this.Logger.Debug("ItemClick.");
            if (playlistBindingModel != null)
            {
                this.Logger.Debug("ItemClick. Playlist '{0}'.", playlistBindingModel.Title);
                this.navigationService.NavigateTo<IPlaylistView>(playlistBindingModel.GetPlaylist());
            }
        }
    }
}