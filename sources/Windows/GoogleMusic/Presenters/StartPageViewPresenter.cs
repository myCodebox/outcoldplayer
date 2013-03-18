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

        private readonly INavigationService navigationService;

        private readonly ISystemPlaylistRepository systemPlaylistRepository;
        private readonly IArtistsRepository artistsRepository;
        private readonly IAlbumsRepository albumsRepository;
        private readonly IGenresRepository genresRepository;
        private readonly IUserPlaylistRepository userPlaylistRepository;

        public StartPageViewPresenter(
            INavigationService navigationService,
            IPlayQueueService playQueueService,
            ISystemPlaylistRepository systemPlaylistRepository,
            IArtistsRepository artistsRepository,
            IAlbumsRepository albumsRepository,
            IGenresRepository genresRepository,
            IUserPlaylistRepository userPlaylistRepository)
        {
            this.playQueueService = playQueueService;
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
            return new GroupPlaylistsGroupBindingModel(null, systemPlaylists.Count, systemPlaylists.Select(x => new GroupPlaylistBindingModel(x)).ToList());
        }

        private async Task<GroupPlaylistsGroupBindingModel> ArtistsAsync()
        {
            int artistsCount = await this.artistsRepository.GetCountAsync();
            var artists = await this.artistsRepository.GetAristsAsync(Order.LastPlayed, take: MaxItems);
            return new GroupPlaylistsGroupBindingModel(
                "Artists",
                artistsCount,
                artists.Select(x => new GroupPlaylistBindingModel(x)).ToList(),
                PlaylistsRequest.Artists);
        }

        private async Task<GroupPlaylistsGroupBindingModel> AlbumsAsync()
        {
            var albumsCount = await this.albumsRepository.GetCountAsync();
            var albums = await this.albumsRepository.GetAlbumsAsync(Order.LastPlayed, take: MaxItems);

            return new GroupPlaylistsGroupBindingModel(
                "Albums",
                albumsCount,
                albums.Select(x => new GroupPlaylistBindingModel(x)).ToList(),
                PlaylistsRequest.Albums);
        }

        private async Task<GroupPlaylistsGroupBindingModel> GenresAsync()
        {
            int genresCount = await this.genresRepository.GetCountAsync();
            var genres = await this.genresRepository.GetGenresAsync(Order.LastPlayed, take: MaxItems);

            return new GroupPlaylistsGroupBindingModel(
                "Genres",
                genresCount,
                genres.Select(x => new GroupPlaylistBindingModel(x)).ToList(),
                PlaylistsRequest.Genres);
        }

        private async Task<GroupPlaylistsGroupBindingModel> UserPlaylistsAsync()
        {
            int userPlaylistsCount = await this.userPlaylistRepository.GetCountAsync();
            var playlists = await this.userPlaylistRepository.GetPlaylistsAsync(Order.LastPlayed, take: MaxItems);

            return new GroupPlaylistsGroupBindingModel(
                "Playlists",
                userPlaylistsCount,
                playlists.Select(x => new GroupPlaylistBindingModel(x)).ToList(),
                PlaylistsRequest.Playlists);
        }

        private void Play(object commandParameter)
        {
            PlaylistBaseBindingModel playlist = commandParameter as PlaylistBaseBindingModel;
            if (playlist != null)
            {
                this.playQueueService.PlayAsync(playlist);
                this.navigationService.NavigateTo<IPlaylistPageView>(playlist);
                this.Toolbar.IsBottomAppBarOpen = true;
            }
        }
    }
}