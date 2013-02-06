// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    public class StartViewPresenter : PlaylistsViewPresenterBase<IStartView>
    {
        private const int MaxItems = 12;

        private readonly ISongsService songsService;

        private readonly IPlaylistCollectionsService collectionsService;

        public StartViewPresenter(
            IDependencyResolverContainer container, 
            IStartView view,
            ISongsService songsService,
            IPlaylistCollectionsService collectionsService)
            : base(container, view)
        {
            this.songsService = songsService;
            this.collectionsService = collectionsService;

            this.BindingModel = new StartViewBindingModel();
        }

        public StartViewBindingModel BindingModel { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs eventArgs)
        {
            base.OnNavigatedTo(eventArgs);

            this.View.SetGroups(null);
            this.BindingModel.IsLoading = true;

            this.Logger.Debug("Loading playlists.");
            this.GetGroupsAsync().ContinueWith(
                task =>
                    {
                        if (task.IsCompleted && !task.IsFaulted)
                        {
                            this.View.SetGroups(task.Result);
                        }

                        this.BindingModel.IsLoading = false;
                    },
                TaskScheduler.FromCurrentSynchronizationContext());
        }

        public async Task<List<PlaylistsGroupBindingModel>> GetGroupsAsync()
        {
            var groups = new List<PlaylistsGroupBindingModel>();

            var systemPlaylists = await this.songsService.GetSystemPlaylists();
            groups.Add(new PlaylistsGroupBindingModel(null, systemPlaylists.Count, systemPlaylists.Select(x => new PlaylistBindingModel(x))));

            var playlists = await this.songsService.GetAllPlaylistsAsync(Order.LastPlayed, canReload: true);
            groups.Add(new PlaylistsGroupBindingModel("Playlists", playlists.Count, playlists.Take(MaxItems).Select(x => new PlaylistBindingModel(x)), PlaylistsRequest.Playlists));

            var artists = (await this.collectionsService.GetArtistCollection().GetAllAsync(Order.LastPlayed)).ToList();
            groups.Add(new PlaylistsGroupBindingModel("Artists", artists.Count, artists.Take(MaxItems).Select(x => new PlaylistBindingModel(x)), PlaylistsRequest.Artists));

            var albums = (await this.collectionsService.GetAlbumCollection().GetAllAsync(Order.LastPlayed)).ToList();
            groups.Add(new PlaylistsGroupBindingModel("Albums", albums.Count, albums.Take(MaxItems).Select(x => new PlaylistBindingModel(x)), PlaylistsRequest.Albums));

            var genres = (await this.collectionsService.GetGenreCollection().GetAllAsync(Order.LastPlayed)).ToList();
            groups.Add(new PlaylistsGroupBindingModel("Genres", genres.Count, genres.Take(MaxItems).Select(x => new PlaylistBindingModel(x)), PlaylistsRequest.Genres));

            return groups;
        }
    }
}