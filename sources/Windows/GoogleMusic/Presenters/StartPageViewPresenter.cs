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
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.Presenters;

    public class StartPageViewPresenter : DataPagePresenterBase<IStartPageView, StartViewBindingModel>
    {
        private const int MaxItems = 12;

        private readonly IPlayQueueService playQueueService;
        private readonly IPlaylistCollectionsService collectionsService;
        private readonly INavigationService navigationService;

        public StartPageViewPresenter(
            INavigationService navigationService,
            IPlayQueueService playQueueService,
            IPlaylistCollectionsService collectionsService)
        {
            this.playQueueService = playQueueService;
            this.collectionsService = collectionsService;
            this.navigationService = navigationService;

            this.PlayCommand = new DelegateCommand(this.Play);
        }

        public DelegateCommand PlayCommand { get; set; }

        protected override async Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs)
        {
            var groups = new List<PlaylistsGroupBindingModel>();

            groups.Add(new PlaylistsGroupBindingModel(
                null,
                await this.collectionsService.GetCollection<SystemPlaylist>().CountAsync(),
                (await this.collectionsService.GetCollection<SystemPlaylist>().GetAllAsync(Order.None)).Select(x => new PlaylistBindingModel(x) { PlayCommand = this.PlayCommand })));

            await this.LoadAndAdd<UserPlaylist>(groups, "Playlists", PlaylistsRequest.Playlists);
            await this.LoadAndAdd<Artist>(groups, "Artists", PlaylistsRequest.Artists);
            await this.LoadAndAdd<Album>(groups, "Albums", PlaylistsRequest.Albums);
            await this.LoadAndAdd<Genre>(groups, "Genres", PlaylistsRequest.Genres);

            this.BindingModel.Groups = groups;
        }

        private async Task LoadAndAdd<TPlaylist>(
            IList<PlaylistsGroupBindingModel> groups,
            string title, 
            PlaylistsRequest playlistsRequest) where TPlaylist : Playlist
        {
            var collection = this.collectionsService.GetCollection<TPlaylist>();
            var playlists = (await collection.GetAllAsync(Order.LastPlayed, MaxItems)).ToList();

            if (playlists.Count > 0)
            {
                groups.Add(new PlaylistsGroupBindingModel(
                    title,
                    await collection.CountAsync(),
                    playlists.Select(x => new PlaylistBindingModel(x) { PlayCommand = this.PlayCommand }),
                    playlistsRequest));
            }
        }

        private void Play(object commandParameter)
        {
            Playlist playlist = commandParameter as Playlist;
            if (playlist != null)
            {
                this.playQueueService.PlayAsync(playlist);
                this.navigationService.NavigateTo<IPlaylistPageView>(playlist);
                this.Toolbar.IsBottomAppBarOpen = true;
            }
        }
    }
}