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

    public class StartViewPresenter : PlaylistsViewPresenterBase<IStartView>
    {
        private const int MaxItems = 12;

        private readonly ISongsService songsService;

        public StartViewPresenter(
            IDependencyResolverContainer container, 
            IStartView view,
            ISongsService songsService)
            : base(container, view)
        {
            this.songsService = songsService;

            this.BindingModel = new StartViewBindingModel();
        }

        public StartViewBindingModel BindingModel { get; private set; }

        public override void OnNavigatedTo(object parameter)
        {
            base.OnNavigatedTo(parameter);

            this.BindingModel.Playlists.Clear();
            this.BindingModel.Artists.Clear();
            this.BindingModel.Albums.Clear();
            this.BindingModel.Genres.Clear();

            this.BindingModel.IsLoadingPlaylists = true;
            this.BindingModel.IsLoadingAlbums = true;
            this.BindingModel.IsLoadingGenres = true;
            this.BindingModel.IsLoadingArtists = true;

            this.BindingModel.PlaylistsCount = 0;
            this.BindingModel.AlbumsCount = 0;
            this.BindingModel.ArtistsCount = 0;
            this.BindingModel.GenresCount = 0;

            this.Logger.Debug("Loading playlists.");
            this.songsService.GetAllPlaylistsAsync(Order.LastPlayed).ContinueWith(
                task =>
                {
                    this.Logger.Debug("Playlists count {0}.", task.Result.Count);

                    this.BindingModel.PlaylistsCount = task.Result.Count;

                    foreach (var playlist in task.Result.Take(MaxItems))
                    {
                        this.BindingModel.Playlists.Add(new PlaylistBindingModel(playlist));
                    }

                    this.BindingModel.IsLoadingPlaylists = false;
                },
                TaskScheduler.FromCurrentSynchronizationContext());

            this.songsService.GetAllAlbumsAsync(Order.LastPlayed).ContinueWith(
                task =>
                {
                    this.Logger.Debug("Albums count {0}.", task.Result.Count);

                    this.BindingModel.AlbumsCount = task.Result.Count;

                    foreach (var playlist in task.Result.Take(MaxItems))
                    {
                        this.BindingModel.Albums.Add(new PlaylistBindingModel(playlist));
                    }

                    this.BindingModel.IsLoadingAlbums = false;
                },
                TaskScheduler.FromCurrentSynchronizationContext());

            this.songsService.GetAllGenresAsync(Order.LastPlayed).ContinueWith(
                task =>
                {
                    this.Logger.Debug("Genres count {0}.", task.Result.Count);

                    this.BindingModel.GenresCount = task.Result.Count;

                    foreach (var playlist in task.Result.Take(MaxItems))
                    {
                        this.BindingModel.Genres.Add(new PlaylistBindingModel(playlist));
                    }

                    this.BindingModel.IsLoadingGenres = false;
                },
                TaskScheduler.FromCurrentSynchronizationContext());

            this.songsService.GetAllArtistsAsync(Order.LastPlayed).ContinueWith(
                task =>
                {
                    this.Logger.Debug("Artists count {0}.", task.Result.Count);

                    this.BindingModel.ArtistsCount = task.Result.Count;

                    foreach (var playlist in task.Result.Take(MaxItems))
                    {
                        this.BindingModel.Artists.Add(new PlaylistBindingModel(playlist));
                    }

                    this.BindingModel.IsLoadingArtists = false;
                },
                TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}