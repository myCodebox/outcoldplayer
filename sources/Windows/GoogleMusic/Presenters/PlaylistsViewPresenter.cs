// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.WebServices;
    using OutcoldSolutions.GoogleMusic.WebServices.Models;

    public class PlaylistsViewPresenter : ViewPresenterBase<IPlaylistsView>
    {
        private const int MaxPlaylists = 12;

        private readonly IPlaylistsWebService playlistsWebService;
        private readonly INavigationService navigationService;
        private readonly ICurrentPlaylistService currentPlaylistService;

        public PlaylistsViewPresenter(
            IDependencyResolverContainer container,
            IPlaylistsView view,
            IPlaylistsWebService playlistsWebService,
            INavigationService navigationService,
            ICurrentPlaylistService currentPlaylistService)
            : base(container, view)
        {
            this.playlistsWebService = playlistsWebService;
            this.navigationService = navigationService;
            this.currentPlaylistService = currentPlaylistService;
            this.BindingModel = new PlaylistsViewBindingModel();

            this.Logger.Debug("Loading playlists.");
            this.playlistsWebService.GetAllPlaylistsAsync()
                .ContinueWith(
                task =>
                    {
                        var playlists = (task.Result.Playlists ?? Enumerable.Empty<GoogleMusicPlaylist>())
                            .Union(task.Result.MagicPlaylists ?? Enumerable.Empty<GoogleMusicPlaylist>()).ToList();

                        this.Logger.Debug("Playlists count {0}.", playlists.Count);
                        foreach (var playlist in playlists)
                        {
                            this.BindingModel.Playlists.Add(new PlaylistBindingModel(playlist));
                            if (this.BindingModel.Playlists.Count == MaxPlaylists)
                            {
                                break;
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

        public void StartPlaylist(PlaylistBindingModel playlistBindingModel)
        {
            if (playlistBindingModel != null)
            {
                var googleMusicPlaylist = playlistBindingModel.GetPlaylist();

                this.currentPlaylistService.ClearPlaylist();
                if (googleMusicPlaylist.Playlist != null)
                {
                    this.currentPlaylistService.AddSongs(googleMusicPlaylist.Playlist);
                    this.currentPlaylistService.PlayAsync(0);
                }

                this.navigationService.NavigateTo<IPlaylistView>(googleMusicPlaylist);
            }
        }
    }
}