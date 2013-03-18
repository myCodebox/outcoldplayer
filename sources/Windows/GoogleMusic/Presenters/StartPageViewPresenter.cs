// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Repositories.DbModels;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.Presenters;

    public class StartPageViewPresenter : DataPagePresenterBase<IStartPageView, StartViewBindingModel>
    {
        private const int MaxItems = 12;

        private readonly ISongsQueueService songsQueueService;

        private readonly INavigationService navigationService;

        private readonly ISystemPlaylistRepository systemPlaylistRepository;
        private readonly IArtistsRepository artistsRepository;
        private readonly IAlbumsRepository albumsRepository;
        private readonly IGenresRepository genresRepository;
        private readonly IUserPlaylistRepository userPlaylistRepository;

        public StartPageViewPresenter(
            INavigationService navigationService,
            ISongsQueueService songsQueueService,
            ISystemPlaylistRepository systemPlaylistRepository,
            IArtistsRepository artistsRepository,
            IAlbumsRepository albumsRepository,
            IGenresRepository genresRepository,
            IUserPlaylistRepository userPlaylistRepository)
        {
            this.songsQueueService = songsQueueService;
            this.systemPlaylistRepository = systemPlaylistRepository;
            this.artistsRepository = artistsRepository;
            this.albumsRepository = albumsRepository;
            this.genresRepository = genresRepository;
            this.userPlaylistRepository = userPlaylistRepository;
            this.navigationService = navigationService;

            this.PlayCommand = new DelegateCommand(this.Play);
        }

        public DelegateCommand PlayCommand { get; set; }

        protected override async Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs)
        {
            var groups = new List<GroupPlaylistsGroupBindingModel>();
            groups.AddRange(await Task.WhenAll(
                this.SystemPlaylistsAsync(), 
                this.UserPlaylistsAsync(), 
                this.ArtistsAsync(), 
                this.AlbumsAsync(), 
                this.GenresAsync()));
            this.BindingModel.Groups = groups;
        }

        private async Task<GroupPlaylistsGroupBindingModel> SystemPlaylistsAsync()
        {
            var systemPlaylists = await this.systemPlaylistRepository.GetSystemPlaylistsAsync();

            return this.CreateGroup(null, systemPlaylists.Count, systemPlaylists, SongsContainerType.Unknown);
        }

        private async Task<GroupPlaylistsGroupBindingModel> ArtistsAsync()
        {
            int artistsCount = await this.artistsRepository.GetCountAsync();
            var artists = await this.artistsRepository.GetAristsAsync(Order.LastPlayed, take: MaxItems);

            return this.CreateGroup("Artists", artistsCount, artists, SongsContainerType.Artist);
        }

        private async Task<GroupPlaylistsGroupBindingModel> AlbumsAsync()
        {
            var albumsCount = await this.albumsRepository.GetCountAsync();
            var albums = await this.albumsRepository.GetAlbumsAsync(Order.LastPlayed, take: MaxItems);

            return this.CreateGroup("Albums", albumsCount, albums, SongsContainerType.Album);
        }

        private async Task<GroupPlaylistsGroupBindingModel> GenresAsync()
        {
            int genresCount = await this.genresRepository.GetCountAsync();
            var genres = await this.genresRepository.GetGenresAsync(Order.LastPlayed, take: MaxItems);

            return this.CreateGroup("Genres", genresCount, genres, SongsContainerType.Genre);
        }

        private async Task<GroupPlaylistsGroupBindingModel> UserPlaylistsAsync()
        {
            int userPlaylistsCount = await this.userPlaylistRepository.GetCountAsync();
            var playlists = await this.userPlaylistRepository.GetPlaylistsAsync(Order.LastPlayed, take: MaxItems);

            return this.CreateGroup("Playlists", userPlaylistsCount, playlists, SongsContainerType.UserPlaylist);
        }

        private GroupPlaylistsGroupBindingModel CreateGroup(string title, int userPlaylistsCount, IEnumerable<ISongsContainer> playlists, SongsContainerType type)
        {
            List<GroupPlaylistBindingModel> groupItems =
                playlists.Select(
                    playlist =>
                    new GroupPlaylistBindingModel(playlist)
                        {
                            PlayCommand = this.PlayCommand
                        }).ToList();

            return new GroupPlaylistsGroupBindingModel(
                title,
                userPlaylistsCount,
                groupItems,
                type);
        }

        private void Play(object commandParameter)
        {
            ISongsContainer playlist = commandParameter as ISongsContainer;
            if (playlist != null)
            {
                this.Toolbar.IsBottomAppBarOpen = true;
                this.Logger.LogTask(this.songsQueueService.PlayAsync(playlist));
                this.navigationService.NavigateToPlaylist(playlist);
            }
        }
    }
}