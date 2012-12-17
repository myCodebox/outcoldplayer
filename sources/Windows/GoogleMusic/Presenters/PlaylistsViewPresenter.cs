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

        public PlaylistsViewPresenter(
            IDependencyResolverContainer container,
            IPlaylistsView view,
            IPlaylistsWebService playlistsWebService)
            : base(container, view)
        {
            this.playlistsWebService = playlistsWebService;
            this.BindingModel = new PlaylistsViewBindingModel();

            this.playlistsWebService.GetAllPlaylistsAsync()
                .ContinueWith(
                task =>
                    {
                        var playlists = task.Result;
                        if (playlists.Playlists != null)
                        {
                            foreach (var playlist in playlists.Playlists)
                            {
                                this.BindingModel.Playlists.Add(new PlaylistBindingModel(playlist));
                            }
                        }

                        if (playlists.MagicPlaylists != null)
                        {
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
    }
}