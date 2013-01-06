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

            this.View.SetGroups(null);
            this.BindingModel.IsLoading = true;

            this.Logger.Debug("Loading playlists.");
            this.GetGroupsAsync().ContinueWith(
                task =>
                    {
                        this.View.SetGroups(task.Result);
                        this.BindingModel.IsLoading = false;
                    },
                TaskScheduler.FromCurrentSynchronizationContext());
        }

        public async Task<List<GroupBindingModel>> GetGroupsAsync()
        {
            var groups = new List<GroupBindingModel>();

            var playlists = await this.songsService.GetAllPlaylistsAsync(Order.LastPlayed);
            groups.Add(new GroupBindingModel("Playlists", playlists.Count, PlaylistsRequest.Playlists, playlists.Take(MaxItems).Select(x => new PlaylistBindingModel(x))));

            var artists = await this.songsService.GetAllArtistsAsync(Order.LastPlayed);
            groups.Add(new GroupBindingModel("Artists", artists.Count, PlaylistsRequest.Artists, artists.Take(MaxItems).Select(x => new PlaylistBindingModel(x))));

            var albums = await this.songsService.GetAllAlbumsAsync(Order.LastPlayed);
            groups.Add(new GroupBindingModel("Albums", albums.Count, PlaylistsRequest.Albums, albums.Take(MaxItems).Select(x => new PlaylistBindingModel(x))));

            var genres = await this.songsService.GetAllGenresAsync(Order.LastPlayed);
            groups.Add(new GroupBindingModel("Genres", genres.Count, PlaylistsRequest.Genres, genres.Take(MaxItems).Select(x => new PlaylistBindingModel(x))));

            return groups;
        }
    }
}