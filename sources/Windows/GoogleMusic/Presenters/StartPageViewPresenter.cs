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

        private readonly ICurrentPlaylistService currentPlaylistService;

        private readonly IPlaylistCollectionsService collectionsService;

        private readonly INavigationService navigationService;

        public StartPageViewPresenter(
            IDependencyResolverContainer container, 
            ICurrentPlaylistService currentPlaylistService,
            IPlaylistCollectionsService collectionsService)
            : base(container)
        {
            this.currentPlaylistService = currentPlaylistService;
            this.collectionsService = collectionsService;

            this.navigationService = container.Resolve<INavigationService>();

            this.PlayCommand = new DelegateCommand(this.Play);
        }

        public DelegateCommand PlayCommand { get; set; }

        protected override async Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs)
        {
            var groups = new List<PlaylistsGroupBindingModel>();

            // TODO: PlaylistsRequest should be null
            groups.Add(new PlaylistsGroupBindingModel(
                null,
                await this.collectionsService.GetCollection<SystemPlaylist>().CountAsync(),
                (await this.collectionsService.GetCollection<SystemPlaylist>().GetAllAsync(Order.None)).Select(x => new PlaylistBindingModel(x) { PlayCommand = this.PlayCommand }),
                PlaylistsRequest.Albums));
            groups.Add(await this.GetGroupAsync<MusicPlaylist>("Playlists", PlaylistsRequest.Playlists));
            groups.Add(await this.GetGroupAsync<Artist>("Artists", PlaylistsRequest.Artists));
            groups.Add(await this.GetGroupAsync<Album>("Albums", PlaylistsRequest.Albums));
            groups.Add(await this.GetGroupAsync<Genre>("Genres", PlaylistsRequest.Genres));

            this.BindingModel.Groups = groups;
        }

        private async Task<PlaylistsGroupBindingModel> GetGroupAsync<TPlaylist>(string title, PlaylistsRequest playlistsRequest) where TPlaylist : Playlist
        {
            var collection = this.collectionsService.GetCollection<TPlaylist>();
            var playlists = (await collection.GetAllAsync(Order.LastPlayed, MaxItems)).ToList();
            return new PlaylistsGroupBindingModel(
                title,
                await collection.CountAsync(),
                playlists.Select(x => new PlaylistBindingModel(x) { PlayCommand = this.PlayCommand }),
                playlistsRequest);
        }

        private void Play(object commandParameter)
        {
            Playlist playlist = commandParameter as Playlist;
            if (playlist != null)
            {
                this.currentPlaylistService.ClearPlaylist();
                if (playlist.Songs.Count > 0)
                {
                    this.currentPlaylistService.SetPlaylist(playlist);
                    this.currentPlaylistService.PlayAsync();
                }

                this.navigationService.NavigateTo<IPlaylistPageView>(playlist);
            }
        }
    }
}