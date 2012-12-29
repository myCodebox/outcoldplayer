// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    public class PlaylistsViewPresenter : PlaylistsViewPresenterBase<IPlaylistsView>
    {
        private readonly ISongsService songsService;

        public PlaylistsViewPresenter(
            IDependencyResolverContainer container,
            IPlaylistsView view,
            ISongsService songsService)
            : base(container, view)
        {
            this.songsService = songsService;
            this.BindingModel = new PlaylistsViewBindingModel();
        }

        public PlaylistsViewBindingModel BindingModel { get; private set; }

        public override void OnNavigatedTo(object parameter)
        {
            base.OnNavigatedTo(parameter);

            this.BindingModel.Playlists.Clear();
            this.BindingModel.Count = 0;

            if (parameter is PlaylistsRequest)
            {
                this.BindingModel.IsLoading = true;
                var playlistsRequest = (PlaylistsRequest)parameter;

                if (playlistsRequest == PlaylistsRequest.Albums)
                {
                    this.BindingModel.Title = "Albums";
                    this.songsService.GetAllAlbumsAsync().ContinueWith(
                        t =>
                            {
                                this.BindingModel.Count = t.Result.Count;
                                this.BindingModel.IsLoading = false;

                                foreach (var album in t.Result)
                                {
                                    this.BindingModel.Playlists.Add(new PlaylistBindingModel(album));
                                }
                            },
                        TaskScheduler.FromCurrentSynchronizationContext());
                }
                else if (playlistsRequest == PlaylistsRequest.Playlists)
                {
                    this.BindingModel.Title = "Playlists";
                    this.songsService.GetAllPlaylistsAsync().ContinueWith(
                        t =>
                            {
                                this.BindingModel.Count = t.Result.Count;
                                this.BindingModel.IsLoading = false;

                                foreach (var playlist in t.Result)
                                {
                                    this.BindingModel.Playlists.Add(new PlaylistBindingModel(playlist));
                                }
                            },
                        TaskScheduler.FromCurrentSynchronizationContext());
                }
                else if (playlistsRequest == PlaylistsRequest.Genres)
                {
                    this.BindingModel.Title = "Genres";
                    this.songsService.GetAllGenresAsync().ContinueWith(
                        t =>
                        {
                            this.BindingModel.Count = t.Result.Count;
                            this.BindingModel.IsLoading = false;

                            foreach (var playlist in t.Result)
                            {
                                this.BindingModel.Playlists.Add(new PlaylistBindingModel(playlist));
                            }
                        },
                        TaskScheduler.FromCurrentSynchronizationContext());
                }
                else
                {
                    this.BindingModel.Title = "Artists";
                    this.songsService.GetAllArtistsAsync().ContinueWith(
                        t =>
                        {
                            this.BindingModel.Count = t.Result.Count;
                            this.BindingModel.IsLoading = false;

                            foreach (var playlist in t.Result)
                            {
                                this.BindingModel.Playlists.Add(new PlaylistBindingModel(playlist));
                            }
                        },
                        TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
            
        }
    }
}